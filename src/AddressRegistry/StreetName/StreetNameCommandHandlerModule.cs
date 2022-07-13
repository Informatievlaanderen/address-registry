namespace AddressRegistry.StreetName
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Commands;
    using SqlStreamStore;

    public sealed class StreetNameCommandHandlerModule : CommandHandlerModule
    {
        public StreetNameCommandHandlerModule(
            IStreetNameFactory streetNameFactory,
            Func<IStreetNames> getStreetNames,
            Func<ISnapshotStore> getSnapshotStore,
            Func<ConcurrentUnitOfWork> getUnitOfWork,
            Func<IStreamStore> getStreamStore,
            EventMapping eventMapping,
            EventSerializer eventSerializer,
            IProvenanceFactory<StreetName> provenanceFactory)
        {
            For<ImportMigratedStreetName>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<ImportMigratedStreetName, StreetName>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streetNameStreamId = new StreetNameStreamId(message.Command.PersistentLocalId);
                    var streetName = await getStreetNames().GetOptionalAsync(streetNameStreamId, ct);

                    if (streetName.HasValue)
                    {
                        throw new AggregateSourceException($"StreetName with id {message.Command.PersistentLocalId} already exists");
                    }

                    var newStreetName = StreetName.Register(
                        streetNameFactory,
                        message.Command.StreetNameId,
                        message.Command.PersistentLocalId,
                        message.Command.MunicipalityId,
                        message.Command.NisCode,
                        message.Command.StreetNameStatus);

                    getStreetNames().Add(streetNameStreamId, newStreetName);
                });

            For<ImportStreetName>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<ImportStreetName, StreetName>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streetNameStreamId = new StreetNameStreamId(message.Command.PersistentLocalId);
                    var streetName = await getStreetNames().GetOptionalAsync(streetNameStreamId, ct);

                    if (streetName.HasValue)
                    {
                        throw new AggregateSourceException($"StreetName with id {message.Command.PersistentLocalId} already exists");
                    }

                    var newStreetName = StreetName.Register(
                        streetNameFactory,
                        message.Command.PersistentLocalId,
                        message.Command.MunicipalityId,
                        message.Command.StreetNameStatus);

                    getStreetNames().Add(streetNameStreamId, newStreetName);
                });

            For<ApproveStreetName>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<ApproveStreetName, StreetName>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streetNameStreamId = new StreetNameStreamId(message.Command.PersistentLocalId);
                    var streetName = await getStreetNames().GetAsync(streetNameStreamId, ct);

                    streetName.ApproveStreetName();
                });

            For<RemoveStreetName>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<RemoveStreetName, StreetName>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streetNameStreamId = new StreetNameStreamId(message.Command.PersistentLocalId);
                    var streetName = await getStreetNames().GetAsync(streetNameStreamId, ct);

                    streetName.RemoveStreetName();
                });
        }
    }
}
