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

    public sealed class AddressCommandHandlerModule : CommandHandlerModule
    {
        public AddressCommandHandlerModule(
            Func<IStreetNames> getStreetNames,
            Func<ConcurrentUnitOfWork> getUnitOfWork,
            Func<IStreamStore> getStreamStore,
            EventMapping eventMapping,
            EventSerializer eventSerializer,
            Func<ISnapshotStore> getSnapshotStore,
            IProvenanceFactory<StreetName> provenanceFactory,
            IMunicipalities municipalities)
        {
            For<MigrateAddressToStreetName>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<MigrateAddressToStreetName, StreetName>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streetNameStreamId = new StreetNameStreamId(message.Command.StreetNamePersistentLocalId);
                    var streetName = await getStreetNames().GetAsync(streetNameStreamId, ct);

                    streetName.MigrateAddress(
                        message.Command.AddressId,
                        message.Command.StreetNameId,
                        message.Command.AddressPersistentLocalId,
                        message.Command.Status,
                        message.Command.HouseNumber,
                        message.Command.BoxNumber,
                        message.Command.Geometry,
                        message.Command.OfficiallyAssigned,
                        message.Command.PostalCode,
                        message.Command.IsCompleted,
                        message.Command.IsRemoved,
                        message.Command.ParentAddressId);
                });

            For<ProposeAddress>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<ProposeAddress, StreetName>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streetNameStreamId = new StreetNameStreamId(message.Command.StreetNamePersistentLocalId);
                    var streetName = await getStreetNames().GetAsync(streetNameStreamId, ct);

                    streetName.ProposeAddress(
                        message.Command.StreetNamePersistentLocalId,
                        message.Command.AddressPersistentLocalId,
                        message.Command.PostalCode,
                        message.Command.PostalCodeMunicipalityId,
                        message.Command.HouseNumber,
                        message.Command.BoxNumber);
                });

            For<ApproveAddress>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<ApproveAddress, StreetName>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streetNameStreamId = new StreetNameStreamId(message.Command.StreetNamePersistentLocalId);
                    var streetName = await getStreetNames().GetAsync(streetNameStreamId, ct);

                    streetName.ApproveAddress(message.Command.AddressPersistentLocalId);
                });

            For<RejectAddress>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<RejectAddress, StreetName>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streetNameStreamId = new StreetNameStreamId(message.Command.StreetNamePersistentLocalId);
                    var streetName = await getStreetNames().GetAsync(streetNameStreamId, ct);

                    streetName.RejectAddress(message.Command.AddressPersistentLocalId);
                });

            For<DeregulateAddress>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<DeregulateAddress, StreetName>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streetNameStreamId = new StreetNameStreamId(message.Command.StreetNamePersistentLocalId);
                    var streetName = await getStreetNames().GetAsync(streetNameStreamId, ct);

                    streetName.DeregulateAddress(message.Command.AddressPersistentLocalId);
                });

            For<RegularizeAddress>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<RegularizeAddress, StreetName>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streetNameStreamId = new StreetNameStreamId(message.Command.StreetNamePersistentLocalId);
                    var streetName = await getStreetNames().GetAsync(streetNameStreamId, ct);

                    streetName.RegularizeAddress(message.Command.AddressPersistentLocalId);
                });

            For<RetireAddress>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<RetireAddress, StreetName>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streetNameStreamId = new StreetNameStreamId(message.Command.StreetNamePersistentLocalId);
                    var streetName = await getStreetNames().GetAsync(streetNameStreamId, ct);

                    streetName.RetireAddress(message.Command.AddressPersistentLocalId);
                });

            For<ChangeAddressPosition>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<ChangeAddressPosition, StreetName>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streetNameStreamId = new StreetNameStreamId(message.Command.StreetNamePersistentLocalId);
                    var streetName = await getStreetNames().GetAsync(streetNameStreamId, ct);

                    streetName.ChangeAddressPosition(
                        message.Command.AddressPersistentLocalId,
                        message.Command.GeometryMethod,
                        message.Command.GeometrySpecification,
                        message.Command.Position,
                        municipalities);
                });
        }
    }
}
