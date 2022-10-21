namespace AddressRegistry.Consumer.Read.StreetName
{
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Simple;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Projections;

    public class StreetNameLatestItemConsumer
    {
        private readonly ILifetimeScope _container;
        private readonly KafkaOptions _options;
        private readonly StreetNameConsumerOptions _streetNameConsumerOptions;

        public StreetNameLatestItemConsumer(
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
            var latestItemProjector = new ConnectedProjector<StreetNameConsumerContext>(Resolve.WhenEqualToHandlerMessageType(new StreetNameLatestItemProjections().Handlers));

            var consumerGroupId = $"{nameof(AddressRegistry)}.{nameof(StreetNameLatestItemConsumer)}.{_streetNameConsumerOptions.Topic}{_streetNameConsumerOptions.ConsumerGroupSuffix}";
            return KafkaConsumer.Consume(
                new KafkaConsumerOptions(
                    _options.BootstrapServers,
                    _options.SaslUserName,
                    _options.SaslPassword,
                    consumerGroupId,
                    _streetNameConsumerOptions.Topic,
                    async message =>
                    {
                        //Cannot cancel in between
                        var streetnameConsumerContext = _container.Resolve<StreetNameConsumerContext>();
                        await latestItemProjector.ProjectAsync(streetnameConsumerContext, message, CancellationToken.None);
                        await streetnameConsumerContext.SaveChangesAsync(CancellationToken.None);
                    },
                    noMessageFoundDelay: 300,
                    offset: null,
                    _options.JsonSerializerSettings),
                cancellationToken);
        }
    }
}
