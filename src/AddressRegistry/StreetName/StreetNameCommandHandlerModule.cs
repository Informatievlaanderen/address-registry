namespace AddressRegistry.StreetName
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
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
            Func<IStreetNames> getStreetNames,
            Func<ConcurrentUnitOfWork> getUnitOfWork,
            Func<IStreamStore> getStreamStore,
            EventMapping eventMapping,
            EventSerializer eventSerializer,
            StreetNameProvenanceFactory provenanceFactory)
        {
            For<ImportStreetName>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer)
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

                    var newStreetName = StreetName.Register(message.Command.PersistentLocalId, message.Command.MunicipalityId, message.Command.StreetNameStatus);

                    getStreetNames().Add(streetNameStreamId, newStreetName);
                });

            For<ApproveStreetName>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer)
                .AddEventHash<ApproveStreetName, StreetName>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streetNameStreamId = new StreetNameStreamId(message.Command.PersistentLocalId);
                    var streetName = await getStreetNames().GetAsync(streetNameStreamId, ct);

                    streetName.ApproveStreetName();
                });

            For<RemoveStreetName>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer)
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
