namespace AddressRegistry.Projections.Extract.AddressExtract
{
    using Address.Events;
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
    using Address.Events.Crab;
    using Address.ValueObjects;
    using Microsoft.Extensions.Options;

    [ConnectedProjectionName("Extract adressen")]
    [ConnectedProjectionDescription("Projectie die de adressen data voor het adressen extract voorziet.")]
    public class AddressExtractProjection : ConnectedProjection<ExtractContext>
    {
        //TODO: should these translations also be used in other places?
        private const string StatusCurrent = "InGebruik";
        private const string StatusProposed = "Voorgesteld";
        private const string StatusRetired = "Gehistoreerd";

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

        public AddressExtractProjection(IOptions<ExtractConfig> extractConfig, Encoding encoding, WKBReader wkbReader)
        {
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));

            When<Envelope<AddressWasRegistered>>(async (context, message, ct) =>
            {
                await context.AddressExtract.AddAsync(new AddressExtractItem
                {
                    AddressId = message.Message.AddressId,
                    StreetNameId = message.Message.StreetNameId,
                    Complete = false,
                    DbaseRecord = new AddressDbaseRecord
                    {
                        huisnr = { Value = message.Message.HouseNumber },
                        versieid = { Value = message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset().FromDateTimeOffset() }
                    }.ToBytes(_encoding),
                }, cancellationToken: ct);
            });

            When<Envelope<AddressBecameComplete>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                item.Complete = true;
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressStreetNameWasChanged>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                item.StreetNameId = message.Message.StreetNameId;
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressStreetNameWasCorrected>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                item.StreetNameId = message.Message.StreetNameId;
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressBecameCurrent>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                UpdateDbaseRecordField(item, record => record.status.Value = StatusCurrent);
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressBecameIncomplete>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                if (item != null) // in rare cases were we might get this event after an AddressWasRemoved event, we can just ignore it
                {
                    item.Complete = false;
                    UpdateVersie(item, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<AddressBecameNotOfficiallyAssigned>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                UpdateDbaseRecordField(item, record => record.offtoegknd.Value = false);
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressHouseNumberWasChanged>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                UpdateDbaseRecordField(item, record => record.huisnr.Value = message.Message.HouseNumber);
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressHouseNumberWasCorrected>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                UpdateDbaseRecordField(item, record => record.huisnr.Value = message.Message.HouseNumber);
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressOfficialAssignmentWasRemoved>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                if (item != null) // in rare cases were we might get this event after an AddressWasRemoved event, we can just ignore it
                {
                    UpdateDbaseRecordField(item, record => record.offtoegknd.Value = null);
                    UpdateVersie(item, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<AddressPersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                if (item != null) // in rare cases were we might get this event after an AddressWasRemoved event, we can just ignore it
                    UpdateDbaseRecordField(item, record =>
                    {
                        record.id.Value = $"{extractConfig.Value.DataVlaanderenNamespace}/{message.Message.PersistentLocalId}";
                        record.adresid.Value = message.Message.PersistentLocalId;
                    });
            });

            When<Envelope<AddressPositionWasCorrected>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                if (item != null) // in rare cases were we might get this event after an AddressWasRemoved event, we can just ignore it
                {
                    UpdateDbaseRecordField(item, record =>
                    {
                        record.posgeommet.Value = Map(message.Message.GeometryMethod);
                        record.posspec.Value = Map(message.Message.GeometrySpecification);
                    });

                    var coordinate = wkbReader.Read(message.Message.ExtendedWkbGeometry.ToByteArray()).Coordinate;
                    var pointShapeContent = new PointShapeContent(new Point(coordinate.X, coordinate.Y));
                    item.ShapeRecordContent = pointShapeContent.ToBytes();
                    item.ShapeRecordContentLength = pointShapeContent.ContentLength.ToInt32();
                    item.MinimumX = pointShapeContent.Shape.X;
                    item.MaximumX = pointShapeContent.Shape.X;
                    item.MinimumY = pointShapeContent.Shape.Y;
                    item.MaximumY = pointShapeContent.Shape.Y;

                    UpdateVersie(item, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<AddressPositionWasRemoved>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                if (item != null) // in rare cases were we might get this event after an AddressWasRemoved event, we can just ignore it
                {
                    UpdateDbaseRecordField(item, record =>
                    {
                        record.posgeommet.Value = null;
                        record.posspec.Value = null;
                    });

                    item.ShapeRecordContent = null;
                    item.ShapeRecordContentLength = 0;
                    item.MaximumX = 0;
                    item.MinimumX = 0;
                    item.MaximumY = 0;
                    item.MinimumY = 0;

                    UpdateVersie(item, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<AddressPostalCodeWasChanged>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                UpdateDbaseRecordField(item, record => record.postcode.Value = message.Message.PostalCode);
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressPostalCodeWasCorrected>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                if (item != null)
                {
                    UpdateDbaseRecordField(item, record => record.postcode.Value = message.Message.PostalCode);
                    UpdateVersie(item, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<AddressPostalCodeWasRemoved>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                if (item != null)
                {
                    UpdateDbaseRecordField(item, record => record.postcode.Value = null);
                    UpdateVersie(item, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<AddressStatusWasCorrectedToRemoved>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                UpdateDbaseRecordField(item, record => record.status.Value = null);
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressStatusWasRemoved>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                if (item != null) // in rare cases were we might get this event after an AddressWasRemoved event, we can just ignore it
                {
                    UpdateDbaseRecordField(item, record => record.status.Value = null);
                    UpdateVersie(item, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<AddressWasCorrectedToCurrent>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                UpdateDbaseRecordField(item, record => record.status.Value = StatusCurrent);
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressWasCorrectedToNotOfficiallyAssigned>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                UpdateDbaseRecordField(item, record => record.offtoegknd.Value = false);
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressWasCorrectedToOfficiallyAssigned>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                UpdateDbaseRecordField(item, record => record.offtoegknd.Value = true);
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressWasCorrectedToProposed>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                UpdateDbaseRecordField(item, record => record.status.Value = StatusProposed);
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressWasCorrectedToRetired>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                UpdateDbaseRecordField(item, record => record.status.Value = StatusRetired);
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressWasOfficiallyAssigned>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                UpdateDbaseRecordField(item, record => record.offtoegknd.Value = true);
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressWasPositioned>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                if (item != null) // in rare cases were we might get this event after an AddressWasRemoved event, we can just ignore it
                {
                    UpdateDbaseRecordField(item, record =>
                    {
                        record.posgeommet.Value = Map(message.Message.GeometryMethod);
                        record.posspec.Value = Map(message.Message.GeometrySpecification);
                    });

                    var coordinate = wkbReader.Read(message.Message.ExtendedWkbGeometry.ToByteArray()).Coordinate;
                    var pointShapeContent = new PointShapeContent(new Point(coordinate.X, coordinate.Y));
                    item.ShapeRecordContent = pointShapeContent.ToBytes();
                    item.ShapeRecordContentLength = pointShapeContent.ContentLength.ToInt32();
                    item.MinimumX = pointShapeContent.Shape.X;
                    item.MaximumX = pointShapeContent.Shape.X;
                    item.MinimumY = pointShapeContent.Shape.Y;
                    item.MaximumY = pointShapeContent.Shape.Y;

                    UpdateVersie(item, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<AddressWasProposed>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                UpdateDbaseRecordField(item, record => record.status.Value = StatusProposed);
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressWasRemoved>>(async (context, message, ct) =>
            {
                var address = await context.AddressExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                context.AddressExtract.Remove(address);
            });

            When<Envelope<AddressWasRetired>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                UpdateDbaseRecordField(item, record => record.status.Value = StatusRetired);
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressBoxNumberWasChanged>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                UpdateDbaseRecordField(item, record => record.busnr.Value = message.Message.BoxNumber);
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressBoxNumberWasCorrected>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                UpdateDbaseRecordField(item, record => record.busnr.Value = message.Message.BoxNumber);
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressBoxNumberWasRemoved>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                UpdateDbaseRecordField(item, record => record.busnr.Value = null);
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressHouseNumberWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressHouseNumberStatusWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressHouseNumberPositionWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressHouseNumberMailCantonWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressSubaddressWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressSubaddressPositionWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressSubaddressStatusWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
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
                    throw new NotImplementedException();
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
                    throw new NotImplementedException();
            }
        }

        private static void DoNothing() { }
    }
}
