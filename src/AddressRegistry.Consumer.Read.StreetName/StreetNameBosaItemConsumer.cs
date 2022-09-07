namespace AddressRegistry.Consumer.Read.StreetName
{
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Simple;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Projections;

    public class StreetNameBosaItemConsumer
    {
        private readonly ILifetimeScope _container;
        private readonly KafkaOptions _options;
        private readonly StreetNameConsumerOptions _streetNameConsumerOptions;

        public StreetNameBosaItemConsumer(
            ILifetimeScope container,
            KafkaOptions options,
            StreetNameConsumerOptions streetNameConsumerOptions)
        {
            _container = container;
            _options = options;
            _streetNameConsumerOptions = streetNameConsumerOptions;
        }

        public Task Start(CancellationToken cancellationToken = default)
        {
            var bosaItemProjections = new ConnectedProjector<StreetNameConsumerContext>(Resolve.WhenEqualToHandlerMessageType(new StreetNameBosaItemProjections().Handlers));

            var consumerGroupId = $"{nameof(AddressRegistry)}.{nameof(StreetNameLatestItemConsumer)}.{_streetNameConsumerOptions.Topic}{_streetNameConsumerOptions.ConsumerGroupSuffix}.bosa";
            return KafkaConsumer.Consume(
                new KafkaConsumerOptions(
                    _options.BootstrapServers,
                    _options.SaslUserName,
                    _options.SaslPassword,
                    consumerGroupId,
                    _streetNameConsumerOptions.Topic,
                    async message =>
                    {
                        var streetnameConsumerContext = _container.Resolve<StreetNameConsumerContext>();
                        await bosaItemProjections.ProjectAsync(streetnameConsumerContext, message, cancellationToken);
                        await streetnameConsumerContext.SaveChangesAsync(cancellationToken);
                    },
                    noMessageFoundDelay: 300,
                    offset: null,
                    _options.JsonSerializerSettings),
                cancellationToken);
        }
    }
}
