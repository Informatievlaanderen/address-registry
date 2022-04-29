namespace AddressRegistry.Projections.Wfs.AddressWfs
{
    using System;
    using StreetName;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;
    using NodaTime;
    using StreetName.Events;

    [ConnectedProjectionName("WFS adressen")]
    [ConnectedProjectionDescription("Projectie die de adressen data voor het WFS adresregister voorziet.")]
    public class AddressWfsProjections : ConnectedProjection<WfsContext>
    {
        private static readonly string AdresStatusInGebruik = AdresStatus.InGebruik.ToString();
        private static readonly string AdresStatusGehistoreerd = AdresStatus.Gehistoreerd.ToString();
        private static readonly string AdresStatusVoorgesteld = AdresStatus.Voorgesteld.ToString();
        private static readonly string AdresStatusAfgekeurd = "Afgekeurd";

        private readonly WKBReader _wkbReader;

        public AddressWfsProjections(WKBReader wkbReader)
        {
            _wkbReader = wkbReader;

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

        public Point ParsePosition(string extendedWkbGeometry)
            => (Point) _wkbReader.Read(extendedWkbGeometry.ToByteArray());


        private static void UpdateVersionTimestamp(AddressWfsItem addressDetailItem, Instant versionTimestamp)
            => addressDetailItem.VersionTimestamp = versionTimestamp;

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
    }
}
