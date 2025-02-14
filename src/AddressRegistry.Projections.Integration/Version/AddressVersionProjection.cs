namespace AddressRegistry.Projections.Integration.Version
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Address.Events;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Convertors;
    using Infrastructure;
    using Microsoft.Extensions.Options;
    using StreetName;
    using StreetName.Events;

    [ConnectedProjectionName("Integratie adres versie")]
    [ConnectedProjectionDescription("Projectie die de laatste adres data voor de integratie database bijhoudt.")]
    public sealed class AddressVersionProjections : ConnectedProjection<IntegrationContext>
    {
        public AddressVersionProjections(IOptions<IntegrationOptions> options, IEventsRepository eventsRepository)
        {
            // Address
            When<Envelope<AddressWasMigratedToStreetName>>(async (context, message, ct) =>
            {
                var geometry = WKBReaderFactory.CreateForLegacy().Read(message.Message.ExtendedWkbGeometry.ToByteArray());

                var addressVersion = new AddressVersion
                {
                    Position = message.Position,
                    PersistentLocalId = message.Message.AddressPersistentLocalId,
                    PostalCode = message.Message.PostalCode,
                    StreetNamePersistentLocalId = message.Message.StreetNamePersistentLocalId,
                    ParentPersistentLocalId = message.Message.ParentPersistentLocalId,
                    Status = message.Message.Status,
                    OsloStatus = message.Message.Status.Map(),
                    HouseNumber = message.Message.HouseNumber,
                    BoxNumber = message.Message.BoxNumber,
                    Geometry = geometry,
                    PositionMethod = message.Message.GeometryMethod,
                    OsloPositionMethod = message.Message.GeometryMethod.ToPositieGeometrieMethode(),
                    PositionSpecification = message.Message.GeometrySpecification,
                    OsloPositionSpecification = message.Message.GeometrySpecification.ToPositieSpecificatie(),
                    OfficiallyAssigned = message.Message.OfficiallyAssigned,
                    Removed = message.Message.IsRemoved,
                    VersionTimestamp = message.Message.Provenance.Timestamp,
                    CreatedOnTimestamp = message.Message.Provenance.Timestamp,
                    Namespace = options.Value.Namespace,
                    PuriId = $"{options.Value.Namespace}/{message.Message.AddressPersistentLocalId}",
                    Type = message.EventName
                };

                await context.AddressVersions.AddAsync(addressVersion, ct);
            });

            When<Envelope<AddressWasProposedV2>>(async (context, message, ct) =>
            {
                var geometry = WKBReaderFactory.CreateForLegacy().Read(message.Message.ExtendedWkbGeometry.ToByteArray());

                var addressVersion = new AddressVersion()
                {
                    Position = message.Position,
                    PersistentLocalId = message.Message.AddressPersistentLocalId,
                    PostalCode = message.Message.PostalCode,
                    StreetNamePersistentLocalId = message.Message.StreetNamePersistentLocalId,
                    ParentPersistentLocalId = message.Message.ParentPersistentLocalId,
                    Status = AddressStatus.Proposed,
                    OsloStatus = AddressStatus.Proposed.Map(),
                    HouseNumber = message.Message.HouseNumber,
                    BoxNumber = message.Message.BoxNumber,
                    Geometry = geometry,
                    PositionMethod = message.Message.GeometryMethod,
                    OsloPositionMethod = message.Message.GeometryMethod.ToPositieGeometrieMethode(),
                    PositionSpecification = message.Message.GeometrySpecification,
                    OsloPositionSpecification = message.Message.GeometrySpecification.ToPositieSpecificatie(),
                    OfficiallyAssigned = true,
                    Removed = false,
                    VersionTimestamp = message.Message.Provenance.Timestamp,
                    CreatedOnTimestamp = message.Message.Provenance.Timestamp,
                    Namespace = options.Value.Namespace,
                    PuriId = $"{options.Value.Namespace}/{message.Message.AddressPersistentLocalId}",
                    Type = message.EventName
                };

                await context.AddressVersions.AddAsync(addressVersion, ct);
            });

            When<Envelope<AddressWasProposedForMunicipalityMerger>>(async (context, message, ct) =>
            {
                var geometry = WKBReaderFactory.CreateForLegacy().Read(message.Message.ExtendedWkbGeometry.ToByteArray());

                var addressVersion = new AddressVersion()
                {
                    Position = message.Position,
                    PersistentLocalId = message.Message.AddressPersistentLocalId,
                    PostalCode = message.Message.PostalCode,
                    StreetNamePersistentLocalId = message.Message.StreetNamePersistentLocalId,
                    ParentPersistentLocalId = message.Message.ParentPersistentLocalId,
                    Status = AddressStatus.Proposed,
                    OsloStatus = AddressStatus.Proposed.Map(),
                    HouseNumber = message.Message.HouseNumber,
                    BoxNumber = message.Message.BoxNumber,
                    Geometry = geometry,
                    PositionMethod = message.Message.GeometryMethod,
                    OsloPositionMethod = message.Message.GeometryMethod.ToPositieGeometrieMethode(),
                    PositionSpecification = message.Message.GeometrySpecification,
                    OsloPositionSpecification = message.Message.GeometrySpecification.ToPositieSpecificatie(),
                    OfficiallyAssigned = message.Message.OfficiallyAssigned,
                    Removed = false,
                    VersionTimestamp = message.Message.Provenance.Timestamp,
                    CreatedOnTimestamp = message.Message.Provenance.Timestamp,
                    Namespace = options.Value.Namespace,
                    PuriId = $"{options.Value.Namespace}/{message.Message.AddressPersistentLocalId}",
                    Type = message.EventName
                };

                await context.AddressVersions.AddAsync(addressVersion, ct);
            });

            When<Envelope<AddressWasApproved>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.Status = AddressStatus.Current;
                        item.OsloStatus = AddressStatus.Current.Map();
                    },
                    ct);
            });

            When<Envelope<AddressWasCorrectedFromApprovedToProposed>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.Status = AddressStatus.Proposed;
                        item.OsloStatus = AddressStatus.Proposed.Map();
                    },
                    ct);
            });

            When<Envelope<AddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.Status = AddressStatus.Proposed;
                        item.OsloStatus = AddressStatus.Proposed.Map();
                    },
                    ct);
            });

            When<Envelope<AddressWasRejected>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.Status = AddressStatus.Rejected;
                        item.OsloStatus = AddressStatus.Rejected.Map();
                    },
                    ct);
            });

            When<Envelope<AddressWasRejectedBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.Status = AddressStatus.Rejected;
                        item.OsloStatus = AddressStatus.Rejected.Map();
                    },
                    ct);
            });

            When<Envelope<AddressWasRejectedBecauseHouseNumberWasRejected>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.Status = AddressStatus.Rejected;
                        item.OsloStatus = AddressStatus.Rejected.Map();
                    },
                    ct);
            });

            When<Envelope<AddressWasRejectedBecauseHouseNumberWasRetired>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.Status = AddressStatus.Rejected;
                        item.OsloStatus = AddressStatus.Rejected.Map();
                    },
                    ct);
            });

            When<Envelope<AddressWasRejectedBecauseStreetNameWasRejected>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.Status = AddressStatus.Rejected;
                        item.OsloStatus = AddressStatus.Rejected.Map();
                    },
                    ct);
            });

            When<Envelope<AddressWasRetiredBecauseStreetNameWasRejected>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.Status = AddressStatus.Retired;
                        item.OsloStatus = AddressStatus.Retired.Map();
                    },
                    ct);
            });

            When<Envelope<AddressWasRejectedBecauseStreetNameWasRetired>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.Status = AddressStatus.Rejected;
                        item.OsloStatus = AddressStatus.Rejected.Map();
                    },
                    ct);
            });

            When<Envelope<AddressWasDeregulated>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.OfficiallyAssigned = false;
                        item.Status = AddressStatus.Current;
                        item.OsloStatus = AddressStatus.Current.Map();
                    },
                    ct);
            });

            When<Envelope<AddressWasRegularized>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item => { item.OfficiallyAssigned = true; },
                    ct);
            });

            When<Envelope<AddressWasRetiredV2>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.Status = AddressStatus.Retired;
                        item.OsloStatus = AddressStatus.Retired.Map();
                    },
                    ct);
            });

            When<Envelope<AddressWasRetiredBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.Status = AddressStatus.Retired;
                        item.OsloStatus = AddressStatus.Retired.Map();
                    },
                    ct);
            });

            When<Envelope<AddressWasRetiredBecauseHouseNumberWasRetired>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.Status = AddressStatus.Retired;
                        item.OsloStatus = AddressStatus.Retired.Map();
                    },
                    ct);
            });

            When<Envelope<AddressWasRetiredBecauseStreetNameWasRetired>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.Status = AddressStatus.Retired;
                        item.OsloStatus = AddressStatus.Retired.Map();
                    },
                    ct);
            });

            When<Envelope<AddressWasCorrectedFromRetiredToCurrent>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.Status = AddressStatus.Current;
                        item.OsloStatus = AddressStatus.Current.Map();
                    },
                    ct);
            });

            When<Envelope<AddressPostalCodeWasChangedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item => { item.PostalCode = message.Message.PostalCode; },
                    ct);

                foreach (var boxNumberPersistentLocalId in message.Message.BoxNumberPersistentLocalIds)
                {
                    await context.CreateNewAddressVersion(
                        new PersistentLocalId(boxNumberPersistentLocalId),
                        message,
                        boxNumberItem => { boxNumberItem.PostalCode = message.Message.PostalCode; },
                        ct);
                }
            });

            When<Envelope<AddressPostalCodeWasCorrectedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item => { item.PostalCode = message.Message.PostalCode; },
                    ct);

                foreach (var boxNumberPersistentLocalId in message.Message.BoxNumberPersistentLocalIds)
                {
                    await context.CreateNewAddressVersion(
                        new PersistentLocalId(boxNumberPersistentLocalId),
                        message,
                        boxNumberItem => { boxNumberItem.PostalCode = message.Message.PostalCode; },
                        ct);
                }
            });

            When<Envelope<AddressHouseNumberWasCorrectedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item => { item.HouseNumber = message.Message.HouseNumber; },
                    ct);

                foreach (var boxNumberPersistentLocalId in message.Message.BoxNumberPersistentLocalIds)
                {
                    await context.CreateNewAddressVersion(
                        new PersistentLocalId(boxNumberPersistentLocalId),
                        message,
                        boxNumberItem => { boxNumberItem.HouseNumber = message.Message.HouseNumber; },
                        ct);
                }
            });

            When<Envelope<AddressBoxNumberWasCorrectedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item => { item.BoxNumber = message.Message.BoxNumber; },
                    ct);
            });

            When<Envelope<AddressBoxNumbersWereCorrected>>(async (context, message, ct) =>
            {
                foreach (var (addressPersistentLocalId, boxNumber) in message.Message.AddressBoxNumbers)
                {
                    await context.CreateNewAddressVersion(
                        new PersistentLocalId(addressPersistentLocalId),
                        message,
                        boxNumberItem => { boxNumberItem.BoxNumber = boxNumber; },
                        ct);
                }
            });

            When<Envelope<AddressPositionWasChanged>>(async (context, message, ct) =>
            {
                var geometry = WKBReaderFactory.CreateForLegacy().Read(message.Message.ExtendedWkbGeometry.ToByteArray());

                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.PositionMethod = message.Message.GeometryMethod;
                        item.OsloPositionMethod = message.Message.GeometryMethod.ToPositieGeometrieMethode();
                        item.PositionSpecification =  message.Message.GeometrySpecification;
                        item.OsloPositionSpecification =  message.Message.GeometrySpecification.ToPositieSpecificatie();
                        item.Geometry = geometry;
                    },
                    ct);
            });

            When<Envelope<AddressPositionWasCorrectedV2>>(async (context, message, ct) =>
            {
                var geometry = WKBReaderFactory.CreateForLegacy().Read(message.Message.ExtendedWkbGeometry.ToByteArray());

                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.PositionMethod = message.Message.GeometryMethod;
                        item.OsloPositionMethod = message.Message.GeometryMethod.ToPositieGeometrieMethode();
                        item.PositionSpecification =  message.Message.GeometrySpecification;
                        item.OsloPositionSpecification =  message.Message.GeometrySpecification.ToPositieSpecificatie();
                        item.Geometry = geometry;
                    },
                    ct);
            });

            When<Envelope<AddressHouseNumberWasReaddressed>>(async (context, message, ct) =>
            {
                var geometry = WKBReaderFactory.CreateForLegacy()
                    .Read(message.Message.ReaddressedHouseNumber.SourceExtendedWkbGeometry.ToByteArray());

                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.Status = message.Message.ReaddressedHouseNumber.SourceStatus;
                        item.OsloStatus = message.Message.ReaddressedHouseNumber.SourceStatus.Map();
                        item.HouseNumber = message.Message.ReaddressedHouseNumber.DestinationHouseNumber;
                        item.PostalCode = message.Message.ReaddressedHouseNumber.SourcePostalCode;
                        item.OfficiallyAssigned = message.Message.ReaddressedHouseNumber.SourceIsOfficiallyAssigned;
                        item.PositionMethod = message.Message.ReaddressedHouseNumber.SourceGeometryMethod;
                        item.OsloPositionMethod = message.Message.ReaddressedHouseNumber.SourceGeometryMethod.ToPositieGeometrieMethode();
                        item.PositionSpecification = message.Message.ReaddressedHouseNumber.SourceGeometrySpecification;
                        item.OsloPositionSpecification = message.Message.ReaddressedHouseNumber.SourceGeometrySpecification.ToPositieSpecificatie();
                        item.Geometry = geometry;
                    },
                    ct);

                foreach (var readdressedBoxNumber in message.Message.ReaddressedBoxNumbers)
                {
                    var boxNumberGeometry = WKBReaderFactory.CreateForLegacy()
                        .Read(message.Message.ReaddressedHouseNumber.SourceExtendedWkbGeometry.ToByteArray());

                    await context.CreateNewAddressVersion(
                        new PersistentLocalId(readdressedBoxNumber.DestinationAddressPersistentLocalId),
                        message,
                        item =>
                        {
                            item.Status = readdressedBoxNumber.SourceStatus;
                            item.OsloStatus = readdressedBoxNumber.SourceStatus.Map();
                            item.HouseNumber = readdressedBoxNumber.DestinationHouseNumber;
                            item.BoxNumber = readdressedBoxNumber.SourceBoxNumber;
                            item.PostalCode = readdressedBoxNumber.SourcePostalCode;
                            item.OfficiallyAssigned = readdressedBoxNumber.SourceIsOfficiallyAssigned;
                            item.PositionMethod = readdressedBoxNumber.SourceGeometryMethod;
                            item.OsloPositionMethod = readdressedBoxNumber.SourceGeometryMethod.ToPositieGeometrieMethode();
                            item.PositionSpecification = readdressedBoxNumber.SourceGeometrySpecification;
                            item.OsloPositionSpecification = readdressedBoxNumber.SourceGeometrySpecification.ToPositieSpecificatie();
                            item.Geometry = boxNumberGeometry;
                        },
                        ct);
                }
            });

            When<Envelope<AddressWasProposedBecauseOfReaddress>>(async (context, message, ct) =>
            {
                var geometry = WKBReaderFactory.CreateForLegacy().Read(message.Message.ExtendedWkbGeometry.ToByteArray());

                var addressDetailItemV2 = new AddressVersion
                {
                    Position = message.Position,
                    PersistentLocalId = new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    PostalCode = message.Message.PostalCode,
                    StreetNamePersistentLocalId = message.Message.StreetNamePersistentLocalId,
                    ParentPersistentLocalId = message.Message.ParentPersistentLocalId,
                    Status = AddressStatus.Proposed,
                    OsloStatus = AddressStatus.Proposed.Map(),
                    HouseNumber = message.Message.HouseNumber,
                    BoxNumber = message.Message.BoxNumber,
                    Geometry = geometry,
                    PositionMethod = message.Message.GeometryMethod,
                    OsloPositionMethod = message.Message.GeometryMethod.ToPositieGeometrieMethode(),
                    PositionSpecification = message.Message.GeometrySpecification,
                    OsloPositionSpecification = message.Message.GeometrySpecification.ToPositieSpecificatie(),
                    OfficiallyAssigned = true,
                    Removed = false,
                    VersionTimestamp = message.Message.Provenance.Timestamp,
                    CreatedOnTimestamp = message.Message.Provenance.Timestamp,
                    Namespace = options.Value.Namespace,
                    PuriId = $"{options.Value.Namespace}/{message.Message.AddressPersistentLocalId}",
                    Type = message.EventName
                };

                await context
                    .AddressVersions
                    .AddAsync(addressDetailItemV2, ct);
            });

            When<Envelope<AddressWasRejectedBecauseOfReaddress>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.Status = AddressStatus.Rejected;
                        item.OsloStatus = AddressStatus.Rejected.Map();
                    },
                    ct);
            });

            When<Envelope<AddressWasRetiredBecauseOfReaddress>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.Status = AddressStatus.Retired;
                        item.OsloStatus = AddressStatus.Retired.Map();
                    },
                    ct);
            });

            When<Envelope<AddressWasRemovedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item => { item.Removed = true; },
                    ct);
            });

            When<Envelope<AddressWasRemovedBecauseStreetNameWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item => { item.Removed = true; },
                    ct);
            });

            When<Envelope<AddressWasRemovedBecauseHouseNumberWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item => { item.Removed = true; },
                    ct);
            });

            When<Envelope<AddressWasCorrectedFromRejectedToProposed>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.Status = AddressStatus.Proposed;
                        item.OsloStatus = AddressStatus.Proposed.Map();
                    },
                    ct);
            });

            When<Envelope<AddressRegularizationWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.OfficiallyAssigned = false;
                        item.Status = AddressStatus.Current;
                        item.OsloStatus = AddressStatus.Current.Map();
                    },
                    ct);
            });

            When<Envelope<AddressDeregulationWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item => { item.OfficiallyAssigned = true; },
                    ct);
            });

            When<Envelope<AddressRemovalWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        var geometry = WKBReaderFactory.CreateForLegacy().Read(message.Message.ExtendedWkbGeometry.ToByteArray());

                        item.Status = message.Message.Status;
                        item.OsloStatus = message.Message.Status.Map();
                        item.PostalCode = message.Message.PostalCode;
                        item.HouseNumber = message.Message.HouseNumber;
                        item.BoxNumber = message.Message.BoxNumber;
                        item.Geometry = geometry;
                        item.PositionMethod = message.Message.GeometryMethod;
                        item.OsloPositionMethod = message.Message.GeometryMethod.ToPositieGeometrieMethode();
                        item.PositionSpecification = message.Message.GeometrySpecification;
                        item.OsloPositionSpecification = message.Message.GeometrySpecification.ToPositieSpecificatie();
                        item.OfficiallyAssigned = message.Message.OfficiallyAssigned;
                        item.ParentPersistentLocalId = message.Message.ParentPersistentLocalId;
                        item.Removed = false;
                    },
                    ct);
            });

            When<Envelope<MigratedStreetNameWasImported>>(DoNothing);
            When<Envelope<StreetNameWasImported>>(DoNothing);
            When<Envelope<StreetNameWasApproved>>(DoNothing);
            When<Envelope<StreetNameWasCorrectedFromApprovedToProposed>>(DoNothing);
            When<Envelope<StreetNameWasCorrectedFromRetiredToCurrent>>(DoNothing);
            When<Envelope<StreetNameWasCorrectedFromRejectedToProposed>>(DoNothing);
            When<Envelope<StreetNameWasRejected>>(DoNothing);
            When<Envelope<StreetNameWasRejectedBecauseOfMunicipalityMerger>>(DoNothing);
            When<Envelope<StreetNameWasRetired>>(DoNothing);
            When<Envelope<StreetNameWasRetiredBecauseOfMunicipalityMerger>>(DoNothing);
            When<Envelope<StreetNameWasRemoved>>(DoNothing);
            When<Envelope<StreetNameWasReaddressed>>(DoNothing);
            When<Envelope<StreetNameWasRenamed>>(DoNothing);
            When<Envelope<StreetNameNamesWereChanged>>(DoNothing);
            When<Envelope<StreetNameNamesWereCorrected>>(DoNothing);
            When<Envelope<StreetNameHomonymAdditionsWereCorrected>>(DoNothing);
            When<Envelope<StreetNameHomonymAdditionsWereRemoved>>(DoNothing);

            #region Legacy

            When<Envelope<AddressWasRegistered>>(async (context, message, ct) =>
            {
                var addressPersistentLocalId = await eventsRepository.GetAddressPersistentLocalId(message.Message.AddressId);

                if (!addressPersistentLocalId.HasValue)
                {
                    throw new InvalidOperationException($"No persistent local id found for {message.Message.AddressId}");
                }

                var address = new AddressVersion
                {
                    Position = message.Position,
                    PersistentLocalId = addressPersistentLocalId.Value,
                    AddressId = message.Message.AddressId,
                    StreetNameId = message.Message.StreetNameId,
                    HouseNumber = message.Message.HouseNumber,
                    PuriId = $"{options.Value.Namespace}/{addressPersistentLocalId}",
                    Namespace = options.Value.Namespace,
                    VersionTimestamp = message.Message.Provenance.Timestamp,
                    CreatedOnTimestamp = message.Message.Provenance.Timestamp,
                    Type = message.EventName
                };

                await context.AddressVersions.AddAsync(address, ct);
            });

            When<Envelope<AddressBecameComplete>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    _ => { },
                    ct);
            });

            When<Envelope<AddressBecameCurrent>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item =>
                    {
                        item.Status = AddressStatus.Current;
                        item.OsloStatus = AddressStatus.Current.Map();
                    },
                    ct);
            });

            When<Envelope<AddressBecameIncomplete>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    _ => { },
                    ct);
            });

            When<Envelope<AddressBecameNotOfficiallyAssigned>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item => { item.OfficiallyAssigned = false; },
                    ct);
            });

            When<Envelope<AddressHouseNumberWasChanged>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item => { item.HouseNumber = message.Message.HouseNumber; },
                    ct);
            });

            When<Envelope<AddressHouseNumberWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item => { item.HouseNumber = message.Message.HouseNumber; },
                    ct);
            });

            When<Envelope<AddressOfficialAssignmentWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item => { item.OfficiallyAssigned = false; },
                    ct);
            });

            When<Envelope<AddressPersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item =>
                    {
                        item.PersistentLocalId = message.Message.PersistentLocalId;
                    },
                    ct);
            });

            When<Envelope<AddressPositionWasCorrected>>(async (context, message, ct) =>
            {
                var geometry = WKBReaderFactory.CreateForLegacy()
                    .Read(message.Message.ExtendedWkbGeometry.ToByteArray());

                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item =>
                    {
                        item.PositionMethod = message.Message.GeometryMethod.ToGeometryMethod();
                        item.OsloPositionMethod = message.Message.GeometryMethod.ToPositieGeometrieMethode();
                        item.PositionSpecification = message.Message.GeometrySpecification.ToGeometrySpecification();
                        item.OsloPositionSpecification = message.Message.GeometrySpecification.ToPositieSpecificatie();
                        item.Geometry = geometry;
                    },
                    ct);
            });

            When<Envelope<AddressPositionWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item =>
                    {
                        item.Geometry = null;
                        item.PositionMethod = null;
                        item.OsloPositionMethod = null;
                        item.PositionSpecification = null;
                        item.OsloPositionSpecification = null;
                    },
                    ct);
            });

            When<Envelope<AddressPostalCodeWasChanged>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item => { item.PostalCode = message.Message.PostalCode; },
                    ct);
            });

            When<Envelope<AddressPostalCodeWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item => { item.PostalCode = message.Message.PostalCode; },
                    ct);
            });

            When<Envelope<AddressPostalCodeWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item => { item.PostalCode = null; },
                    ct);
            });

            When<Envelope<AddressStatusWasCorrectedToRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item => { item.Status = null; },
                    ct);
            });

            When<Envelope<AddressStatusWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item => { item.Status = null; },
                    ct);
            });

            When<Envelope<AddressStreetNameWasChanged>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item =>
                    {
                        item.StreetNameId = message.Message.StreetNameId;
                    },
                    ct);
            });

            When<Envelope<AddressStreetNameWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item =>
                    {
                        item.StreetNameId = message.Message.StreetNameId;
                    },
                    ct);
            });

            When<Envelope<AddressWasCorrectedToCurrent>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item =>
                    {
                        item.Status = AddressStatus.Current;
                        item.OsloStatus = AddressStatus.Current.Map();
                    },
                    ct);
            });

            When<Envelope<AddressWasCorrectedToNotOfficiallyAssigned>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item => { item.OfficiallyAssigned = false; },
                    ct);
            });

            When<Envelope<AddressWasCorrectedToOfficiallyAssigned>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item => { item.OfficiallyAssigned = true; },
                    ct);
            });

            When<Envelope<AddressWasCorrectedToProposed>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item =>
                    {
                        item.Status = AddressStatus.Proposed;
                        item.OsloStatus = AddressStatus.Proposed.Map();
                    },
                    ct);
            });

            When<Envelope<AddressWasCorrectedToRetired>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item =>
                    {
                        item.Status = AddressStatus.Retired;
                        item.OsloStatus = AddressStatus.Retired.Map();
                    },
                    ct);
            });

            When<Envelope<AddressWasOfficiallyAssigned>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item => { item.OfficiallyAssigned = true; },
                    ct);
            });

            When<Envelope<AddressWasPositioned>>(async (context, message, ct) =>
            {
                var geometry = WKBReaderFactory.CreateForLegacy()
                    .Read(message.Message.ExtendedWkbGeometry.ToByteArray());

                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item =>
                    {
                        item.PositionMethod = message.Message.GeometryMethod.ToGeometryMethod();
                        item.OsloPositionMethod = message.Message.GeometryMethod.ToPositieGeometrieMethode();
                        item.PositionSpecification = message.Message.GeometrySpecification.ToGeometrySpecification();
                        item.OsloPositionSpecification = message.Message.GeometrySpecification.ToPositieSpecificatie();
                        item.Geometry = geometry;
                    },
                    ct);
            });

            When<Envelope<AddressWasProposed>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item =>
                    {
                        item.Status = AddressStatus.Proposed;
                        item.OsloStatus = AddressStatus.Proposed.Map();
                    },
                    ct);
            });

            When<Envelope<AddressWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item => { item.Removed = true; },
                    ct);
            });

            When<Envelope<AddressWasRetired>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item =>
                    {
                        item.Status = AddressStatus.Retired;
                        item.OsloStatus = AddressStatus.Retired.Map();
                    },
                    ct);
            });

            When<Envelope<AddressBoxNumberWasChanged>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item => { item.BoxNumber = message.Message.BoxNumber; },
                    ct);
            });
            When<Envelope<AddressBoxNumberWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item => { item.BoxNumber = message.Message.BoxNumber; },
                    ct);
            });
            When<Envelope<AddressBoxNumberWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item => { item.BoxNumber = null; },
                    ct);
            });

            #endregion
        }

        private static Task DoNothing<T>(IntegrationContext context, Envelope<T> envelope, CancellationToken ct) where T: IMessage => Task.CompletedTask;
    }
}
