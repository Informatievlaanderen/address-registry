namespace AddressRegistry.Consumer
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Simple;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Simple.Extensions;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Confluent.Kafka;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Projections;
    using Serilog;

    public class Consumer
    {
        private readonly ILifetimeScope _container;
        private readonly ILoggerFactory _loggerFactory;
        private readonly KafkaOptions _options;
        private readonly ConsumerOptions _consumerOptions;
        private readonly Offset? _offset;
        private readonly ILogger<Consumer> _logger;

        public Consumer(
            ILifetimeScope container,
            ILoggerFactory loggerFactory,
            KafkaOptions options,
            ConsumerOptions consumerOptions,
            Offset? offset)
        {
            _container = container;
            _loggerFactory = loggerFactory;
            _options = options;
            _consumerOptions = consumerOptions;
            _offset = offset;

            _logger = _loggerFactory.CreateLogger<Consumer>();
        }

        public Task<Result<KafkaJsonMessage>> Start(CancellationToken cancellationToken = default)
        {
            Task.Yield().GetAwaiter().GetResult();
            var commandHandler = new CommandHandler(_container, _loggerFactory);
            var projector = new ConnectedProjector<CommandHandler>(Resolve.WhenEqualToHandlerMessageType(new StreetNameKafkaProjection().Handlers));

            var consumerGroupId = $"{nameof(AddressRegistry)}.{nameof(Consumer)}.{_consumerOptions.Topic}{_consumerOptions.ConsumerGroupSuffix}";
            return KafkaConsumerV2.Consume(
                _options,
                consumerGroupId,
                _consumerOptions.Topic,
                async message =>
                {
                    _logger.LogInformation("Handling next message");
                    await projector.ProjectAsync(commandHandler, message, cancellationToken);
                },
                _offset,
                cancellationToken);

            //if (!result.IsSuccess)
            //{
            //    var logger = _loggerFactory.CreateLogger<Consumer>();
            //    logger.LogCritical($"Consumer group {consumerGroupId} could not consume from topic {_consumerOptions.Topic}");
            //}
        }
    }

    public static class KafkaConsumerV2
    {
        public static async Task<Result<KafkaJsonMessage>> Consume(
            KafkaOptions options,
            string consumerGroupId,
            string topic,
            Func<object, Task> messageHandler,
            Offset? offset = null,
            CancellationToken cancellationToken = default)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = options.BootstrapServers,
                GroupId = consumerGroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            }.WithAuthentication(options);

            var serializer = JsonSerializer.CreateDefault(options.JsonSerializerSettings);

            var consumerBuilder = new ConsumerBuilder<Ignore, string>(config)
                .SetValueDeserializer(Deserializers.Utf8);
            if (offset.HasValue)
            {
                consumerBuilder.SetPartitionsAssignedHandler((cons, topicPartitions) =>
                {
                    var partitionOffset = topicPartitions.Select(x => new TopicPartitionOffset(x.Topic, x.Partition, offset.Value));
                    return partitionOffset;
                });
            }

            var kafkaJsonMessage = new KafkaJsonMessage("", "");
            using var consumer = consumerBuilder.Build();
            try
            {
                Log.Information("Subscribing to topic");
                consumer.Subscribe(topic);
                Log.Information("Subscribed to topic");

                while (!cancellationToken.IsCancellationRequested)
                {
                    Log.Information("Consuming topic");
                    var consumeResult = consumer.Consume(TimeSpan.FromSeconds(3));
                    if (consumeResult == null) //if no message is found, returns null
                    {
                        //await Task.Delay(1000, cancellationToken);
                        continue;
                    }

                    Log.Information("Consuming message");
                    kafkaJsonMessage = serializer.Deserialize<KafkaJsonMessage>(consumeResult.Message.Value) ?? throw new ArgumentException("Kafka json message is null.");
                    var messageData = kafkaJsonMessage.Map() ?? throw new ArgumentException("Kafka message data is null.");

                    await messageHandler(messageData);
                    Log.Information("Committing message");
                    consumer.Commit(consumeResult);
                }

                return Result<KafkaJsonMessage>.Success(kafkaJsonMessage);
            }
            catch (ConsumeException ex)
            {
                Log.Error(ex, "ConsumeException");
                return Result<KafkaJsonMessage>.Failure(ex.Error.Code.ToString(), ex.Error.Reason);
            }
            catch (OperationCanceledException ex)
            {
                Log.Error(ex, "OperationCanceledException");
                return Result<KafkaJsonMessage>.Success(kafkaJsonMessage);
            }
            finally
            {
                consumer.Unsubscribe();
            }
        }
    }
}
