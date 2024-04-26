namespace AddressRegistry.Projections.Extract.AddressExtract
{
    using System;
    using System.Linq;
    using System.Text;
    using Address.Events;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Extracts;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Microsoft.Extensions.Options;
    using NetTopologySuite.IO;
    using NodaTime;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using StreetName;
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

        public AddressExtractProjectionsV2(
            IReadonlyStreamStore streamStore,
            EventDeserializer eventDeserializer,
            IOptions<ExtractConfig> extractConfig,
            Encoding encoding,
            WKBReader wkbReader)
        {
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));

            // StreetName
            When<Envelope<StreetNameNamesWereCorrected>>(async (context, message, ct) =>
            {
                foreach (var addressPersistentLocalId in message.Message.AddressPersistentLocalIds)
                {
                    var item = await context.AddressExtractV2.FindAsync(addressPersistentLocalId, cancellationToken: ct);
                    UpdateVersieIfNewer(item, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<StreetNameHomonymAdditionsWereCorrected>>(async (context, message, ct) =>
            {
                foreach (var addressPersistentLocalId in message.Message.AddressPersistentLocalIds)
                {
                    var item = await context.AddressExtractV2.FindAsync(addressPersistentLocalId, cancellationToken: ct);
                    UpdateVersieIfNewer(item, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<StreetNameHomonymAdditionsWereRemoved>>(async (context, message, ct) =>
            {
                foreach (var addressPersistentLocalId in message.Message.AddressPersistentLocalIds)
                {
                    var item = await context.AddressExtractV2.FindAsync(addressPersistentLocalId, cancellationToken: ct);
                    UpdateVersieIfNewer(item, message.Message.Provenance.Timestamp);
                }
            });

            // Address
            When<Envelope<AddressWasMigratedToStreetName>>(async (context, message, ct) =>
            {
                if (message.Message.IsRemoved)
                    return;

                var coordinate = wkbReader.Read(message.Message.ExtendedWkbGeometry.ToByteArray()).Coordinate;
                var pointShapeContent = new PointShapeContent(new Point(coordinate.X, coordinate.Y));

                var firstEventJsonData = await (await streamStore
                    .ReadStreamForwards(message.Message.AddressId.ToString("D"), StreamVersion.Start, 1, ct))
                    .Messages
                    .First()
                    .GetJsonData(ct);
                var registeredEvent = (AddressWasRegistered)eventDeserializer.DeserializeObject(firstEventJsonData, typeof(AddressWasRegistered));
                var creationTimestampAsString = registeredEvent.Provenance.Timestamp.ToBelgianDateTimeOffset().FromDateTimeOffset();

                var addressDbaseRecord = new AddressDbaseRecordV2
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
                    creatieid = { Value = creationTimestampAsString },
                    versieid = { Value = message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset().FromDateTimeOffset() },
                };

                if (!string.IsNullOrEmpty(message.Message.BoxNumber))
                {
                    addressDbaseRecord.busnr.Value = message.Message.BoxNumber;
                }

                await context.AddressExtractV2.AddAsync(new AddressExtractItemV2
                {
                    AddressPersistentLocalId = message.Message.AddressPersistentLocalId,
                    StreetNamePersistentLocalId = message.Message.StreetNamePersistentLocalId,
                    MinimumX = pointShapeContent.Shape.X,
                    MaximumX = pointShapeContent.Shape.X,
                    MinimumY = pointShapeContent.Shape.Y,
                    MaximumY = pointShapeContent.Shape.Y,
                    ShapeRecordContent = pointShapeContent.ToBytes(),
                    ShapeRecordContentLength = pointShapeContent.ContentLength.ToInt32(),
                    DbaseRecord = addressDbaseRecord.ToBytes(_encoding),
                }, cancellationToken: ct);
            });

            When<Envelope<AddressWasProposedV2>>(async (context, message, ct) =>
            {
                var addressDbaseRecord = new AddressDbaseRecordV2
                {
                    id = { Value = $"{extractConfig.Value.DataVlaanderenNamespace}/{message.Message.AddressPersistentLocalId}" },
                    adresid = { Value = message.Message.AddressPersistentLocalId },
                    huisnr = { Value = message.Message.HouseNumber },
                    postcode = { Value = message.Message.PostalCode },
                    posgeommet = { Value = Map(message.Message.GeometryMethod) },
                    posspec = { Value = Map(message.Message.GeometrySpecification) },
                    offtoegknd = { Value = true },
                    straatnmid = { Value = message.Message.StreetNamePersistentLocalId.ToString() },
                    status = { Value = Map(AddressStatus.Proposed) },
                    creatieid = { Value = message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset().FromDateTimeOffset() },
                    versieid = { Value = message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset().FromDateTimeOffset() }
                };

                if (!string.IsNullOrEmpty(message.Message.BoxNumber))
                {
                    addressDbaseRecord.busnr.Value = message.Message.BoxNumber;
                }

                var coordinate = wkbReader.Read(message.Message.ExtendedWkbGeometry.ToByteArray()).Coordinate;
                var pointShapeContent = new PointShapeContent(new Point(coordinate.X, coordinate.Y));

                await context.AddressExtractV2.AddAsync(new AddressExtractItemV2
                {
                    AddressPersistentLocalId = message.Message.AddressPersistentLocalId,
                    StreetNamePersistentLocalId = message.Message.StreetNamePersistentLocalId,
                    MinimumX = pointShapeContent.Shape.X,
                    MaximumX = pointShapeContent.Shape.X,
                    MinimumY = pointShapeContent.Shape.Y,
                    MaximumY = pointShapeContent.Shape.Y,
                    ShapeRecordContent = pointShapeContent.ToBytes(),
                    ShapeRecordContentLength = pointShapeContent.ContentLength.ToInt32(),
                    DbaseRecord = addressDbaseRecord.ToBytes(_encoding),
                }, cancellationToken: ct);
            });

            When<Envelope<AddressWasApproved>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtractV2.FindAsync(message.Message.AddressPersistentLocalId, cancellationToken: ct);
                UpdateDbaseRecordField(item, record => record.status.Value = Map(AddressStatus.Current));
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressWasCorrectedFromApprovedToProposed>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtractV2.FindAsync(message.Message.AddressPersistentLocalId, cancellationToken: ct);
                UpdateDbaseRecordField(item, record => record.status.Value = Map(AddressStatus.Proposed));
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtractV2.FindAsync(message.Message.AddressPersistentLocalId, cancellationToken: ct);
                UpdateDbaseRecordField(item, record => record.status.Value = Map(AddressStatus.Proposed));
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressWasRejected>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtractV2.FindAsync(message.Message.AddressPersistentLocalId, cancellationToken: ct);
                UpdateDbaseRecordField(item, record => record.status.Value = Map(AddressStatus.Rejected));
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressWasRejectedBecauseHouseNumberWasRetired>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtractV2.FindAsync(message.Message.AddressPersistentLocalId, cancellationToken: ct);
                UpdateDbaseRecordField(item, record => record.status.Value = Map(AddressStatus.Rejected));
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressWasRejectedBecauseHouseNumberWasRejected>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtractV2.FindAsync(message.Message.AddressPersistentLocalId, cancellationToken: ct);
                UpdateDbaseRecordField(item, record => record.status.Value = Map(AddressStatus.Rejected));
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressWasRejectedBecauseStreetNameWasRejected>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtractV2.FindAsync(message.Message.AddressPersistentLocalId, cancellationToken: ct);
                UpdateDbaseRecordField(item, record => record.status.Value = Map(AddressStatus.Rejected));
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressWasRetiredBecauseStreetNameWasRejected>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtractV2.FindAsync(message.Message.AddressPersistentLocalId, cancellationToken: ct);
                UpdateDbaseRecordField(item, record => record.status.Value = Map(AddressStatus.Retired));
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressWasRejectedBecauseStreetNameWasRetired>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtractV2.FindAsync(message.Message.AddressPersistentLocalId, cancellationToken: ct);
                UpdateDbaseRecordField(item, record => record.status.Value = Map(AddressStatus.Rejected));
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressWasDeregulated>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtractV2.FindAsync(message.Message.AddressPersistentLocalId, cancellationToken: ct);
                UpdateDbaseRecordField(item, record =>
                {
                    record.offtoegknd.Value = false;
                    record.status.Value = Map(AddressStatus.Current);
                });
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressWasRegularized>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtractV2.FindAsync(message.Message.AddressPersistentLocalId, cancellationToken: ct);
                UpdateDbaseRecordField(item, record => record.offtoegknd.Value = true);
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressWasRetiredV2>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtractV2.FindAsync(message.Message.AddressPersistentLocalId, cancellationToken: ct);
                UpdateDbaseRecordField(item, record => record.status.Value = Map(AddressStatus.Retired));
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressWasRetiredBecauseHouseNumberWasRetired>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtractV2.FindAsync(message.Message.AddressPersistentLocalId, cancellationToken: ct);
                UpdateDbaseRecordField(item, record => record.status.Value = Map(AddressStatus.Retired));
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressWasRetiredBecauseStreetNameWasRetired>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtractV2.FindAsync(message.Message.AddressPersistentLocalId, cancellationToken: ct);
                UpdateDbaseRecordField(item, record => record.status.Value = Map(AddressStatus.Retired));
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressWasCorrectedFromRetiredToCurrent>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtractV2.FindAsync(message.Message.AddressPersistentLocalId, cancellationToken: ct);
                UpdateDbaseRecordField(item, record => record.status.Value = Map(AddressStatus.Current));
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressPostalCodeWasChangedV2>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtractV2.FindAsync(message.Message.AddressPersistentLocalId, cancellationToken: ct);
                UpdateDbaseRecordField(item, record => record.postcode.Value = message.Message.PostalCode );
                UpdateVersie(item, message.Message.Provenance.Timestamp);

                foreach (var boxNumberPersistentLocalId in message.Message.BoxNumberPersistentLocalIds)
                {
                    var boxNumberItem = await context.AddressExtractV2.FindAsync(boxNumberPersistentLocalId, cancellationToken: ct);
                    UpdateDbaseRecordField(boxNumberItem, record => record.postcode.Value = message.Message.PostalCode );
                    UpdateVersie(boxNumberItem, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<AddressPostalCodeWasCorrectedV2>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtractV2.FindAsync(message.Message.AddressPersistentLocalId, cancellationToken: ct);
                UpdateDbaseRecordField(item, record => record.postcode.Value = message.Message.PostalCode );
                UpdateVersie(item, message.Message.Provenance.Timestamp);

                foreach (var boxNumberPersistentLocalId in message.Message.BoxNumberPersistentLocalIds)
                {
                    var boxNumberItem = await context.AddressExtractV2.FindAsync(boxNumberPersistentLocalId, cancellationToken: ct);
                    UpdateDbaseRecordField(boxNumberItem, record => record.postcode.Value = message.Message.PostalCode );
                    UpdateVersie(boxNumberItem, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<AddressHouseNumberWasCorrectedV2>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtractV2.FindAsync(message.Message.AddressPersistentLocalId, cancellationToken: ct);
                UpdateDbaseRecordField(item, record => record.huisnr.Value = message.Message.HouseNumber );
                UpdateVersie(item, message.Message.Provenance.Timestamp);

                foreach (var boxNumberPersistentLocalId in message.Message.BoxNumberPersistentLocalIds)
                {
                    var boxNumberItem = await context.AddressExtractV2.FindAsync(boxNumberPersistentLocalId, cancellationToken: ct);
                    UpdateDbaseRecordField(boxNumberItem, record => record.huisnr.Value = message.Message.HouseNumber );
                    UpdateVersie(boxNumberItem, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<AddressBoxNumberWasCorrectedV2>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtractV2.FindAsync(message.Message.AddressPersistentLocalId, cancellationToken: ct);
                UpdateDbaseRecordField(item, record => record.busnr.Value = message.Message.BoxNumber);
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressPositionWasChanged>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtractV2.FindAsync(message.Message.AddressPersistentLocalId, cancellationToken: ct);
                UpdateDbaseRecordField(item, record =>
                {
                    record.posgeommet.Value = Map(message.Message.GeometryMethod);
                    record.posspec.Value = Map(message.Message.GeometrySpecification);

                    UpdateShape(item, wkbReader, message.Message.ExtendedWkbGeometry);
                });
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressPositionWasCorrectedV2>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtractV2.FindAsync(message.Message.AddressPersistentLocalId, cancellationToken: ct);
                UpdateDbaseRecordField(item, record =>
                {
                    record.posgeommet.Value = Map(message.Message.GeometryMethod);
                    record.posspec.Value = Map(message.Message.GeometrySpecification);

                    UpdateShape(item, wkbReader, message.Message.ExtendedWkbGeometry);
                });
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressHouseNumberWasReaddressed>>(async (context, message, ct) =>
            {
                var houseNumberItem = await context.AddressExtractV2.FindAsync(message.Message.AddressPersistentLocalId, cancellationToken: ct);
                UpdateDbaseRecordField(houseNumberItem, record =>
                {
                    record.status.Value = Map(message.Message.ReaddressedHouseNumber.SourceStatus);
                    record.huisnr.Value = message.Message.ReaddressedHouseNumber.DestinationHouseNumber;
                    record.postcode.Value = message.Message.ReaddressedHouseNumber.SourcePostalCode;
                    record.offtoegknd.Value = message.Message.ReaddressedHouseNumber.SourceIsOfficiallyAssigned;
                    record.posgeommet.Value = Map(message.Message.ReaddressedHouseNumber.SourceGeometryMethod);
                    record.posspec.Value = Map(message.Message.ReaddressedHouseNumber.SourceGeometrySpecification);

                    UpdateShape(houseNumberItem, wkbReader, message.Message.ReaddressedHouseNumber.SourceExtendedWkbGeometry);
                });
                UpdateVersie(houseNumberItem, message.Message.Provenance.Timestamp);

                foreach (var readdressedBoxNumber in message.Message.ReaddressedBoxNumbers)
                {
                    var boxNumberItem = await context.AddressExtractV2.FindAsync(readdressedBoxNumber.DestinationAddressPersistentLocalId, cancellationToken: ct);
                    UpdateDbaseRecordField(boxNumberItem, record =>
                    {
                        record.status.Value = Map(readdressedBoxNumber.SourceStatus);
                        record.huisnr.Value = readdressedBoxNumber.DestinationHouseNumber;
                        record.busnr.Value = readdressedBoxNumber.SourceBoxNumber;
                        record.postcode.Value = readdressedBoxNumber.SourcePostalCode;
                        record.offtoegknd.Value = readdressedBoxNumber.SourceIsOfficiallyAssigned;
                        record.posgeommet.Value = Map(readdressedBoxNumber.SourceGeometryMethod);
                        record.posspec.Value = Map(readdressedBoxNumber.SourceGeometrySpecification);

                        UpdateShape(boxNumberItem, wkbReader, readdressedBoxNumber.SourceExtendedWkbGeometry);
                    });
                    UpdateVersie(boxNumberItem, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<AddressWasProposedBecauseOfReaddress>>(async (context, message, ct) =>
            {
                var addressDbaseRecord = new AddressDbaseRecordV2
                {
                    id = { Value = $"{extractConfig.Value.DataVlaanderenNamespace}/{message.Message.AddressPersistentLocalId}" },
                    adresid = { Value = message.Message.AddressPersistentLocalId },
                    huisnr = { Value = message.Message.HouseNumber },
                    postcode = { Value = message.Message.PostalCode },
                    posgeommet = { Value = Map(message.Message.GeometryMethod) },
                    posspec = { Value = Map(message.Message.GeometrySpecification) },
                    offtoegknd = { Value = true },
                    straatnmid = { Value = message.Message.StreetNamePersistentLocalId.ToString() },
                    status = { Value = Map(AddressStatus.Proposed) },
                    creatieid = { Value = message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset().FromDateTimeOffset() },
                    versieid = { Value = message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset().FromDateTimeOffset() }
                };

                if (!string.IsNullOrEmpty(message.Message.BoxNumber))
                {
                    addressDbaseRecord.busnr.Value = message.Message.BoxNumber;
                }

                var coordinate = wkbReader.Read(message.Message.ExtendedWkbGeometry.ToByteArray()).Coordinate;
                var pointShapeContent = new PointShapeContent(new Point(coordinate.X, coordinate.Y));

                await context.AddressExtractV2.AddAsync(new AddressExtractItemV2
                {
                    AddressPersistentLocalId = message.Message.AddressPersistentLocalId,
                    StreetNamePersistentLocalId = message.Message.StreetNamePersistentLocalId,
                    MinimumX = pointShapeContent.Shape.X,
                    MaximumX = pointShapeContent.Shape.X,
                    MinimumY = pointShapeContent.Shape.Y,
                    MaximumY = pointShapeContent.Shape.Y,
                    ShapeRecordContent = pointShapeContent.ToBytes(),
                    ShapeRecordContentLength = pointShapeContent.ContentLength.ToInt32(),
                    DbaseRecord = addressDbaseRecord.ToBytes(_encoding),
                }, cancellationToken: ct);
            });

            When<Envelope<AddressWasRejectedBecauseOfReaddress>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtractV2.FindAsync(message.Message.AddressPersistentLocalId, cancellationToken: ct);
                UpdateDbaseRecordField(item, record => record.status.Value = Map(AddressStatus.Rejected));
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressWasRetiredBecauseOfReaddress>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtractV2.FindAsync(message.Message.AddressPersistentLocalId, cancellationToken: ct);
                UpdateDbaseRecordField(item, record => record.status.Value = Map(AddressStatus.Retired));
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressWasRemovedV2>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtractV2.FindAsync(message.Message.AddressPersistentLocalId, cancellationToken: ct);
                context.AddressExtractV2.Remove(item);
            });

            When<Envelope<AddressWasRemovedBecauseStreetNameWasRemoved>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtractV2.FindAsync(message.Message.AddressPersistentLocalId, cancellationToken: ct);
                context.AddressExtractV2.Remove(item);
            });

            When<Envelope<AddressWasRemovedBecauseHouseNumberWasRemoved>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtractV2.FindAsync(message.Message.AddressPersistentLocalId, cancellationToken: ct);
                context.AddressExtractV2.Remove(item);
            });

            When<Envelope<AddressWasCorrectedFromRejectedToProposed>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtractV2.FindAsync(message.Message.AddressPersistentLocalId, cancellationToken: ct);
                UpdateDbaseRecordField(item, record => record.status.Value = Map(AddressStatus.Proposed));
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressRegularizationWasCorrected>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtractV2.FindAsync(message.Message.AddressPersistentLocalId, cancellationToken: ct);
                UpdateDbaseRecordField(item, record =>
                {
                    record.status.Value = Map(AddressStatus.Current);
                    record.offtoegknd.Value = false;
                });
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressDeregulationWasCorrected>>(async (context, message, ct) =>
            {
                var item = await context.AddressExtractV2.FindAsync(message.Message.AddressPersistentLocalId, cancellationToken: ct);
                UpdateDbaseRecordField(item, record =>
                {
                    record.offtoegknd.Value = true;
                });
                UpdateVersie(item, message.Message.Provenance.Timestamp);
            });

            When<Envelope<AddressRemovalWasCorrected>>(async (context, message, ct) =>
            {
                var addressDbaseRecord = new AddressDbaseRecordV2
                {
                    id = { Value = $"{extractConfig.Value.DataVlaanderenNamespace}/{message.Message.AddressPersistentLocalId}" },
                    straatnmid = { Value = message.Message.StreetNamePersistentLocalId.ToString() },
                    adresid = { Value = message.Message.AddressPersistentLocalId },
                    status = { Value = Map(message.Message.Status) },
                    huisnr = { Value = message.Message.HouseNumber },
                    postcode = { Value = message.Message.PostalCode },
                    posgeommet = { Value = Map(message.Message.GeometryMethod) },
                    posspec = { Value = Map(message.Message.GeometrySpecification) },
                    offtoegknd = { Value = true },
                    creatieid = { Value = message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset().FromDateTimeOffset() },
                    versieid = { Value = message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset().FromDateTimeOffset() }
                };

                if (!string.IsNullOrEmpty(message.Message.BoxNumber))
                {
                    addressDbaseRecord.busnr.Value = message.Message.BoxNumber;
                }

                var coordinate = wkbReader.Read(message.Message.ExtendedWkbGeometry.ToByteArray()).Coordinate;
                var pointShapeContent = new PointShapeContent(new Point(coordinate.X, coordinate.Y));

                await context.AddressExtractV2.AddAsync(new AddressExtractItemV2
                {
                    AddressPersistentLocalId = message.Message.AddressPersistentLocalId,
                    StreetNamePersistentLocalId = message.Message.StreetNamePersistentLocalId,
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

        private static void UpdateShape(AddressExtractItemV2 item, WKBReader wkbReader, string extendedWkbGeometry)
        {
            var coordinate = wkbReader.Read(extendedWkbGeometry.ToByteArray()).Coordinate;
            var pointShapeContent = new PointShapeContent(new Point(coordinate.X, coordinate.Y));

            item.MinimumX = pointShapeContent.Shape.X;
            item.MaximumX = pointShapeContent.Shape.X;
            item.MinimumY = pointShapeContent.Shape.Y;
            item.MaximumY = pointShapeContent.Shape.Y;
            item.ShapeRecordContent = pointShapeContent.ToBytes();
            item.ShapeRecordContentLength = pointShapeContent.ContentLength.ToInt32();
        }

        private void UpdateDbaseRecordField(AddressExtractItemV2 item, Action<AddressDbaseRecordV2> update)
        {
            var record = new AddressDbaseRecordV2();
            record.FromBytes(item.DbaseRecord, _encoding);
            update(record);
            item.DbaseRecord = record.ToBytes(_encoding);
        }

        private void UpdateVersieIfNewer(AddressExtractItemV2 address, Instant timestamp)
            => UpdateDbaseRecordField(address, record =>
            {
                if (Instant.FromDateTimeOffset(record.versieid.ValueAsDateTimeOffset) < timestamp)
                {
                    record.versieid.SetValue(timestamp.ToBelgianDateTimeOffset());
                }
            });

        private void UpdateVersie(AddressExtractItemV2 address, Instant timestamp)
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
