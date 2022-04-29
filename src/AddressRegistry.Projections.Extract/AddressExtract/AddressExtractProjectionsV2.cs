namespace AddressRegistry.Projections.Extract.AddressExtract
{
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Extracts;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using NetTopologySuite.IO;
    using NodaTime;
    using System;
    using System.Text;
    using StreetName;
    using Microsoft.Extensions.Options;
    using StreetName.Events;

    [ConnectedProjectionName("Extract adressen")]
    [ConnectedProjectionDescription("Projectie die de adressen data voor het adressen extract voorziet.")]
    public class AddressExtractProjectionsV2 : ConnectedProjection<ExtractContext>
    {
        private const string StatusCurrent = "InGebruik";
        private const string StatusProposed = "Voorgesteld";
        private const string StatusRetired = "Gehistoreerd";
        private const string StatusRejected = "Afgekeurd";

        private const string GeomMethAppointedByAdministrator = "AangeduidDoorBeheerder";
        private const string GeomMethDerivedFromObject = "AfgeleidVanObject";
        private const string GeomMethInterpolated = "Geinterpoleerd";

        private readonly string GeomSpecMunicipality = "Gemeente";
        private readonly string GeomSpecBerth = "Ligplaats";
        private readonly string GeomSpecBuilding = "Gebouw";
        private readonly string GeomSpecStreet = "Straat";
        private readonly string GeomSpecStand = "Standplaats";
        private readonly string GeomSpecRoadSegment = "Wegsegment";
        private readonly string GeomSpecParcel = "Perceel";
        private readonly string GeomSpecLot = "Lot";
        private readonly string GeomSpecEntry = "Ingang";
        private readonly string GeomSpecBuildingUnit = "Gebouweenheid";
        private readonly Encoding _encoding;

        public AddressExtractProjectionsV2(IOptions<ExtractConfig> extractConfig, Encoding encoding, WKBReader wkbReader)
        {
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));

            When<Envelope<AddressWasMigratedToStreetName>>(async (context, message, ct) =>
            {
                var coordinate = wkbReader.Read(message.Message.ExtendedWkbGeometry.ToByteArray()).Coordinate;
                var pointShapeContent = new PointShapeContent(new Point(coordinate.X, coordinate.Y));

                var addressDbaseRecord = new AddressDbaseRecord
                {
                    id = { Value = $"{extractConfig.Value.DataVlaanderenNamespace}/{message.Message.AddressPersistentLocalId}" },
                    adresid = { Value = message.Message.AddressPersistentLocalId },
                    huisnr = { Value = message.Message.HouseNumber },
                    postcode = { Value = message.Message.PostalCode },
                    offtoegknd = { Value = message.Message.OfficiallyAssigned },
                    posgeommet = { Value = Map(message.Message.GeometryMethod) },
                    posspec = { Value = Map(message.Message.GeometrySpecification) },
                    straatnmid = { Value = message.Message.StreetNamePersistentLocalId.ToString() },
                    status = { Value = Map(message.Message.Status)},
                    versieid = { Value = message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset().FromDateTimeOffset() }
                };

                if (!string.IsNullOrEmpty(message.Message.BoxNumber))
                {
                    addressDbaseRecord.busnr.Value = message.Message.BoxNumber;
                }

                await context.AddressExtractV2.AddAsync(new AddressExtractItemV2
                {
                    AddressPersistentLocalId = message.Message.AddressPersistentLocalId,
                    StreetNamePersistentLocalId = message.Message.StreetNamePersistentLocalId,
                    Complete = false,
                    MinimumX = pointShapeContent.Shape.X,
                    MaximumX = pointShapeContent.Shape.X,
                    MinimumY = pointShapeContent.Shape.Y,
                    MaximumY = pointShapeContent.Shape.Y,
                    ShapeRecordContent = pointShapeContent.ToBytes(),
                    ShapeRecordContentLength = pointShapeContent.ContentLength.ToInt32(),
                    DbaseRecord = addressDbaseRecord.ToBytes(_encoding),
                }, cancellationToken: ct);
            });
        }

        private void UpdateDbaseRecordField(AddressExtractItem item, Action<AddressDbaseRecord> update)
        {
            var record = new AddressDbaseRecord();
            record.FromBytes(item.DbaseRecord, _encoding);
            update(record);
            item.DbaseRecord = record.ToBytes(_encoding);
        }

        private void UpdateVersie(AddressExtractItem address, Instant timestamp)
            => UpdateDbaseRecordField(address, record => record.versieid.SetValue(timestamp.ToBelgianDateTimeOffset()));

        private static string Map(AddressStatus addressStatus)
        {
            switch (addressStatus)
            {
                case AddressStatus.Current:
                    return StatusCurrent;

                case AddressStatus.Proposed:
                    return StatusProposed;

                case AddressStatus.Retired:
                    return StatusRetired;

                case AddressStatus.Rejected:
                    return StatusRejected;

                default:
                    throw new ArgumentOutOfRangeException(nameof(addressStatus), addressStatus, null);
            }
        }

        private static string Map(GeometryMethod geometryMethod)
        {
            switch (geometryMethod)
            {
                case GeometryMethod.AppointedByAdministrator:
                    return GeomMethAppointedByAdministrator;

                case GeometryMethod.DerivedFromObject:
                    return GeomMethDerivedFromObject;

                case GeometryMethod.Interpolated:
                    return GeomMethInterpolated;

                default:
                    throw new ArgumentOutOfRangeException(nameof(geometryMethod), geometryMethod, null);
            }
        }

        private string Map(GeometrySpecification geometrySpecification)
        {
            switch (geometrySpecification)
            {
                case GeometrySpecification.Municipality:
                    return GeomSpecMunicipality;

                case GeometrySpecification.Berth:
                    return GeomSpecBerth;

                case GeometrySpecification.Building:
                    return GeomSpecBuilding;

                case GeometrySpecification.BuildingUnit:
                    return GeomSpecBuildingUnit;

                case GeometrySpecification.Entry:
                    return GeomSpecEntry;

                case GeometrySpecification.Lot:
                    return GeomSpecLot;

                case GeometrySpecification.Parcel:
                    return GeomSpecParcel;

                case GeometrySpecification.RoadSegment:
                    return GeomSpecRoadSegment;

                case GeometrySpecification.Stand:
                    return GeomSpecStand;

                case GeometrySpecification.Street:
                    return GeomSpecStreet;

                default:
                    throw new ArgumentOutOfRangeException(nameof(geometrySpecification), geometrySpecification, null);
            }
        }
    }
}