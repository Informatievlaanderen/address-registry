namespace AddressRegistry.Producer.Snapshot.Oslo
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.SnapshotProducer;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Producer;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using StreetName.Events;

    [ConnectedProjectionName("Kafka producer")]
    [ConnectedProjectionDescription("Projectie die berichten naar de kafka broker stuurt.")]
    public sealed class ProducerProjections : ConnectedProjection<ProducerContext>
    {
        public const string TopicKey = "AddressTopic";

        private readonly IProducer _producer;

        public ProducerProjections(IProducer producer, ISnapshotManager snapshotManager, string osloNamespace)
        {
            _producer = producer;

            // StreetName
            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameNamesWereCorrected>>(async (_, message, ct) =>
            {
                foreach (var addressPersistentLocalId in message.Message.AddressPersistentLocalIds)
                {
                    await FindAndProduce(async () =>
                            await snapshotManager.FindMatchingSnapshot(
                                addressPersistentLocalId.ToString(),
                                message.Message.Provenance.Timestamp,
                                message.Position,
                                throwStaleWhenGone: false,
                                ct),
                        message.Position,
                        ct);
                }
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameHomonymAdditionsWereCorrected>>(async (_, message, ct) =>
            {
                foreach (var addressPersistentLocalId in message.Message.AddressPersistentLocalIds)
                {
                    await FindAndProduce(async () =>
                            await snapshotManager.FindMatchingSnapshot(
                                addressPersistentLocalId.ToString(),
                                message.Message.Provenance.Timestamp,
                                message.Position,
                                throwStaleWhenGone: false,
                                ct),
                        message.Position,
                        ct);
                }
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<StreetNameHomonymAdditionsWereRemoved>>(async (_, message, ct) =>
            {
                foreach (var addressPersistentLocalId in message.Message.AddressPersistentLocalIds)
                {
                    await FindAndProduce(async () =>
                            await snapshotManager.FindMatchingSnapshot(
                                addressPersistentLocalId.ToString(),
                                message.Message.Provenance.Timestamp,
                                message.Position,
                                throwStaleWhenGone: false,
                                ct),
                        message.Position,
                        ct);
                }
            });

            // Address
            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressWasMigratedToStreetName>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.AddressPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                        message.Position,
                        ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressWasProposedV2>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.AddressPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                        message.Position,
                        ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressWasApproved>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.AddressPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                        message.Position,
                        ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressWasRejected>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.AddressPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                        message.Position,
                        ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressWasRejectedBecauseHouseNumberWasRejected>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.AddressPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                        message.Position,
                        ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressWasRejectedBecauseHouseNumberWasRetired>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.AddressPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                        message.Position,
                        ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressWasRejectedBecauseStreetNameWasRetired>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.AddressPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                        message.Position,
                        ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressWasRejectedBecauseStreetNameWasRejected>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.AddressPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                        message.Position,
                        ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressWasRetiredBecauseStreetNameWasRejected>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.AddressPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                        message.Position,
                        ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressWasRegularized>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.AddressPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                        message.Position,
                        ct);
            });


            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressWasDeregulated>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.AddressPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                        message.Position,
                        ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressWasRetiredV2>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.AddressPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                        message.Position,
                        ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressWasRetiredBecauseHouseNumberWasRetired>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.AddressPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                        message.Position,
                        ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressWasRetiredBecauseStreetNameWasRetired>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.AddressPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                        message.Position,
                        ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressBoxNumberWasCorrectedV2>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.AddressPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                        message.Position,
                        ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressHouseNumberWasCorrectedV2>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.AddressPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                        message.Position,
                        ct);

                foreach (var boxNumberPersistentLocalId in message.Message.BoxNumberPersistentLocalIds)
                {
                    await FindAndProduce(async () =>
                            await snapshotManager.FindMatchingSnapshot(
                                boxNumberPersistentLocalId.ToString(),
                                message.Message.Provenance.Timestamp,
                                message.Position,
                                throwStaleWhenGone: false,
                                ct),
                        message.Position,
                        ct);
                }
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressPositionWasCorrectedV2>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.AddressPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                        message.Position,
                        ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressPostalCodeWasCorrectedV2>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.AddressPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                        message.Position,
                        ct);

                foreach (var boxNumberPersistentLocalId in message.Message.BoxNumberPersistentLocalIds)
                {
                    await FindAndProduce(async () =>
                            await snapshotManager.FindMatchingSnapshot(
                                boxNumberPersistentLocalId.ToString(),
                                message.Message.Provenance.Timestamp,
                                message.Position,
                                throwStaleWhenGone: false,
                                ct),
                        message.Position,
                        ct);
                }
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressWasCorrectedFromApprovedToProposed>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.AddressPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                        message.Position,
                        ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.AddressPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                        message.Position,
                        ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressWasCorrectedFromRejectedToProposed>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.AddressPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                        message.Position,
                        ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressWasCorrectedFromRetiredToCurrent>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.AddressPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                        message.Position,
                        ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressPositionWasChanged>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.AddressPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                        message.Position,
                        ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressPostalCodeWasChangedV2>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.AddressPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                        message.Position,
                        ct);

                foreach (var boxNumberPersistentLocalId in message.Message.BoxNumberPersistentLocalIds)
                {
                    await FindAndProduce(async () =>
                            await snapshotManager.FindMatchingSnapshot(
                                boxNumberPersistentLocalId.ToString(),
                                message.Message.Provenance.Timestamp,
                                message.Position,
                                throwStaleWhenGone: false,
                                ct),
                        message.Position,
                        ct);
                }
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressRegularizationWasCorrected>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.AddressPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                    message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressDeregulationWasCorrected>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.AddressPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                    message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressWasRemovedBecauseHouseNumberWasRemoved>>(async (_, message, ct) =>
            {
                await Produce($"{osloNamespace}/{message.Message.AddressPersistentLocalId}", "{}", message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressWasRemovedV2>>(async (_, message, ct) =>
            {
                await Produce($"{osloNamespace}/{message.Message.AddressPersistentLocalId}", "{}", message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressWasRemovedBecauseStreetNameWasRemoved>>(async (_, message, ct) =>
            {
                await Produce($"{osloNamespace}/{message.Message.AddressPersistentLocalId}", "{}", message.Position, ct);
            });
        }

        private async Task FindAndProduce(Func<Task<OsloResult?>> findMatchingSnapshot, long storePosition, CancellationToken ct)
        {
            var result = await findMatchingSnapshot.Invoke();

            if (result != null)
            {
                await Produce(result.Identificator.Id, result.JsonContent, storePosition, ct);
            }
        }

        private async Task Produce(string objectId, string jsonContent, long storePosition, CancellationToken cancellationToken = default)
        {
            var result = await _producer.Produce(
                new MessageKey(objectId),
                jsonContent,
                new List<MessageHeader> { new MessageHeader(MessageHeader.IdempotenceKey, storePosition.ToString()) },
                cancellationToken);

            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(result.Error + Environment.NewLine + result.ErrorReason); //TODO: create custom exception
            }
        }
    }
}
