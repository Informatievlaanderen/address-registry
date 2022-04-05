namespace AddressRegistry.Consumer
{
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Simple;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Microsoft.Extensions.Logging;
    using Projections;

    public class Consumer
    {
        private readonly ILifetimeScope _container;
        private readonly ILoggerFactory _loggerFactory;
        private readonly KafkaOptions _options;
        private readonly ConsumerOptions _consumerOptions;

        public Consumer(
            ILifetimeScope container,
            ILoggerFactory loggerFactory,
            KafkaOptions options,
            ConsumerOptions consumerOptions)
        {
            _container = container;
            _loggerFactory = loggerFactory;
            _options = options;
            _consumerOptions = consumerOptions;
        }

        public async Task Start(CancellationToken cancellationToken = default)
        {
            var commandHandler = new CommandHandler(_container, _loggerFactory);
            var projector = new ConnectedProjector<CommandHandler>(Resolve.WhenEqualToHandlerMessageType(new StreetNameKafkaProjection().Handlers));

            var consumerGroupId = $"{nameof(AddressRegistry)}.{nameof(Consumer)}.{_consumerOptions.Topic}{_consumerOptions.ConsumerGroupSuffix}";
            var result = await KafkaConsumer.Consume(
                _options,
                consumerGroupId,
                _consumerOptions.Topic,
                async message =>
                {
                    await projector.ProjectAsync(commandHandler, message, cancellationToken);
                },
                cancellationToken);

            if (!result.IsSuccess)
            {
                var logger = _loggerFactory.CreateLogger<Consumer>();
                logger.LogCritical($"Consumer group {consumerGroupId} could not consume from topic {_consumerOptions.Topic}");
            }
        }
    }
}
