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
    using AddressDomain = Address.Events;
    using StreetName = StreetName.Events;

    [ConnectedProjectionName("Kafka producer")]
    [ConnectedProjectionDescription("Projectie die berichten naar de kafka broker stuurt.")]
    public class ProducerProjections : ConnectedProjection<ProducerContext>
    {
        public const string AddressTopicKey = "AddressTopic";

        private readonly IProducer _producer;
        
        public ProducerProjections(IProducer producer)
        {
            _producer = producer;

            #region AddressAggregate Events
            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressBecameComplete>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressBecameCurrent>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressBecameIncomplete>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressBecameNotOfficiallyAssigned>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressBoxNumberWasChanged>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressBoxNumberWasCorrected>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressBoxNumberWasRemoved>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressHouseNumberWasChanged>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressHouseNumberWasCorrected>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressOfficialAssignmentWasRemoved>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressPersistentLocalIdWasAssigned>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressPositionWasCorrected>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressPositionWasRemoved>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressPostalCodeWasChanged>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressPostalCodeWasCorrected>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressPostalCodeWasRemoved>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressStatusWasCorrectedToRemoved>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressStatusWasRemoved>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressStreetNameWasChanged>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressStreetNameWasCorrected>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressWasCorrectedToCurrent>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressWasCorrectedToNotOfficiallyAssigned>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressWasCorrectedToOfficiallyAssigned>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressWasCorrectedToProposed>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressWasCorrectedToRetired>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressWasMigrated>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressWasOfficiallyAssigned>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressWasPositioned>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressWasProposed>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressWasRegistered>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressWasRemoved>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDomain.AddressWasRetired>>(async (_, message, ct) =>
            {
                await Produce(message.Message.AddressId, message.Message.ToContract(), message.Position, ct);
            });
            #endregion

            // StreetName Aggregate Events
            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetName.AddressWasRemovedBecauseHouseNumberWasRemoved>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNamePersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetName.AddressWasRemovedV2>>(async (_, message, ct) =>
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

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetName.AddressBoxNumberWasCorrectedV2>>(async (_, message, ct) =>
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

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetName.AddressWasCorrectedFromRejectedToProposed>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNamePersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetName.AddressRegularizationWasCorrected>>(async (_, message, ct) =>
            {
                await Produce(message.Message.StreetNamePersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });
        }

        private async Task Produce<T>(Guid addressId, T message, long storePosition, CancellationToken cancellationToken = default)
            where T : class, IQueueMessage
        {
            var result = await _producer.ProduceJsonMessage(
                new MessageKey(addressId.ToString("D")),
                message,
                new List<MessageHeader> { new MessageHeader(MessageHeader.IdempotenceKey, storePosition.ToString()) },
                cancellationToken);

            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(result.Error + Environment.NewLine + result.ErrorReason); //TODO: create custom exception
            }
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
                throw new InvalidOperationException(result.Error + Environment.NewLine + result.ErrorReason); //TODO: create custom exception
            }
        }
    }
}
