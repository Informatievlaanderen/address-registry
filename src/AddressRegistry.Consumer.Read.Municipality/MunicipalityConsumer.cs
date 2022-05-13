namespace AddressRegistry.Consumer.Read.Municipality
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Simple;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Microsoft.Extensions.Logging;
    using Projections;

    public class MunicipalityConsumer
    {
        private readonly ILifetimeScope _container;
        private readonly ILoggerFactory _loggerFactory;
        private readonly KafkaOptions _options;
        private readonly MunicipalityConsumerOptions _municipalityConsumerOptions;

        public MunicipalityConsumer(
            ILifetimeScope container,
            ILoggerFactory loggerFactory,
            KafkaOptions options,
            MunicipalityConsumerOptions municipalityConsumerOptions)
        {
            _container = container;
            _loggerFactory = loggerFactory;
            _options = options;
            _municipalityConsumerOptions = municipalityConsumerOptions;
        }

        public Task Start(CancellationToken cancellationToken = default)
        {
            //var projector = new ConnectedProjector<Func<ConsumerContext>>(Resolve.WhenEqualToHandlerMessageType(new MunicipalityProjections().Handlers));
            var projector = new ConnectedProjector<MunicipalityConsumerContext>(Resolve.WhenEqualToHandlerMessageType(new MunicipalityProjections().Handlers));

            var consumerGroupId = $"{nameof(AddressRegistry)}.{nameof(MunicipalityConsumer)}.{_municipalityConsumerOptions.Topic}{_municipalityConsumerOptions.ConsumerGroupSuffix}";
            return KafkaConsumer.Consume(
                new KafkaConsumerOptions(
                    _options.BootstrapServers,
                    _options.SaslUserName,
                    _options.SaslPassword,
                    consumerGroupId,
                    _municipalityConsumerOptions.Topic,
                    async message =>
                    {
                        await projector.ProjectAsync(_container.Resolve<MunicipalityConsumerContext>(), message, cancellationToken);
                    },
                    noMessageFoundDelay: 300,
                    offset: null,
                    _options.JsonSerializerSettings),
                cancellationToken);

            //if (!result.IsSuccess)
            //{
            //    var logger = _loggerFactory.CreateLogger<MunicipalityConsumer>();
            //    logger.LogCritical($"Consumer group {consumerGroupId} could not consume from topic {_municipalityConsumerOptions.Topic}");
            //}
        }
    }
}
