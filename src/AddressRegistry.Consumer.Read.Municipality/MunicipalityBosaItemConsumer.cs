namespace AddressRegistry.Consumer.Read.Municipality
{
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Simple;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Projections;

    public class MunicipalityBosaItemConsumer
    {
        private readonly ILifetimeScope _container;
        private readonly KafkaOptions _options;
        private readonly MunicipalityConsumerOptions _municipalityConsumerOptions;

        public MunicipalityBosaItemConsumer(
            ILifetimeScope container,
            KafkaOptions options,
            MunicipalityConsumerOptions municipalityConsumerOptions)
        {
            _container = container;
            _options = options;
            _municipalityConsumerOptions = municipalityConsumerOptions;
        }

        public Task Start(CancellationToken cancellationToken = default)
        {
            var projector = new ConnectedProjector<MunicipalityConsumerContext>(Resolve.WhenEqualToHandlerMessageType(new MunicipalityBosaItemProjections().Handlers));

            var consumerGroupId = $"{nameof(AddressRegistry)}.{nameof(MunicipalityBosaItemConsumer)}.{_municipalityConsumerOptions.Topic}{_municipalityConsumerOptions.ConsumerGroupSuffix}";
            return KafkaConsumer.Consume(
                new KafkaConsumerOptions(
                    _options.BootstrapServers,
                    _options.SaslUserName,
                    _options.SaslPassword,
                    consumerGroupId,
                    _municipalityConsumerOptions.Topic,
                    async message =>
                    {
                        var municipalityConsumerContext = _container.Resolve<MunicipalityConsumerContext>();
                        await projector.ProjectAsync(municipalityConsumerContext, message, CancellationToken.None);
                        await municipalityConsumerContext.SaveChangesAsync(CancellationToken.None);
                    },
                    noMessageFoundDelay: 300,
                    offset: null,
                    _options.JsonSerializerSettings),
                cancellationToken);
        }
    }
}
