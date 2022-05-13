namespace AddressRegistry.Consumer
{
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Simple;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Confluent.Kafka;
    using Microsoft.Extensions.Logging;
    using Projections;

    public class Consumer
    {
        private readonly ILifetimeScope _container;
        private readonly ILoggerFactory _loggerFactory;
        private readonly KafkaOptions _options;
        private readonly string _topic;
        private readonly string _consumerGroupSuffix;
        private readonly Offset? _offset;
        private readonly ILogger<Consumer> _logger;

        public Consumer(
            ILifetimeScope container,
            ILoggerFactory loggerFactory,
            KafkaOptions options,
            string topic,
            string consumerGroupSuffix,
            Offset? offset)
        {
            _container = container;
            _loggerFactory = loggerFactory;
            _options = options;
            _topic = topic;
            _consumerGroupSuffix = consumerGroupSuffix;
            _offset = offset;

            _logger = _loggerFactory.CreateLogger<Consumer>();
        }

        public Task<Result<KafkaJsonMessage>> Start(CancellationToken cancellationToken = default)
        {
            var commandHandler = new CommandHandler(_container, _loggerFactory);
            var projector = new ConnectedProjector<CommandHandler>(Resolve.WhenEqualToHandlerMessageType(new StreetNameKafkaProjection().Handlers));

            var consumerGroupId = $"{nameof(AddressRegistry)}.{nameof(Consumer)}.{_topic}{_consumerGroupSuffix}";
            return KafkaConsumer.Consume(
                new KafkaConsumerOptions(
                    _options.BootstrapServers,
                    _options.SaslUserName,
                    _options.SaslPassword,
                    consumerGroupId,
                    _topic,
                    async message =>
                    {
                        _logger.LogInformation("Handling next message");
                        await projector.ProjectAsync(commandHandler, message, cancellationToken);
                    },
                    noMessageFoundDelay: 300,
                    _offset,
                    _options.JsonSerializerSettings),
                cancellationToken);

            //if (!result.IsSuccess)
            //{
            //    var logger = _loggerFactory.CreateLogger<Consumer>();
            //    logger.LogCritical($"Consumer group {consumerGroupId} could not consume from topic {_consumerOptions.Topic}");
            //}
        }
    }
}
