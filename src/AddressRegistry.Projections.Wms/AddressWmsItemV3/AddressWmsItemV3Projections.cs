namespace AddressRegistry.Projections.Wms.AddressWmsItemV3
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.StreetName.Events;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;
    using NodaTime;
    using AddressStatus = StreetName.AddressStatus;
    using GeometryMethod = StreetName.GeometryMethod;
    using GeometrySpecification = StreetName.GeometrySpecification;

    [ConnectedProjectionName("WMS adressen")]
    [ConnectedProjectionDescription("Projectie die de adressen data voor het WMS adressenregister voorziet.")]
    public class AddressWmsItemV3Projections : ConnectedProjection<WmsContext>
    {
        public static readonly string AdresStatusInGebruik = AdresStatus.InGebruik.ToString();
        public static readonly string AdresStatusGehistoreerd = AdresStatus.Gehistoreerd.ToString();
        public static readonly string AdresStatusVoorgesteld = AdresStatus.Voorgesteld.ToString();
        public static readonly string AdresStatusAfgekeurd = AdresStatus.Afgekeurd.ToString();

        private readonly WKBReader _wkbReader;

        public AddressWmsItemV3Projections(WKBReader wkbReader, IHouseNumberLabelUpdater houseNumberLabelUpdater)
        {
            _wkbReader = wkbReader;

            #region StreetName

            When<Envelope<StreetNameNamesWereChanged>>(async (context, message, ct) =>
            {
                foreach (var addressPersistentLocalId in message.Message.AddressPersistentLocalIds)
                {
                    await context.FindAndUpdateAddressDetailV3(
                        addressPersistentLocalId,
                        address => { UpdateVersionTimestampIfNewer(address, message.Message.Provenance.Timestamp); },
                        houseNumberLabelUpdater,
                        updateHouseNumberLabelsBeforeAddressUpdate: false,
                        updateHouseNumberLabelsAfterAddressUpdate: false,
                        allowUpdateRemovedAddress: true, ct: ct);
                }
            });

            When<Envelope<StreetNameNamesWereCorrected>>(async (context, message, ct) =>
            {
                foreach (var addressPersistentLocalId in message.Message.AddressPersistentLocalIds)
                {
                    await context.FindAndUpdateAddressDetailV3(
                        addressPersistentLocalId,
                        address => { UpdateVersionTimestampIfNewer(address, message.Message.Provenance.Timestamp); },
                        houseNumberLabelUpdater,
                        updateHouseNumberLabelsBeforeAddressUpdate: false,
                        updateHouseNumberLabelsAfterAddressUpdate: false,
                        allowUpdateRemovedAddress: true, ct: ct);
                }
            });

            When<Envelope<StreetNameHomonymAdditionsWereCorrected>>(async (context, message, ct) =>
            {
                foreach (var addressPersistentLocalId in message.Message.AddressPersistentLocalIds)
                {
                    await context.FindAndUpdateAddressDetailV3(
                        addressPersistentLocalId,
                        address => { UpdateVersionTimestampIfNewer(address, message.Message.Provenance.Timestamp); },
                        houseNumberLabelUpdater,
                        updateHouseNumberLabelsBeforeAddressUpdate: false,
                        updateHouseNumberLabelsAfterAddressUpdate: false,
                        allowUpdateRemovedAddress: true, ct: ct);
                }
            });

            When<Envelope<StreetNameHomonymAdditionsWereRemoved>>(async (context, message, ct) =>
            {
                foreach (var addressPersistentLocalId in message.Message.AddressPersistentLocalIds)
                {
                    await context.FindAndUpdateAddressDetailV3(
                        addressPersistentLocalId,
                        address => { UpdateVersionTimestampIfNewer(address, message.Message.Provenance.Timestamp); },
                        houseNumberLabelUpdater,
                        updateHouseNumberLabelsBeforeAddressUpdate: false,
                        updateHouseNumberLabelsAfterAddressUpdate: false,
                        allowUpdateRemovedAddress: true, ct: ct);
                }
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
            #endregion StreetName

            // Address
            When<Envelope<AddressWasMigratedToStreetName>>(async (context, message, ct) =>
            {
                var addressWmsItem = new AddressWmsItemV3(
                    message.Message.AddressPersistentLocalId,
                    message.Message.ParentPersistentLocalId,
                    message.Message.StreetNamePersistentLocalId,
                    message.Message.PostalCode,
                    message.Message.HouseNumber,
                    message.Message.BoxNumber,
                    MapStatus(message.Message.Status),
                    message.Message.OfficiallyAssigned,
                    ParsePosition(message.Message.ExtendedWkbGeometry),
                    ConvertGeometryMethodToString(message.Message.GeometryMethod),
                    ConvertGeometrySpecificationToString(message.Message.GeometrySpecification),
                    message.Message.IsRemoved,
                    message.Message.Provenance.Timestamp);

                await houseNumberLabelUpdater.UpdateHouseNumberLabels(context, addressWmsItem, ct, includeAddressInUpdate: true);

                await context
                    .AddressWmsItemsV3
                    .AddAsync(addressWmsItem, ct);

                if (message.Message.ParentPersistentLocalId.HasValue)
                {
                    var parent = await context.FindAddressDetailV3(message.Message.ParentPersistentLocalId.Value, ct);
                    if (parent.Position == addressWmsItem.Position)
                    {
                        await context.FindAndUpdateAddressDetailV3(
                            message.Message.ParentPersistentLocalId.Value,
                            address => { },
                            houseNumberLabelUpdater,
                            updateHouseNumberLabelsBeforeAddressUpdate: false,
                            updateHouseNumberLabelsAfterAddressUpdate: false, ct: ct);
                    }
                }
            });

            When<Envelope<AddressWasProposedV2>>(async (context, message, ct) =>
            {
                var addressWmsItem = new AddressWmsItemV3(
                    message.Message.AddressPersistentLocalId,
                    message.Message.ParentPersistentLocalId,
                    message.Message.StreetNamePersistentLocalId,
                    message.Message.PostalCode,
                    message.Message.HouseNumber,
                    message.Message.BoxNumber,
                    MapStatus(AddressStatus.Proposed),
                    officiallyAssigned: true,
                    ParsePosition(message.Message.ExtendedWkbGeometry),
                    ConvertGeometryMethodToString(message.Message.GeometryMethod),
                    ConvertGeometrySpecificationToString(message.Message.GeometrySpecification),
                    removed: false,
                    message.Message.Provenance.Timestamp);

                await houseNumberLabelUpdater.UpdateHouseNumberLabels(context, addressWmsItem, ct, includeAddressInUpdate: true);

                await context
                    .AddressWmsItemsV3
                    .AddAsync(addressWmsItem, ct);

                if (message.Message.ParentPersistentLocalId.HasValue)
                {
                    var parent = await context.FindAddressDetailV3(message.Message.ParentPersistentLocalId.Value, ct);
                    if (parent.Position == addressWmsItem.Position)
                    {
                        await context.FindAndUpdateAddressDetailV3(
                            message.Message.ParentPersistentLocalId.Value,
                            address => { },
                            houseNumberLabelUpdater,
                            updateHouseNumberLabelsBeforeAddressUpdate: false,
                            updateHouseNumberLabelsAfterAddressUpdate: false, ct: ct);
                    }
                }
            });

            When<Envelope<AddressWasProposedForMunicipalityMerger>>(async (context, message, ct) =>
            {
                var addressWmsItem = new AddressWmsItemV3(
                    message.Message.AddressPersistentLocalId,
                    message.Message.ParentPersistentLocalId,
                    message.Message.StreetNamePersistentLocalId,
                    message.Message.PostalCode,
                    message.Message.HouseNumber,
                    message.Message.BoxNumber,
                    MapStatus(AddressStatus.Proposed),
                    officiallyAssigned: message.Message.OfficiallyAssigned,
                    ParsePosition(message.Message.ExtendedWkbGeometry),
                    ConvertGeometryMethodToString(message.Message.GeometryMethod),
                    ConvertGeometrySpecificationToString(message.Message.GeometrySpecification),
                    removed: false,
                    message.Message.Provenance.Timestamp);

                await houseNumberLabelUpdater.UpdateHouseNumberLabels(context, addressWmsItem, ct, includeAddressInUpdate: true);

                await context
                    .AddressWmsItemsV3
                    .AddAsync(addressWmsItem, ct);

                if (message.Message.ParentPersistentLocalId.HasValue)
                {
                    var parent = await context.FindAddressDetailV3(message.Message.ParentPersistentLocalId.Value, ct);
                    if (parent.Position == addressWmsItem.Position)
                    {
                        await context.FindAndUpdateAddressDetailV3(
                            message.Message.ParentPersistentLocalId.Value,
                            address => { },
                            houseNumberLabelUpdater,
                            updateHouseNumberLabelsBeforeAddressUpdate: false,
                            updateHouseNumberLabelsAfterAddressUpdate: false, ct: ct);
                    }
                }
            });

            When<Envelope<AddressWasApproved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetailV3(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.Status = MapStatus(AddressStatus.Current);
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    houseNumberLabelUpdater,
                    updateHouseNumberLabelsBeforeAddressUpdate: true,
                    updateHouseNumberLabelsAfterAddressUpdate: true, ct: ct);
            });

            When<Envelope<AddressWasCorrectedFromApprovedToProposed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetailV3(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.Status = MapStatus(AddressStatus.Proposed);
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    houseNumberLabelUpdater,
                    updateHouseNumberLabelsBeforeAddressUpdate: true,
                    updateHouseNumberLabelsAfterAddressUpdate: true, ct: ct);
            });

            When<Envelope<AddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetailV3(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.Status = MapStatus(AddressStatus.Proposed);
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    houseNumberLabelUpdater,
                    updateHouseNumberLabelsBeforeAddressUpdate: true,
                    updateHouseNumberLabelsAfterAddressUpdate: true, ct: ct);
            });

            When<Envelope<AddressWasRejected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetailV3(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.Status = MapStatus(AddressStatus.Rejected);
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    houseNumberLabelUpdater,
                    updateHouseNumberLabelsBeforeAddressUpdate: true,
                    updateHouseNumberLabelsAfterAddressUpdate: true, ct: ct);
            });

            When<Envelope<AddressWasRejectedBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetailV3(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.Status = MapStatus(AddressStatus.Rejected);
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    houseNumberLabelUpdater,
                    updateHouseNumberLabelsBeforeAddressUpdate: true,
                    updateHouseNumberLabelsAfterAddressUpdate: true, ct: ct);
            });

            When<Envelope<AddressWasRejectedBecauseHouseNumberWasRejected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetailV3(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.Status = MapStatus(AddressStatus.Rejected);
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    houseNumberLabelUpdater,
                    updateHouseNumberLabelsBeforeAddressUpdate: true,
                    updateHouseNumberLabelsAfterAddressUpdate: true, ct: ct);
            });

            When<Envelope<AddressWasRejectedBecauseHouseNumberWasRetired>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetailV3(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.Status = MapStatus(AddressStatus.Rejected);
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    houseNumberLabelUpdater,
                    updateHouseNumberLabelsBeforeAddressUpdate: true,
                    updateHouseNumberLabelsAfterAddressUpdate: true, ct: ct);
            });

            When<Envelope<AddressWasRejectedBecauseStreetNameWasRejected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetailV3(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.Status = MapStatus(AddressStatus.Rejected);
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    houseNumberLabelUpdater,
                    updateHouseNumberLabelsBeforeAddressUpdate: true,
                    updateHouseNumberLabelsAfterAddressUpdate: true, ct: ct);
            });

            When<Envelope<AddressWasRetiredBecauseStreetNameWasRejected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetailV3(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.Status = MapStatus(AddressStatus.Retired);
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    houseNumberLabelUpdater,
                    updateHouseNumberLabelsBeforeAddressUpdate: true,
                    updateHouseNumberLabelsAfterAddressUpdate: true, ct: ct);
            });

            When<Envelope<AddressWasRejectedBecauseStreetNameWasRetired>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetailV3(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.Status = MapStatus(AddressStatus.Rejected);
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    houseNumberLabelUpdater,
                    updateHouseNumberLabelsBeforeAddressUpdate: true,
                    updateHouseNumberLabelsAfterAddressUpdate: true, ct: ct);
            });

            When<Envelope<AddressWasCorrectedFromRejectedToProposed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetailV3(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.Status = MapStatus(AddressStatus.Proposed);
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    houseNumberLabelUpdater,
                    updateHouseNumberLabelsBeforeAddressUpdate: true,
                    updateHouseNumberLabelsAfterAddressUpdate: true, ct: ct);
            });

            When<Envelope<AddressWasDeregulated>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetailV3(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.OfficiallyAssigned = false;
                        address.Status = MapStatus(AddressStatus.Current);
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    houseNumberLabelUpdater,
                    updateHouseNumberLabelsBeforeAddressUpdate: true,
                    updateHouseNumberLabelsAfterAddressUpdate: true, ct: ct);
            });

            When<Envelope<AddressWasRegularized>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetailV3(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.OfficiallyAssigned = true;
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    houseNumberLabelUpdater,
                    updateHouseNumberLabelsBeforeAddressUpdate: false,
                    updateHouseNumberLabelsAfterAddressUpdate: false, ct: ct);
            });

            When<Envelope<AddressWasRetiredV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetailV3(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.Status = MapStatus(AddressStatus.Retired);
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    houseNumberLabelUpdater,
                    updateHouseNumberLabelsBeforeAddressUpdate: true,
                    updateHouseNumberLabelsAfterAddressUpdate: true, ct: ct);
            });

            When<Envelope<AddressWasRetiredBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetailV3(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.Status = MapStatus(AddressStatus.Retired);
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    houseNumberLabelUpdater,
                    updateHouseNumberLabelsBeforeAddressUpdate: true,
                    updateHouseNumberLabelsAfterAddressUpdate: true, ct: ct);
            });

            When<Envelope<AddressWasRetiredBecauseHouseNumberWasRetired>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetailV3(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.Status = MapStatus(AddressStatus.Retired);
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    houseNumberLabelUpdater,
                    updateHouseNumberLabelsBeforeAddressUpdate: true,
                    updateHouseNumberLabelsAfterAddressUpdate: true, ct: ct);
            });

            When<Envelope<AddressWasRetiredBecauseStreetNameWasRetired>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetailV3(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.Status = MapStatus(AddressStatus.Retired);
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    houseNumberLabelUpdater,
                    updateHouseNumberLabelsBeforeAddressUpdate: true,
                    updateHouseNumberLabelsAfterAddressUpdate: true, ct: ct);
            });

            When<Envelope<AddressWasCorrectedFromRetiredToCurrent>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetailV3(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.Status = MapStatus(AddressStatus.Current);
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    houseNumberLabelUpdater,
                    updateHouseNumberLabelsBeforeAddressUpdate: true,
                    updateHouseNumberLabelsAfterAddressUpdate: true, ct: ct);
            });

            When<Envelope<AddressPostalCodeWasChangedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetailV3(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.PostalCode = message.Message.PostalCode;
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    houseNumberLabelUpdater,
                    updateHouseNumberLabelsBeforeAddressUpdate: false,
                    updateHouseNumberLabelsAfterAddressUpdate: false, ct: ct);

                foreach (var boxNumberPersistentLocalId in message.Message.BoxNumberPersistentLocalIds)
                {
                    await context.FindAndUpdateAddressDetailV3(
                        boxNumberPersistentLocalId,
                        address =>
                        {
                            address.PostalCode = message.Message.PostalCode;
                            UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                        },
                        houseNumberLabelUpdater,
                        updateHouseNumberLabelsBeforeAddressUpdate: false,
                        updateHouseNumberLabelsAfterAddressUpdate: false, ct: ct);
                }
            });

            When<Envelope<AddressPostalCodeWasCorrectedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetailV3(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.PostalCode = message.Message.PostalCode;
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    houseNumberLabelUpdater,
                    updateHouseNumberLabelsBeforeAddressUpdate: false,
                    updateHouseNumberLabelsAfterAddressUpdate: false, ct: ct);

                foreach (var boxNumberPersistentLocalId in message.Message.BoxNumberPersistentLocalIds)
                {
                    await context.FindAndUpdateAddressDetailV3(
                        boxNumberPersistentLocalId,
                        address =>
                        {
                            address.PostalCode = message.Message.PostalCode;
                            UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                        },
                        houseNumberLabelUpdater,
                        updateHouseNumberLabelsBeforeAddressUpdate: false,
                        updateHouseNumberLabelsAfterAddressUpdate: false, ct: ct);
                }
            });

            When<Envelope<AddressHouseNumberWasCorrectedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetailV3(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.HouseNumber = message.Message.HouseNumber;
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    houseNumberLabelUpdater,
                    updateHouseNumberLabelsBeforeAddressUpdate: false,
                    updateHouseNumberLabelsAfterAddressUpdate: true, ct: ct);

                foreach (var boxNumberPersistentLocalId in message.Message.BoxNumberPersistentLocalIds)
                {
                    await context.FindAndUpdateAddressDetailV3(
                        boxNumberPersistentLocalId,
                        address =>
                        {
                            address.HouseNumber = message.Message.HouseNumber;
                            UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                        },
                        houseNumberLabelUpdater,
                        updateHouseNumberLabelsBeforeAddressUpdate: false,
                        updateHouseNumberLabelsAfterAddressUpdate: true, ct: ct);
                }
            });

            When<Envelope<AddressBoxNumberWasCorrectedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetailV3(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.BoxNumber = message.Message.BoxNumber;
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    houseNumberLabelUpdater,
                    updateHouseNumberLabelsBeforeAddressUpdate: false,
                    updateHouseNumberLabelsAfterAddressUpdate: false, ct: ct);
            });

            When<Envelope<AddressBoxNumbersWereCorrected>>(async (context, message, ct) =>
            {
                foreach (var addressBoxNumber in message.Message.AddressBoxNumbers)
                {
                    await context.FindAndUpdateAddressDetailV3(
                        addressBoxNumber.Key,
                        address =>
                        {
                            address.BoxNumber = addressBoxNumber.Value;
                            UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                        },
                        houseNumberLabelUpdater,
                        updateHouseNumberLabelsBeforeAddressUpdate: false,
                        updateHouseNumberLabelsAfterAddressUpdate: false, ct: ct);
                }
            });

            When<Envelope<AddressPositionWasChanged>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetailV3(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.PositionMethod = ConvertGeometryMethodToString(message.Message.GeometryMethod);
                        address.PositionSpecification = ConvertGeometrySpecificationToString(message.Message.GeometrySpecification);
                        address.SetPosition(ParsePosition(message.Message.ExtendedWkbGeometry));
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    houseNumberLabelUpdater,
                    updateHouseNumberLabelsBeforeAddressUpdate: true,
                    updateHouseNumberLabelsAfterAddressUpdate: true,
                    allowUpdateRemovedAddress: true, ct: ct);

                var wmsItemV3 = await context.FindAddressDetailV3(message.Message.AddressPersistentLocalId, ct);

                if (wmsItemV3.ParentAddressPersistentLocalId.HasValue)
                {
                    var parent = await context.FindAddressDetailV3(wmsItemV3.ParentAddressPersistentLocalId.Value, ct);
                    if (parent.Position == wmsItemV3.Position)
                    {
                        await context.FindAndUpdateAddressDetailV3(
                            wmsItemV3.ParentAddressPersistentLocalId.Value,
                            address => { },
                            houseNumberLabelUpdater,
                            updateHouseNumberLabelsBeforeAddressUpdate: false,
                            updateHouseNumberLabelsAfterAddressUpdate: false, ct: ct);
                    }
                }
            });

            When<Envelope<AddressPositionWasCorrectedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetailV3(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.PositionMethod = ConvertGeometryMethodToString(message.Message.GeometryMethod);
                        address.PositionSpecification = ConvertGeometrySpecificationToString(message.Message.GeometrySpecification);
                        address.SetPosition(ParsePosition(message.Message.ExtendedWkbGeometry));
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    houseNumberLabelUpdater,
                    updateHouseNumberLabelsBeforeAddressUpdate: true,
                    updateHouseNumberLabelsAfterAddressUpdate: true,
                    allowUpdateRemovedAddress: true, ct: ct);

                var wmsItemV3 = await context.FindAddressDetailV3(message.Message.AddressPersistentLocalId, ct);

                if (wmsItemV3.ParentAddressPersistentLocalId.HasValue)
                {
                    var parent = await context.FindAddressDetailV3(wmsItemV3.ParentAddressPersistentLocalId.Value, ct);
                    if (parent.Position == wmsItemV3.Position)
                    {
                        await context.FindAndUpdateAddressDetailV3(
                            wmsItemV3.ParentAddressPersistentLocalId.Value,
                            address => { },
                            houseNumberLabelUpdater,
                            updateHouseNumberLabelsBeforeAddressUpdate: false,
                            updateHouseNumberLabelsAfterAddressUpdate: false, ct: ct);
                    }
                }
            });

            When<Envelope<AddressHouseNumberWasReaddressed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetailV3(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.Status = MapStatus(message.Message.ReaddressedHouseNumber.SourceStatus);
                        address.HouseNumber = message.Message.ReaddressedHouseNumber.DestinationHouseNumber;
                        address.PostalCode = message.Message.ReaddressedHouseNumber.SourcePostalCode;
                        address.OfficiallyAssigned = message.Message.ReaddressedHouseNumber.SourceIsOfficiallyAssigned;
                        address.PositionMethod = ConvertGeometryMethodToString(message.Message.ReaddressedHouseNumber.SourceGeometryMethod);
                        address.PositionSpecification =
                            ConvertGeometrySpecificationToString(message.Message.ReaddressedHouseNumber.SourceGeometrySpecification);
                        address.SetPosition(ParsePosition(message.Message.ReaddressedHouseNumber.SourceExtendedWkbGeometry));
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    houseNumberLabelUpdater,
                    updateHouseNumberLabelsBeforeAddressUpdate: true,
                    updateHouseNumberLabelsAfterAddressUpdate: true, ct: ct);

                foreach (var readdressedBoxNumber in message.Message.ReaddressedBoxNumbers)
                {
                    await context.FindAndUpdateAddressDetailV3(
                        readdressedBoxNumber.DestinationAddressPersistentLocalId,
                        address =>
                        {
                            address.Status = MapStatus(readdressedBoxNumber.SourceStatus);
                            address.HouseNumber = readdressedBoxNumber.DestinationHouseNumber;
                            address.BoxNumber = readdressedBoxNumber.SourceBoxNumber;
                            address.PostalCode = readdressedBoxNumber.SourcePostalCode;
                            address.OfficiallyAssigned = readdressedBoxNumber.SourceIsOfficiallyAssigned;
                            address.PositionMethod = ConvertGeometryMethodToString(readdressedBoxNumber.SourceGeometryMethod);
                            address.PositionSpecification = ConvertGeometrySpecificationToString(readdressedBoxNumber.SourceGeometrySpecification);
                            address.SetPosition(ParsePosition(readdressedBoxNumber.SourceExtendedWkbGeometry));
                            UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                        },
                        houseNumberLabelUpdater,
                        updateHouseNumberLabelsBeforeAddressUpdate: true,
                        updateHouseNumberLabelsAfterAddressUpdate: true, ct: ct);
                }
            });

            When<Envelope<AddressWasProposedBecauseOfReaddress>>(async (context, message, ct) =>
            {
                var addressWmsItem = new AddressWmsItemV3(
                    message.Message.AddressPersistentLocalId,
                    message.Message.ParentPersistentLocalId,
                    message.Message.StreetNamePersistentLocalId,
                    message.Message.PostalCode,
                    message.Message.HouseNumber,
                    message.Message.BoxNumber,
                    MapStatus(AddressStatus.Proposed),
                    officiallyAssigned: true,
                    ParsePosition(message.Message.ExtendedWkbGeometry),
                    ConvertGeometryMethodToString(message.Message.GeometryMethod),
                    ConvertGeometrySpecificationToString(message.Message.GeometrySpecification),
                    removed: false,
                    message.Message.Provenance.Timestamp);

                await houseNumberLabelUpdater.UpdateHouseNumberLabels(context, addressWmsItem, ct, includeAddressInUpdate: true);

                await context
                    .AddressWmsItemsV3
                    .AddAsync(addressWmsItem, ct);

                if (message.Message.ParentPersistentLocalId.HasValue)
                {
                    var parent = await context.FindAddressDetailV3(message.Message.ParentPersistentLocalId.Value, ct);
                    if (parent.Position == addressWmsItem.Position)
                    {
                        await context.FindAndUpdateAddressDetailV3(
                            message.Message.ParentPersistentLocalId.Value,
                            address => { },
                            houseNumberLabelUpdater,
                            updateHouseNumberLabelsBeforeAddressUpdate: false,
                            updateHouseNumberLabelsAfterAddressUpdate: false, ct: ct);
                    }
                }
            });

            When<Envelope<AddressWasRejectedBecauseOfReaddress>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetailV3(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.Status = MapStatus(AddressStatus.Rejected);
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    houseNumberLabelUpdater,
                    updateHouseNumberLabelsBeforeAddressUpdate: true,
                    updateHouseNumberLabelsAfterAddressUpdate: true, ct: ct);
            });

            When<Envelope<AddressWasRetiredBecauseOfReaddress>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetailV3(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.Status = MapStatus(AddressStatus.Retired);
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    houseNumberLabelUpdater,
                    updateHouseNumberLabelsBeforeAddressUpdate: true,
                    updateHouseNumberLabelsAfterAddressUpdate: true, ct: ct);
            });

            When<Envelope<AddressWasRemovedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetailV3(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.Removed = true;
                        address.SetHouseNumberLabel(null, address.LabelType);
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    houseNumberLabelUpdater,
                    updateHouseNumberLabelsBeforeAddressUpdate: true,
                    updateHouseNumberLabelsAfterAddressUpdate: false,
                    allowUpdateRemovedAddress: true, ct: ct);
            });

            When<Envelope<AddressWasRemovedBecauseStreetNameWasRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetailV3(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.Removed = true;
                        address.SetHouseNumberLabel(null, address.LabelType);
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    houseNumberLabelUpdater,
                    updateHouseNumberLabelsBeforeAddressUpdate: true,
                    updateHouseNumberLabelsAfterAddressUpdate: false,
                    allowUpdateRemovedAddress: true, ct: ct);
            });

            When<Envelope<AddressWasRemovedBecauseHouseNumberWasRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetailV3(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.Removed = true;
                        address.SetHouseNumberLabel(null, address.LabelType);
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    houseNumberLabelUpdater,
                    updateHouseNumberLabelsBeforeAddressUpdate: true,
                    updateHouseNumberLabelsAfterAddressUpdate: false,
                    allowUpdateRemovedAddress: true, ct: ct);
            });

            When<Envelope<AddressRegularizationWasCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetailV3(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.OfficiallyAssigned = false;
                        address.Status = MapStatus(AddressStatus.Current);
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    houseNumberLabelUpdater,
                    updateHouseNumberLabelsBeforeAddressUpdate: true,
                    updateHouseNumberLabelsAfterAddressUpdate: true,
                    allowUpdateRemovedAddress: true, ct: ct);
            });

            When<Envelope<AddressDeregulationWasCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetailV3(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.OfficiallyAssigned = true;
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    houseNumberLabelUpdater,
                    updateHouseNumberLabelsBeforeAddressUpdate: false,
                    updateHouseNumberLabelsAfterAddressUpdate: false,
                    allowUpdateRemovedAddress: true, ct: ct);
            });

            When<Envelope<AddressRemovalWasCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetailV3(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.PostalCode = message.Message.PostalCode;
                        address.HouseNumber = message.Message.HouseNumber;
                        address.BoxNumber = message.Message.BoxNumber;
                        address.Status = MapStatus(message.Message.Status);
                        address.OfficiallyAssigned = message.Message.OfficiallyAssigned;
                        address.SetPosition(ParsePosition(message.Message.ExtendedWkbGeometry));
                        address.PositionMethod = ConvertGeometryMethodToString(message.Message.GeometryMethod)!;
                        address.PositionSpecification = ConvertGeometrySpecificationToString(message.Message.GeometrySpecification)!;
                        address.Removed = false;

                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    houseNumberLabelUpdater,
                    updateHouseNumberLabelsBeforeAddressUpdate: false,
                    updateHouseNumberLabelsAfterAddressUpdate: true, allowUpdateRemovedAddress: true, ct: ct);
            });
        }

        private static void UpdateVersionTimestamp(AddressWmsItemV3 addressWmsItem, Instant versionTimestamp)
            => addressWmsItem.VersionTimestamp = versionTimestamp;

        private static void UpdateVersionTimestampIfNewer(AddressWmsItemV3 addressWmsItem, Instant versionTimestamp)
        {
            if (versionTimestamp > addressWmsItem.VersionTimestamp)
            {
                addressWmsItem.VersionTimestamp = versionTimestamp;
            }
        }

        public static string MapStatus(AddressStatus status)
        {
            return status switch
            {
                AddressStatus.Proposed => AdresStatusVoorgesteld,
                AddressStatus.Current => AdresStatusInGebruik,
                AddressStatus.Retired => AdresStatusGehistoreerd,
                AddressStatus.Rejected => AdresStatusAfgekeurd,
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
            };
        }

        private Point ParsePosition(string extendedWkbGeometry)
            => (Point)_wkbReader.Read(extendedWkbGeometry.ToByteArray());

        public static string ConvertGeometryMethodToString(GeometryMethod method) =>
            MapGeometryMethodToPositieGeometrieMethode(method)
                .ToString()
                .Replace("Geinterpoleerd", "Geïnterpoleerd");

        private static PositieGeometrieMethode MapGeometryMethodToPositieGeometrieMethode(
            GeometryMethod geometryMethod)
        {
            return geometryMethod switch
            {
                GeometryMethod.Interpolated => PositieGeometrieMethode.Geinterpoleerd,
                GeometryMethod.AppointedByAdministrator => PositieGeometrieMethode.AangeduidDoorBeheerder,
                GeometryMethod.DerivedFromObject => PositieGeometrieMethode.AfgeleidVanObject,
                _ => throw new ArgumentOutOfRangeException(nameof(geometryMethod), geometryMethod, null)
            };
        }

        public static string ConvertGeometrySpecificationToString(GeometrySpecification specification) =>
            MapGeometrySpecificationToPositieSpecificatie(specification).ToString();

        private static PositieSpecificatie MapGeometrySpecificationToPositieSpecificatie(
            GeometrySpecification geometrySpecification)
        {
            return geometrySpecification switch
            {
                GeometrySpecification.Municipality => PositieSpecificatie.Gemeente,
                GeometrySpecification.Street => PositieSpecificatie.Straat,
                GeometrySpecification.Parcel => PositieSpecificatie.Perceel,
                GeometrySpecification.Lot => PositieSpecificatie.Lot,
                GeometrySpecification.Stand => PositieSpecificatie.Standplaats,
                GeometrySpecification.Berth => PositieSpecificatie.Ligplaats,
                GeometrySpecification.Building => PositieSpecificatie.Gebouw,
                GeometrySpecification.BuildingUnit => PositieSpecificatie.Gebouweenheid,
                GeometrySpecification.Entry => PositieSpecificatie.Ingang,
                GeometrySpecification.RoadSegment => PositieSpecificatie.Wegsegment,
                _ => throw new ArgumentOutOfRangeException(nameof(geometrySpecification), geometrySpecification, null)
            };
        }

        private static Task DoNothing<T>(WmsContext context, Envelope<T> envelope, CancellationToken ct) where T: IMessage => Task.CompletedTask;
    }
}
