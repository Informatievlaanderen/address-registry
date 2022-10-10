namespace AddressRegistry.Projections.Wms.AddressWmsItem
{
    using System;
    using AddressRegistry.StreetName;
    using AddressRegistry.StreetName.Events;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;
    using NodaTime;

    [ConnectedProjectionName("WMS adressen")]
    [ConnectedProjectionDescription("Projectie die de adressen data voor het WMS adressenregister voorziet.")]
    public class AddressWmsItemProjections : ConnectedProjection<WmsContext>
    {
        public static readonly string AdresStatusInGebruik = AdresStatus.InGebruik.ToString();
        public static readonly string AdresStatusGehistoreerd = AdresStatus.Gehistoreerd.ToString();
        public static readonly string AdresStatusVoorgesteld = AdresStatus.Voorgesteld.ToString();
        public static readonly string AdresStatusAfgekeurd = AdresStatus.Afgekeurd.ToString();

        private readonly WKBReader _wkbReader;

        public AddressWmsItemProjections(WKBReader wkbReader)
        {
            _wkbReader = wkbReader;

            When<Envelope<AddressWasMigratedToStreetName>>(async (context, message, ct) =>
            {
                var addressWmsItem = new AddressWmsItem(
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

                await context.UpdateHouseNumberLabels(addressWmsItem, ct, includeAddressInUpdate: true);

                await context
                    .AddressWmsItems
                    .AddAsync(addressWmsItem, ct);
            });

            When<Envelope<AddressWasProposedV2>>(async (context, message, ct) =>
            {
                var addressWmsItem = new AddressWmsItem(
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

                await context.UpdateHouseNumberLabels(addressWmsItem, ct, includeAddressInUpdate: true);

                await context
                    .AddressWmsItems
                    .AddAsync(addressWmsItem, ct);
            });

            When<Envelope<AddressWasApproved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.Status = MapStatus(AddressStatus.Current);
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    ct,
                    updateHouseNumberLabelsBeforeAddressUpdate: true,
                    updateHouseNumberLabelsAfterAddressUpdate: true);
            });

            When<Envelope<AddressWasCorrectedFromApprovedToProposed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.Status =  MapStatus(AddressStatus.Proposed);
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    ct,
                    updateHouseNumberLabelsBeforeAddressUpdate: true,
                    updateHouseNumberLabelsAfterAddressUpdate: true);
            });

            When<Envelope<AddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.Status =  MapStatus(AddressStatus.Proposed);
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    ct,
                    updateHouseNumberLabelsBeforeAddressUpdate: true,
                    updateHouseNumberLabelsAfterAddressUpdate: true);
            });

            When<Envelope<AddressWasRejected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.Status =  MapStatus(AddressStatus.Rejected);
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    ct,
                    updateHouseNumberLabelsBeforeAddressUpdate: true,
                    updateHouseNumberLabelsAfterAddressUpdate: true);
            });

            When<Envelope<AddressWasRejectedBecauseHouseNumberWasRejected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.Status =  MapStatus(AddressStatus.Rejected);
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    ct,
                    updateHouseNumberLabelsBeforeAddressUpdate: true,
                    updateHouseNumberLabelsAfterAddressUpdate: true);
            });

            When<Envelope<AddressWasRejectedBecauseHouseNumberWasRetired>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.Status =  MapStatus(AddressStatus.Rejected);
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    ct,
                    updateHouseNumberLabelsBeforeAddressUpdate: true,
                    updateHouseNumberLabelsAfterAddressUpdate: true);
            });

            When<Envelope<AddressWasRejectedBecauseStreetNameWasRetired>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.Status =  MapStatus(AddressStatus.Rejected);
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    ct,
                    updateHouseNumberLabelsBeforeAddressUpdate: true,
                    updateHouseNumberLabelsAfterAddressUpdate: true);
            });

            When<Envelope<AddressWasCorrectedFromRejectedToProposed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.Status = MapStatus(AddressStatus.Proposed);
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    ct,
                    updateHouseNumberLabelsBeforeAddressUpdate: true,
                    updateHouseNumberLabelsAfterAddressUpdate: true);
            });

            When<Envelope<AddressWasDeregulated>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.OfficiallyAssigned = false;
                        address.Status =  MapStatus(AddressStatus.Current);
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    ct,
                    updateHouseNumberLabelsBeforeAddressUpdate: true,
                    updateHouseNumberLabelsAfterAddressUpdate: true);
            });

            When<Envelope<AddressWasRegularized>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.OfficiallyAssigned = true;
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<AddressWasRetiredV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.Status =  MapStatus(AddressStatus.Retired);
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    ct,
                    updateHouseNumberLabelsBeforeAddressUpdate: true,
                    updateHouseNumberLabelsAfterAddressUpdate: true);
            });

            When<Envelope<AddressWasRetiredBecauseHouseNumberWasRetired>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.Status =  MapStatus(AddressStatus.Retired);
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    ct,
                    updateHouseNumberLabelsBeforeAddressUpdate: true,
                    updateHouseNumberLabelsAfterAddressUpdate: true);
            });

            When<Envelope<AddressWasRetiredBecauseStreetNameWasRetired>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.Status =  MapStatus(AddressStatus.Retired);
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    ct,
                    updateHouseNumberLabelsBeforeAddressUpdate: true,
                    updateHouseNumberLabelsAfterAddressUpdate: true);
            });

            When<Envelope<AddressWasCorrectedFromRetiredToCurrent>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.Status =  MapStatus(AddressStatus.Current);
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    ct,
                    updateHouseNumberLabelsBeforeAddressUpdate: true,
                    updateHouseNumberLabelsAfterAddressUpdate: true);
            });

            When<Envelope<AddressPostalCodeWasChangedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.PostalCode = message.Message.PostalCode;
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    ct);

                foreach (var boxNumberPersistentLocalId in message.Message.BoxNumberPersistentLocalIds)
                {
                    await context.FindAndUpdateAddressDetail(
                        boxNumberPersistentLocalId,
                        address =>
                        {
                            address.PostalCode = message.Message.PostalCode;
                            UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                        },
                        ct);
                }
            });

            When<Envelope<AddressPostalCodeWasCorrectedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.PostalCode = message.Message.PostalCode;
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    ct);

                foreach (var boxNumberPersistentLocalId in message.Message.BoxNumberPersistentLocalIds)
                {
                    await context.FindAndUpdateAddressDetail(
                        boxNumberPersistentLocalId,
                        address =>
                        {
                            address.PostalCode = message.Message.PostalCode;
                            UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                        },
                        ct);
                }
            });

            When<Envelope<AddressHouseNumberWasCorrectedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.HouseNumber = message.Message.HouseNumber;
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    ct,
                    updateHouseNumberLabelsAfterAddressUpdate: true);

                foreach (var boxNumberPersistentLocalId in message.Message.BoxNumberPersistentLocalIds)
                {
                    await context.FindAndUpdateAddressDetail(
                        boxNumberPersistentLocalId,
                        address =>
                        {
                            address.HouseNumber = message.Message.HouseNumber;
                            UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                        },
                        ct,
                        updateHouseNumberLabelsAfterAddressUpdate: true);
                }
            });

            When<Envelope<AddressBoxNumberWasCorrectedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.BoxNumber = message.Message.BoxNumber;
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<AddressPositionWasChanged>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.PositionMethod = ConvertGeometryMethodToString(message.Message.GeometryMethod);
                        address.PositionSpecification = ConvertGeometrySpecificationToString(message.Message.GeometrySpecification);
                        address.SetPosition(ParsePosition(message.Message.ExtendedWkbGeometry));
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    ct,
                    updateHouseNumberLabelsBeforeAddressUpdate: true,
                    updateHouseNumberLabelsAfterAddressUpdate: true,
                    allowUpdateRemovedAddress: true);
            });

            When<Envelope<AddressPositionWasCorrectedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.PositionMethod = ConvertGeometryMethodToString(message.Message.GeometryMethod);
                        address.PositionSpecification = ConvertGeometrySpecificationToString(message.Message.GeometrySpecification);
                        address.SetPosition(ParsePosition(message.Message.ExtendedWkbGeometry));
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    ct,
                    updateHouseNumberLabelsBeforeAddressUpdate: true,
                    updateHouseNumberLabelsAfterAddressUpdate: true,
                    allowUpdateRemovedAddress: true);
            });

            When<Envelope<AddressWasRemovedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.Removed = true;
                        address.SetHouseNumberLabel(null);
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    ct,
                    updateHouseNumberLabelsBeforeAddressUpdate: true,
                    allowUpdateRemovedAddress: true);
            });

            When<Envelope<AddressWasRemovedBecauseHouseNumberWasRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    address =>
                    {
                        address.Removed = true;
                        address.SetHouseNumberLabel(null);
                        UpdateVersionTimestamp(address, message.Message.Provenance.Timestamp);
                    },
                    ct,
                    updateHouseNumberLabelsBeforeAddressUpdate: true,
                    allowUpdateRemovedAddress: true);
            });
        }

        private static void UpdateVersionTimestamp(AddressWmsItem addressWmsItem, Instant versionTimestamp)
            => addressWmsItem.VersionTimestamp = versionTimestamp;

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
    }
}
