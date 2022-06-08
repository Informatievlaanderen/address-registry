namespace AddressRegistry.Producer
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Simple;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Extensions;
    using Microsoft.Extensions.Configuration;
    using StreetName = StreetName.Events;

    [ConnectedProjectionName("Kafka producer start from migrate")]
    [ConnectedProjectionDescription("Projectie die berichten naar de kafka broker stuurt startende vanaf migratie.")]
    public class ProducerMigrateProjections : ConnectedProjection<ProducerContext>
    {
        private readonly KafkaProducerOptions _kafkaOptions;
        private readonly string _addressTopicKey = "AddressMigrationTopic";

        public ProducerMigrateProjections(IConfiguration configuration)
        {
            var bootstrapServers = configuration["Kafka:BootstrapServers"];
            var topic = $"{configuration[_addressTopicKey]}" ?? throw new ArgumentException($"Configuration has no value for {_addressTopicKey}");
            _kafkaOptions = new KafkaProducerOptions(
                bootstrapServers,
                configuration["Kafka:SaslUserName"],
                configuration["Kafka:SaslPassword"],
                topic,
                EventsJsonSerializerSettingsProvider.CreateSerializerSettings());

            // StreetName Aggregate Events
            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetName.AddressWasApproved>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNamePersistentLocalId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetName.AddressWasMigratedToStreetName>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNamePersistentLocalId, message.Message.ToContract(), ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetName.AddressWasProposedV2>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNamePersistentLocalId, message.Message.ToContract(), ct);
            });
        }

        private async Task Produce<T>(int persistentLocalId, T message, CancellationToken cancellationToken = default)
            where T : class, IQueueMessage
        {
            var result = await KafkaProducer.Produce(_kafkaOptions, persistentLocalId.ToString(), message, cancellationToken);
            if (!result.IsSuccess)
            {
                throw new ApplicationException(result.Error + Environment.NewLine + result.ErrorReason); //TODO: create custom exception
            }
        }
    }
}
