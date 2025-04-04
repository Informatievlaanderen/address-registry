namespace AddressRegistry.Projections.Wfs.AddressWfs
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;
    using NodaTime;
    using StreetName;
    using StreetName.Events;

    [ConnectedProjectionName("WFS adressen")]
    [ConnectedProjectionDescription("Projectie die de adressen data voor het WFS adressenregister voorziet.")]
    public class AddressWfsProjections : ConnectedProjection<WfsContext>
    {
        private static readonly string AdresStatusInGebruik = AdresStatus.InGebruik.ToString();
        private static readonly string AdresStatusGehistoreerd = AdresStatus.Gehistoreerd.ToString();
        private static readonly string AdresStatusVoorgesteld = AdresStatus.Voorgesteld.ToString();
        private static readonly string AdresStatusAfgekeurd = AdresStatus.Afgekeurd.ToString();

        private readonly WKBReader _wkbReader;

        public AddressWfsProjections(WKBReader wkbReader)
        {
            _wkbReader = wkbReader;

            #region StreetName

            When<Envelope<StreetNameNamesWereChanged>>(async (context, message, ct) =>
            {
                foreach (var addressPersistentLocalId in message.Message.AddressPersistentLocalIds)
                {
                    var item = await context.FindAndUpdateAddressDetail(
                        addressPersistentLocalId,
                        x =>  { },
                        ct);

                    UpdateVersionTimestampIfNewer(item, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<StreetNameNamesWereCorrected>>(async (context, message, ct) =>
            {
                foreach (var addressPersistentLocalId in message.Message.AddressPersistentLocalIds)
                {
                    var item = await context.FindAndUpdateAddressDetail(
                        addressPersistentLocalId,
                        x =>  { },
                        ct);

                    UpdateVersionTimestampIfNewer(item, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<StreetNameHomonymAdditionsWereCorrected>>(async (context, message, ct) =>
            {
                foreach (var addressPersistentLocalId in message.Message.AddressPersistentLocalIds)
                {
                    var item = await context.FindAndUpdateAddressDetail(
                        addressPersistentLocalId,
                        x =>  { },
                        ct);

                    UpdateVersionTimestampIfNewer(item, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<StreetNameHomonymAdditionsWereRemoved>>(async (context, message, ct) =>
            {
                foreach (var addressPersistentLocalId in message.Message.AddressPersistentLocalIds)
                {
                    var item = await context.FindAndUpdateAddressDetail(
                        addressPersistentLocalId,
                        x =>  { },
                        ct);

                    UpdateVersionTimestampIfNewer(item, message.Message.Provenance.Timestamp);
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
                var addressWfsItem = new AddressWfsItem(
                    message.Message.AddressPersistentLocalId,
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

                await context
                    .AddressWfsItems
                    .AddAsync(addressWfsItem, ct);
            });

            When<Envelope<AddressWasProposedV2>>(async (context, message, ct) =>
            {
                var addressWfsItem = new AddressWfsItem(
                    message.Message.AddressPersistentLocalId,
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

                await context
                    .AddressWfsItems
                    .AddAsync(addressWfsItem, ct);
            });

            When<Envelope<AddressWasProposedForMunicipalityMerger>>(async (context, message, ct) =>
            {
                var addressWfsItem = new AddressWfsItem(
                    message.Message.AddressPersistentLocalId,
                    message.Message.StreetNamePersistentLocalId,
                    message.Message.PostalCode,
                    message.Message.HouseNumber,
                    message.Message.BoxNumber,
                    MapStatus(AddressStatus.Proposed),
                    officiallyAssigned: message.Message.OfficiallyAssigned,
                    ParsePosition(message.Message.ExtendedWkbGeometry),
                    ConvertGeometryMethodToString(message.Message.GeometryMethod)!,
                    ConvertGeometrySpecificationToString(message.Message.GeometrySpecification)!,
                    removed: false,
                    message.Message.Provenance.Timestamp);

                await context
                    .AddressWfsItems
                    .AddAsync(addressWfsItem, ct);
            });

            When<Envelope<AddressWasApproved>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item => item.Status = MapStatus(AddressStatus.Current),
                    ct);

                UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressWasCorrectedFromApprovedToProposed>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item => item.Status = MapStatus(AddressStatus.Proposed),
                    ct);

                UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item => item.Status = MapStatus(AddressStatus.Proposed),
                    ct);

                UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressWasRejected>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item => item.Status = MapStatus(AddressStatus.Rejected),
                    ct);

                UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressWasRejectedBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item => item.Status = MapStatus(AddressStatus.Rejected),
                    ct);

                UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressWasRejectedBecauseHouseNumberWasRejected>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item => item.Status = MapStatus(AddressStatus.Rejected),
                    ct);

                UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressWasRejectedBecauseHouseNumberWasRetired>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item => item.Status = MapStatus(AddressStatus.Rejected),
                    ct);

                UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressWasRejectedBecauseStreetNameWasRejected>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item => item.Status = MapStatus(AddressStatus.Rejected),
                    ct);

                UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressWasRetiredBecauseStreetNameWasRejected>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item => item.Status = MapStatus(AddressStatus.Retired),
                    ct);

                UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressWasRejectedBecauseStreetNameWasRetired>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item => item.Status = MapStatus(AddressStatus.Rejected),
                    ct);

                UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressWasDeregulated>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.OfficiallyAssigned = false;
                        item.Status = AdresStatusInGebruik;
                    },
                    ct);

                UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressWasRegularized>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item => item.OfficiallyAssigned = true,
                    ct);

                UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressWasRetiredV2>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item => item.Status = MapStatus(AddressStatus.Retired),
                    ct);

                UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressWasRetiredBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item => item.Status = MapStatus(AddressStatus.Retired),
                    ct);

                UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressWasRetiredBecauseHouseNumberWasRetired>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item => item.Status = MapStatus(AddressStatus.Retired),
                    ct);

                UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressWasRetiredBecauseStreetNameWasRetired>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item => item.Status = MapStatus(AddressStatus.Retired),
                    ct);

                UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressWasCorrectedFromRetiredToCurrent>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item => item.Status = MapStatus(AddressStatus.Current),
                    ct);

                UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressPostalCodeWasChangedV2>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item => item.PostalCode = message.Message.PostalCode,
                    ct);

                UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);

                foreach (var boxNumberPersistentLocalId in message.Message.BoxNumberPersistentLocalIds)
                {
                    var boxNumberItem = await context.FindAndUpdateAddressDetail(
                        boxNumberPersistentLocalId,
                        x => x.PostalCode = message.Message.PostalCode,
                        ct);

                    UpdateVersionTimestamp(boxNumberItem, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<AddressPostalCodeWasCorrectedV2>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item => item.PostalCode = message.Message.PostalCode,
                    ct);

                UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);

                foreach (var boxNumberPersistentLocalId in message.Message.BoxNumberPersistentLocalIds)
                {
                    var boxNumberItem = await context.FindAndUpdateAddressDetail(
                        boxNumberPersistentLocalId,
                        x => x.PostalCode = message.Message.PostalCode,
                        ct);

                    UpdateVersionTimestamp(boxNumberItem, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<AddressHouseNumberWasCorrectedV2>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item => item.HouseNumber = message.Message.HouseNumber,
                    ct);

                UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);

                foreach (var boxNumberPersistentLocalId in message.Message.BoxNumberPersistentLocalIds)
                {
                    var boxNumberItem = await context.FindAndUpdateAddressDetail(
                        boxNumberPersistentLocalId,
                        x => x.HouseNumber = message.Message.HouseNumber,
                        ct);

                    UpdateVersionTimestamp(boxNumberItem, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<AddressBoxNumberWasCorrectedV2>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item => item.BoxNumber = message.Message.BoxNumber,
                    ct);

                UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressBoxNumbersWereCorrected>>(async (context, message, ct) =>
            {
                foreach (var addressBoxNumber in message.Message.AddressBoxNumbers)
                {
                    await context.FindAndUpdateAddressDetail(
                        addressBoxNumber.Key,
                        address =>
                        {
                            address.BoxNumber = addressBoxNumber.Value;
                            UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                        },
                        ct);
                }
            });

            When<Envelope<AddressPositionWasChanged>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.PositionMethod = ConvertGeometryMethodToString(message.Message.GeometryMethod);
                        item.PositionSpecification = ConvertGeometrySpecificationToString(message.Message.GeometrySpecification);
                        item.Position = ParsePosition(message.Message.ExtendedWkbGeometry);
                    },
                    ct);

                UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressPositionWasCorrectedV2>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.PositionMethod = ConvertGeometryMethodToString(message.Message.GeometryMethod);
                        item.PositionSpecification = ConvertGeometrySpecificationToString(message.Message.GeometrySpecification);
                        item.Position = ParsePosition(message.Message.ExtendedWkbGeometry);
                    },
                    ct);

                UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressHouseNumberWasReaddressed>>(async (context, message, ct) =>
            {
                var houseNumberItem = await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Status = MapStatus(message.Message.ReaddressedHouseNumber.SourceStatus);
                        item.HouseNumber = message.Message.ReaddressedHouseNumber.DestinationHouseNumber;
                        item.PostalCode = message.Message.ReaddressedHouseNumber.SourcePostalCode;
                        item.OfficiallyAssigned = message.Message.ReaddressedHouseNumber.SourceIsOfficiallyAssigned;
                        item.PositionMethod = ConvertGeometryMethodToString(message.Message.ReaddressedHouseNumber.SourceGeometryMethod);
                        item.PositionSpecification = ConvertGeometrySpecificationToString(message.Message.ReaddressedHouseNumber.SourceGeometrySpecification);
                        item.Position = ParsePosition(message.Message.ReaddressedHouseNumber.SourceExtendedWkbGeometry);
                    },
                    ct);

                UpdateVersionTimestamp(houseNumberItem, message.Message.Provenance.Timestamp);

                foreach (var readdressedBoxNumber in message.Message.ReaddressedBoxNumbers)
                {
                    var boxNumberItem = await context.FindAndUpdateAddressDetail(
                        readdressedBoxNumber.DestinationAddressPersistentLocalId,
                        item =>
                        {
                            item.Status = MapStatus(readdressedBoxNumber.SourceStatus);
                            item.HouseNumber = readdressedBoxNumber.DestinationHouseNumber;
                            item.BoxNumber = readdressedBoxNumber.SourceBoxNumber;
                            item.PostalCode = readdressedBoxNumber.SourcePostalCode;
                            item.OfficiallyAssigned = readdressedBoxNumber.SourceIsOfficiallyAssigned;
                            item.PositionMethod = ConvertGeometryMethodToString(readdressedBoxNumber.SourceGeometryMethod);
                            item.PositionSpecification = ConvertGeometrySpecificationToString(readdressedBoxNumber.SourceGeometrySpecification);
                            item.Position = ParsePosition(readdressedBoxNumber.SourceExtendedWkbGeometry);
                        },
                        ct);

                    UpdateVersionTimestamp(boxNumberItem, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<AddressWasProposedBecauseOfReaddress>>(async (context, message, ct) =>
            {
                var addressWfsItem = new AddressWfsItem(
                    message.Message.AddressPersistentLocalId,
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

                await context
                    .AddressWfsItems
                    .AddAsync(addressWfsItem, ct);
            });

            When<Envelope<AddressWasRejectedBecauseOfReaddress>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item => item.Status = MapStatus(AddressStatus.Rejected),
                    ct);

                UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressWasRetiredBecauseOfReaddress>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item => item.Status = MapStatus(AddressStatus.Retired),
                    ct);

                UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressWasRemovedV2>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item => item.Removed = true,
                    ct);

                UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            });

             When<Envelope<AddressWasRemovedBecauseStreetNameWasRemoved>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item => item.Removed = true,
                    ct);

                UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressWasRemovedBecauseHouseNumberWasRemoved>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item => item.Removed = true,
                    ct);

                UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressWasCorrectedFromRejectedToProposed>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item => item.Status = MapStatus(AddressStatus.Proposed),
                    ct);

                UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressRegularizationWasCorrected>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.OfficiallyAssigned = false;
                        item.Status = AdresStatusInGebruik;
                    },
                    ct);

                UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressDeregulationWasCorrected>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.OfficiallyAssigned = true;
                    },
                    ct);

                UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressRemovalWasCorrected>>(async (context, message, ct) =>
            {
                var item = await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.PostalCode = message.Message.PostalCode;
                        item.HouseNumber = message.Message.HouseNumber;
                        item.BoxNumber = message.Message.BoxNumber;
                        item.Status = MapStatus(message.Message.Status);
                        item.OfficiallyAssigned = message.Message.OfficiallyAssigned;
                        item.Position = ParsePosition(message.Message.ExtendedWkbGeometry);
                        item.PositionMethod = ConvertGeometryMethodToString(message.Message.GeometryMethod)!;
                        item.PositionSpecification = ConvertGeometrySpecificationToString(message.Message.GeometrySpecification)!;
                        item.Removed = false;
                    },
                    ct);

                UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            });
        }

        public static string MapStatus(AddressStatus status)
        {
            switch (status)
            {
                case AddressStatus.Proposed: return AdresStatusVoorgesteld;
                case AddressStatus.Current: return AdresStatusInGebruik;
                case AddressStatus.Retired: return AdresStatusGehistoreerd;
                case AddressStatus.Rejected: return AdresStatusAfgekeurd;
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }

        private Point ParsePosition(string extendedWkbGeometry)
            => (Point) _wkbReader.Read(extendedWkbGeometry.ToByteArray());


        private static void UpdateVersionTimestamp(AddressWfsItem addressWfsItem, Instant versionTimestamp)
            => addressWfsItem.VersionTimestamp = versionTimestamp;

        private static void UpdateVersionTimestampIfNewer(AddressWfsItem addressWfsItem, Instant versionTimestamp)
        {
            if(versionTimestamp > addressWfsItem.VersionTimestamp)
            {
                addressWfsItem.VersionTimestamp = versionTimestamp;
            }
        }

        private static PositieGeometrieMethode? MapGeometryMethodToPositieGeometrieMethode(
            GeometryMethod? geometryMethod)
        {
            if (geometryMethod == null)
                return null;

            switch (geometryMethod)
            {
                case GeometryMethod.Interpolated:
                    return PositieGeometrieMethode.Geinterpoleerd;
                case GeometryMethod.AppointedByAdministrator:
                    return PositieGeometrieMethode.AangeduidDoorBeheerder;
                case GeometryMethod.DerivedFromObject:
                    return PositieGeometrieMethode.AfgeleidVanObject;
                default:
                    return null;
            }
        }

        public static string? ConvertGeometryMethodToString(GeometryMethod? method) =>
            MapGeometryMethodToPositieGeometrieMethode(method)?
            .ToString()
            .Replace("Geinterpoleerd", "Geïnterpoleerd");

        private static PositieSpecificatie? MapGeometrySpecificationToPositieSpecificatie(
            GeometrySpecification? geometrySpecification)
        {
            if (geometrySpecification == null)
                return null;

            switch (geometrySpecification)
            {
                case GeometrySpecification.Municipality:
                    return PositieSpecificatie.Gemeente;
                case GeometrySpecification.Street:
                    return PositieSpecificatie.Straat;
                case GeometrySpecification.Parcel:
                    return PositieSpecificatie.Perceel;
                case GeometrySpecification.Lot:
                    return PositieSpecificatie.Lot;
                case GeometrySpecification.Stand:
                    return PositieSpecificatie.Standplaats;
                case GeometrySpecification.Berth:
                    return PositieSpecificatie.Ligplaats;
                case GeometrySpecification.Building:
                    return PositieSpecificatie.Gebouw;
                case GeometrySpecification.BuildingUnit:
                    return PositieSpecificatie.Gebouweenheid;
                case GeometrySpecification.Entry:
                    return PositieSpecificatie.Ingang;
                case GeometrySpecification.RoadSegment:
                    return PositieSpecificatie.Wegsegment;
                default:
                    return null;
            }
        }

        public static string? ConvertGeometrySpecificationToString(GeometrySpecification? specification) =>
            MapGeometrySpecificationToPositieSpecificatie(specification)?.ToString();

        private static Task DoNothing<T>(WfsContext context, Envelope<T> envelope, CancellationToken ct) where T: IMessage => Task.CompletedTask;
    }
}
