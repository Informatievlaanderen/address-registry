namespace AddressRegistry.Producer
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Producer;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Extensions;
    using StreetName = StreetName.Events;

    [ConnectedProjectionName("Kafka producer start vanaf migratie")]
    [ConnectedProjectionDescription("Projectie die berichten naar de kafka broker stuurt startende vanaf migratie.")]
    public class ProducerMigrateProjections : ConnectedProjection<ProducerContext>
    {
        public const string AddressTopicKey = "AddressMigrationTopic";

        private readonly IProducer _producer;

        public ProducerMigrateProjections(IProducer producer)
        {
            _producer = producer;

            // StreetName Aggregate Events
            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetName.AddressWasRemovedBecauseHouseNumberWasRemoved>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNamePersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetName.AddressWasRemovedV2>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNamePersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetName.AddressHouseNumberWasReaddressed>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNamePersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetName.AddressWasProposedBecauseOfReaddress>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNamePersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetName.AddressWasRejectedBecauseOfReaddress>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNamePersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetName.AddressWasRetiredBecauseOfReaddress>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNamePersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetName.AddressPositionWasChanged>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNamePersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetName.AddressPositionWasCorrectedV2>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNamePersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetName.AddressPostalCodeWasChangedV2>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNamePersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetName.AddressPostalCodeWasCorrectedV2>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNamePersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetName.AddressHouseNumberWasCorrectedV2>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNamePersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetName.AddressWasRejectedBecauseOfReaddress>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNamePersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetName.AddressWasRetiredV2>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNamePersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetName.AddressWasRetiredBecauseHouseNumberWasRetired>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNamePersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetName.AddressWasRetiredBecauseStreetNameWasRetired>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNamePersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetName.AddressWasCorrectedFromRetiredToCurrent>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNamePersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetName.AddressWasRegularized>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNamePersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetName.AddressWasDeregulated>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNamePersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetName.AddressWasRejectedBecauseStreetNameWasRetired>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNamePersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetName.AddressWasRejectedBecauseStreetNameWasRejected>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNamePersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

             When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetName.AddressWasRetiredBecauseStreetNameWasRejected>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNamePersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetName.AddressWasRejectedBecauseHouseNumberWasRetired>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNamePersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetName.AddressWasRejectedBecauseHouseNumberWasRejected>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNamePersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetName.AddressWasRejected>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNamePersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetName.AddressWasApproved>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNamePersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetName.AddressWasCorrectedFromApprovedToProposed>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNamePersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetName.AddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNamePersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetName.AddressWasMigratedToStreetName>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNamePersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetName.AddressWasProposedV2>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNamePersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetName.AddressWasProposedForMunicipalityMerger>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNamePersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetName.AddressWasRetiredBecauseOfMunicipalityMerger>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNamePersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetName.AddressWasRejectedBecauseOfMunicipalityMerger>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNamePersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetName.AddressWasCorrectedFromRejectedToProposed>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNamePersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetName.AddressRegularizationWasCorrected>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNamePersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetName.AddressDeregulationWasCorrected>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNamePersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetName.AddressWasRemovedBecauseStreetNameWasRemoved>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNamePersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetName.StreetNameWasReaddressed>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNamePersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetName.AddressRemovalWasCorrected>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNamePersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });
        }

        private async Task Produce<T>(int persistentLocalId, T message, long storePosition, CancellationToken cancellationToken = default)
            where T : class, IQueueMessage
        {
            var result = await _producer.ProduceJsonMessage(
                new MessageKey(persistentLocalId.ToString()),
                message,
                new List<MessageHeader> { new MessageHeader(MessageHeader.IdempotenceKey, storePosition.ToString()) },
                cancellationToken);

            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(result.Error + Environment.NewLine + result.ErrorReason);
            }
        }
    }
}
