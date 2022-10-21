namespace AddressRegistry.Producer.Snapshot.Oslo
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.StreetName.Events;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.SnapshotProducer;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Simple;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Microsoft.Extensions.Configuration;

    [ConnectedProjectionName("Kafka producer")]
    [ConnectedProjectionDescription("Projectie die berichten naar de kafka broker stuurt.")]
    public sealed class ProducerProjections : ConnectedProjection<ProducerContext>
    {
        private readonly KafkaProducerOptions _kafkaOptions;
        private readonly string _TopicKey = "AddressTopic";

        public ProducerProjections(IConfiguration configuration, ISnapshotManager snapshotManager)
        {
            var bootstrapServers = configuration["Kafka:BootstrapServers"];
            var osloNamespace = configuration["OsloNamespace"];
            osloNamespace = osloNamespace.TrimEnd('/');

            var topic = $"{configuration[_TopicKey]}" ?? throw new ArgumentException($"Configuration has no value for {_TopicKey}");
            _kafkaOptions = new KafkaProducerOptions(
                bootstrapServers,
                configuration["Kafka:SaslUserName"],
                configuration["Kafka:SaslPassword"],
                topic,
                false,
                EventsJsonSerializerSettingsProvider.CreateSerializerSettings());

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressWasMigratedToStreetName>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.AddressPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            throwStaleWhenGone: false,
                            ct),
                        ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressWasProposedV2>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.AddressPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            throwStaleWhenGone: false,
                            ct),
                        ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressWasApproved>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.AddressPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            throwStaleWhenGone: false,
                            ct),
                        ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressWasRejected>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.AddressPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            throwStaleWhenGone: false,
                            ct),
                        ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressWasRejectedBecauseHouseNumberWasRejected>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.AddressPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            throwStaleWhenGone: false,
                            ct),
                        ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressWasRejectedBecauseHouseNumberWasRetired>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.AddressPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            throwStaleWhenGone: false,
                            ct),
                        ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressWasRejectedBecauseStreetNameWasRetired>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.AddressPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            throwStaleWhenGone: false,
                            ct),
                        ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressWasRegularized>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.AddressPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            throwStaleWhenGone: false,
                            ct),
                        ct);
            });


            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressWasDeregulated>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.AddressPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            throwStaleWhenGone: false,
                            ct),
                        ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressWasRetiredV2>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.AddressPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            throwStaleWhenGone: false,
                            ct),
                        ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressWasRetiredBecauseHouseNumberWasRetired>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.AddressPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            throwStaleWhenGone: false,
                            ct),
                        ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressWasRetiredBecauseStreetNameWasRetired>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.AddressPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            throwStaleWhenGone: false,
                            ct),
                        ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressBoxNumberWasCorrectedV2>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.AddressPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            throwStaleWhenGone: false,
                            ct),
                        ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressHouseNumberWasCorrectedV2>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.AddressPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            throwStaleWhenGone: false,
                            ct),
                        ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressPositionWasCorrectedV2>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.AddressPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            throwStaleWhenGone: false,
                            ct),
                        ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressPostalCodeWasCorrectedV2>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.AddressPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            throwStaleWhenGone: false,
                            ct),
                        ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressWasCorrectedFromApprovedToProposed>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.AddressPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            throwStaleWhenGone: false,
                            ct),
                        ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.AddressPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            throwStaleWhenGone: false,
                            ct),
                        ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressWasCorrectedFromRejectedToProposed>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.AddressPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            throwStaleWhenGone: false,
                            ct),
                        ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressWasCorrectedFromRetiredToCurrent>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.AddressPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            throwStaleWhenGone: false,
                            ct),
                        ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressPositionWasChanged>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.AddressPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            throwStaleWhenGone: false,
                            ct),
                        ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressPostalCodeWasChangedV2>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.AddressPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            throwStaleWhenGone: false,
                            ct),
                        ct);
            });

            //When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressCorrectRemovedV2>>(async (_, message, ct) =>
            //{
            //    await FindAndProduce(async () =>
            //            await snapshotManager.FindMatchingSnapshot(
            //                message.Message.AddressPersistentLocalId.ToString(),
            //                message.Message.Provenance.Timestamp,
            //                throwStaleWhenGone: true, // retry getting snapshot if statuscode 410 is returned because the oslo projection hasn't received the event yet
            //                ct),
            //            ct);
            //});

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressWasRemovedBecauseHouseNumberWasRemoved>>(async (_, message, ct) =>
            {
                await Produce($"{osloNamespace}/{message.Message.AddressPersistentLocalId}", "{}", ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<AddressWasRemovedV2>>(async (_, message, ct) =>
            {
                await Produce($"{osloNamespace}/{message.Message.AddressPersistentLocalId}", "{}", ct);
            });
        }

        private async Task FindAndProduce(Func<Task<OsloResult?>> findMatchingSnapshot, CancellationToken ct)
        {
            var result = await findMatchingSnapshot.Invoke();

            if (result != null)
            {
                await Produce(result.Identificator.Id, result.JsonContent, ct);
            }
        }

        private async Task Produce(string objectId, string jsonContent, CancellationToken cancellationToken = default)
        {
            var result = await KafkaProducer.Produce(_kafkaOptions, objectId, jsonContent, cancellationToken);
            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(result.Error + Environment.NewLine + result.ErrorReason); //TODO: create custom exception
            }
        }
    }
}
