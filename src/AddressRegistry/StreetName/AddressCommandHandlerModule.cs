namespace AddressRegistry.StreetName
{
    using System;
    using System.Linq;
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
            Lazy<IPersistentLocalIdGenerator> lazyPersistentLocalIdGenerator,
            IProvenanceFactory<StreetName> provenanceFactory)
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
                        message.Command.AddressPersistentLocalId,
                        message.Command.PostalCode,
                        message.Command.PostalCodeMunicipalityId,
                        message.Command.HouseNumber,
                        message.Command.BoxNumber,
                        message.Command.GeometryMethod,
                        message.Command.GeometrySpecification,
                        message.Command.Position);
                });

            For<ProposeAddressesForMunicipalityMerger>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<ProposeAddressesForMunicipalityMerger, StreetName>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streetNames = getStreetNames();
                    var streetNameStreamId = new StreetNameStreamId(message.Command.StreetNamePersistentLocalId);
                    var streetName = await streetNames.GetAsync(streetNameStreamId, ct);

                    var sortedAddresses = message.Command.Addresses
                        .OrderBy(x => x.HouseNumber)
                        .ThenBy(x => x.BoxNumber)
                        .ToList();
                    foreach (var address in sortedAddresses)
                    {
                        var oldStreetName = await streetNames.GetAsync(new StreetNameStreamId(address.MergedStreetNamePersistentLocalId), ct);
                        var oldAddress = oldStreetName.StreetNameAddresses.GetByPersistentLocalId(new AddressPersistentLocalId(address.MergedAddressPersistentLocalId));

                        streetName.ProposeAddressForMunicipalityMerger(
                            address.AddressPersistentLocalId,
                            address.PostalCode,
                            address.HouseNumber,
                            address.BoxNumber,
                            address.GeometryMethod,
                            address.GeometrySpecification,
                            address.Position,
                            address.OfficiallyAssigned,
                            address.MergedAddressPersistentLocalId,
                            oldAddress.Status);
                    }
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

            For<RemoveAddress>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<RemoveAddress, StreetName>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streetNameStreamId = new StreetNameStreamId(message.Command.StreetNamePersistentLocalId);
                    var streetName = await getStreetNames().GetAsync(streetNameStreamId, ct);

                    streetName.RemoveAddress(message.Command.AddressPersistentLocalId);
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

            For<CorrectAddressPosition>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<CorrectAddressPosition, StreetName>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streetNameStreamId = new StreetNameStreamId(message.Command.StreetNamePersistentLocalId);
                    var streetName = await getStreetNames().GetAsync(streetNameStreamId, ct);

                    streetName.CorrectAddressPosition(
                        message.Command.AddressPersistentLocalId,
                        message.Command.GeometryMethod,
                        message.Command.GeometrySpecification,
                        message.Command.Position);
                });

            For<CorrectAddressPostalCode>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<CorrectAddressPostalCode, StreetName>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streetNameStreamId = new StreetNameStreamId(message.Command.StreetNamePersistentLocalId);
                    var streetName = await getStreetNames().GetAsync(streetNameStreamId, ct);

                    streetName.CorrectAddressPostalCode(message.Command.AddressPersistentLocalId, message.Command.PostalCode, message.Command.PostalCodeMunicipalityId);
                });

            For<CorrectAddressHouseNumber>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<CorrectAddressHouseNumber, StreetName>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streetNameStreamId = new StreetNameStreamId(message.Command.StreetNamePersistentLocalId);
                    var streetName = await getStreetNames().GetAsync(streetNameStreamId, ct);

                    streetName.CorrectAddressHouseNumber(message.Command.AddressPersistentLocalId, message.Command.HouseNumber);
                });

            For<CorrectAddressBoxNumber>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<CorrectAddressBoxNumber, StreetName>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streetNameStreamId = new StreetNameStreamId(message.Command.StreetNamePersistentLocalId);
                    var streetName = await getStreetNames().GetAsync(streetNameStreamId, ct);

                    streetName.CorrectAddressBoxNumber(message.Command.AddressPersistentLocalId, message.Command.BoxNumber);
                });

            For<CorrectAddressBoxNumbers>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<CorrectAddressBoxNumbers, StreetName>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streetNameStreamId = new StreetNameStreamId(message.Command.StreetNamePersistentLocalId);
                    var streetName = await getStreetNames().GetAsync(streetNameStreamId, ct);

                    streetName.CorrectAddressBoxNumbers(message.Command.AddressBoxNumbers);
                });

            For<CorrectAddressApproval>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<CorrectAddressApproval, StreetName>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streetNameStreamId = new StreetNameStreamId(message.Command.StreetNamePersistentLocalId);
                    var streetName = await getStreetNames().GetAsync(streetNameStreamId, ct);

                    streetName.CorrectAddressApproval(message.Command.AddressPersistentLocalId);
                });

            For<CorrectAddressRejection>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<CorrectAddressRejection, StreetName>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streetNameStreamId = new StreetNameStreamId(message.Command.StreetNamePersistentLocalId);
                    var streetName = await getStreetNames().GetAsync(streetNameStreamId, ct);

                    streetName.CorrectAddressRejection(message.Command.AddressPersistentLocalId);
                });

            For<CorrectAddressRetirement>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<CorrectAddressRetirement, StreetName>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streetNameStreamId = new StreetNameStreamId(message.Command.StreetNamePersistentLocalId);
                    var streetName = await getStreetNames().GetAsync(streetNameStreamId, ct);

                    streetName.CorrectAddressRetirement(message.Command.AddressPersistentLocalId);
                });

            For<CorrectAddressRegularization>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<CorrectAddressRegularization, StreetName>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streetNameStreamId = new StreetNameStreamId(message.Command.StreetNamePersistentLocalId);
                    var streetName = await getStreetNames().GetAsync(streetNameStreamId, ct);

                    streetName.CorrectAddressRegularization(message.Command.AddressPersistentLocalId);
                });

            For<CorrectAddressDeregulation>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<CorrectAddressDeregulation, StreetName>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streetNameStreamId = new StreetNameStreamId(message.Command.StreetNamePersistentLocalId);
                    var streetName = await getStreetNames().GetAsync(streetNameStreamId, ct);

                    streetName.CorrectAddressDeregulation(message.Command.AddressPersistentLocalId);
                });

            For<CorrectAddressRemoval>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<CorrectAddressRemoval, StreetName>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streetNameStreamId = new StreetNameStreamId(message.Command.StreetNamePersistentLocalId);
                    var streetName = await getStreetNames().GetAsync(streetNameStreamId, ct);

                    streetName.CorrectAddressRemoval(message.Command.AddressPersistentLocalId);
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
                        message.Command.Position);
                });

            For<ChangeAddressPostalCode>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<ChangeAddressPostalCode, StreetName>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streetNameStreamId = new StreetNameStreamId(message.Command.StreetNamePersistentLocalId);
                    var streetName = await getStreetNames().GetAsync(streetNameStreamId, ct);

                    streetName.ChangeAddressPostalCode(message.Command.AddressPersistentLocalId, message.Command.PostalCode);
                });

            For<Readdress>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<Readdress, StreetName>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var destinationStreetNameStreamId = new StreetNameStreamId(message.Command.DestinationStreetNamePersistentLocalId);

                    var streetNames = getStreetNames();
                    var destinationStreetName = await streetNames.GetAsync(destinationStreetNameStreamId, ct);

                    destinationStreetName.Readdress(
                        streetNames,
                        lazyPersistentLocalIdGenerator.Value,
                        message.Command.ReaddressAddressItems,
                        message.Command.RetireAddressItems,
                        message.Command.ExecutionContext);
                });

            For<RenameStreetName>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<RenameStreetName, StreetName>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var destinationStreetNameStreamId = new StreetNameStreamId(message.Command.DestinationPersistentLocalId);

                    var streetNames = getStreetNames();
                    var destinationStreetName = await streetNames.GetAsync(destinationStreetNameStreamId, ct);

                    destinationStreetName.Rename(
                        streetNames,
                        message.Command.PersistentLocalId,
                        lazyPersistentLocalIdGenerator.Value);
                });

            For<RejectOrRetireAddressForReaddress>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<RejectOrRetireAddressForReaddress, StreetName>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streetNameStreamId = new StreetNameStreamId(message.Command.StreetNamePersistentLocalId);
                    var streetName = await getStreetNames().GetAsync(streetNameStreamId, ct);

                    streetName.RejectOrRetireAddressForReaddress(message.Command.AddressPersistentLocalId);
                });
        }
    }
}
