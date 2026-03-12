namespace AddressRegistry.Projections.Feed.AddressFeed
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.ChangeFeed;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.GrAr.CrsTransform;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Consumer.Read.StreetName;
    using Contract;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using NetTopologySuite.Geometries;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using StreetName;
    using StreetName.DataStructures;
    using StreetName.Events;

    [ConnectedProjectionName("Feed endpoint adres")]
    [ConnectedProjectionDescription("Projectie die de adres data voor de adres cloudevent feed voorziet.")]
    public class AddressFeedProjections : ConnectedProjection<FeedContext>
    {
        private readonly IChangeFeedService _changeFeedService;
        private readonly IDbContextFactory<StreetNameConsumerContext> _streetNameConsumerContextFactory;

        public AddressFeedProjections(
            IChangeFeedService changeFeedService,
            IDbContextFactory<StreetNameConsumerContext> streetNameConsumerContextFactory,
            IReadonlyStreamStore streamStore)
        {
            _changeFeedService = changeFeedService;
            _streetNameConsumerContextFactory = streetNameConsumerContextFactory;

            When<Envelope<AddressWasMigratedToStreetName>>(async (context, message, ct) =>
            {
                var document = new AddressDocument(
                    message.Message.AddressPersistentLocalId,
                    message.Message.StreetNamePersistentLocalId,
                    message.Message.HouseNumber,
                    message.Message.BoxNumber,
                    message.Message.PostalCode ?? string.Empty,
                    message.Message.Provenance.Timestamp);

                document.Document.Status = MapStatus(message.Message.Status);
                document.Document.OfficiallyAssigned = message.Message.OfficiallyAssigned;
                document.IsRemoved = message.Message.IsRemoved;

                var geometry = GmlHelpers.ParseGeometry(message.Message.ExtendedWkbGeometry);
                document.Document.ExtendedWkbGeometry = message.Message.ExtendedWkbGeometry;
                document.Document.PositionAsGml = geometry.ConvertToGml();
                document.Document.PositionGeometryMethod = MapGeometryMethod(message.Message.GeometryMethod);
                document.Document.PositionSpecification = MapGeometrySpecification(message.Message.GeometrySpecification);

                await context.AddressDocuments.AddAsync(document, ct);

                List<BaseRegistriesCloudEventAttribute> attributes = [
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.StreetNameId, null, message.Message.StreetNamePersistentLocalId),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.StatusName, null, document.Document.Status),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.HouseNumber, null, document.Document.HouseNumber),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.PostalCode, null, document.Document.PostalCode),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.OfficiallyAssigned, null, document.Document.OfficiallyAssigned),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.PositionGeometryMethod, null, document.Document.PositionGeometryMethod),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.PositionSpecification, null, document.Document.PositionSpecification),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.Position, null, CreatePositionValues(geometry))
                ];

                if (!string.IsNullOrEmpty(document.Document.BoxNumber))
                    attributes.Add(new BaseRegistriesCloudEventAttribute(AddressAttributeNames.BoxNumber, null, document.Document.BoxNumber));

                await AddCloudEvent(message, document, context, attributes, AddressEventTypes.CreateV1);
            });

            When<Envelope<AddressWasProposedV2>>(async (context, message, ct) =>
            {
                var document = new AddressDocument(
                    message.Message.AddressPersistentLocalId,
                    message.Message.StreetNamePersistentLocalId,
                    message.Message.HouseNumber,
                    message.Message.BoxNumber,
                    message.Message.PostalCode,
                    message.Message.Provenance.Timestamp);

                var geometry = GmlHelpers.ParseGeometry(message.Message.ExtendedWkbGeometry);
                document.Document.ExtendedWkbGeometry = message.Message.ExtendedWkbGeometry;
                document.Document.PositionAsGml = geometry.ConvertToGml();
                document.Document.PositionGeometryMethod = MapGeometryMethod(message.Message.GeometryMethod);
                document.Document.PositionSpecification = MapGeometrySpecification(message.Message.GeometrySpecification);

                await context.AddressDocuments.AddAsync(document, ct);

                List<BaseRegistriesCloudEventAttribute> attributes = [
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.StreetNameId, null, message.Message.StreetNamePersistentLocalId),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.StatusName, null, AdresStatus.Voorgesteld),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.HouseNumber, null, document.Document.HouseNumber),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.PostalCode, null, document.Document.PostalCode),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.OfficiallyAssigned, null, document.Document.OfficiallyAssigned),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.PositionGeometryMethod, null, document.Document.PositionGeometryMethod),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.PositionSpecification, null, document.Document.PositionSpecification),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.Position, null, CreatePositionValues(geometry))
                ];

                if (!string.IsNullOrEmpty(document.Document.BoxNumber))
                    attributes.Add(new BaseRegistriesCloudEventAttribute(AddressAttributeNames.BoxNumber, null, document.Document.BoxNumber));

                await AddCloudEvent(message, document, context, attributes, AddressEventTypes.CreateV1);
            });

            When<Envelope<AddressWasProposedForMunicipalityMerger>>(async (context, message, ct) =>
            {
                var document = new AddressDocument(
                    message.Message.AddressPersistentLocalId,
                    message.Message.StreetNamePersistentLocalId,
                    message.Message.HouseNumber,
                    message.Message.BoxNumber,
                    message.Message.PostalCode,
                    message.Message.Provenance.Timestamp);

                document.Document.Status = MapStatus(message.Message.DesiredStatus);
                document.Document.OfficiallyAssigned = message.Message.OfficiallyAssigned;

                var geometry = GmlHelpers.ParseGeometry(message.Message.ExtendedWkbGeometry);
                document.Document.ExtendedWkbGeometry = message.Message.ExtendedWkbGeometry;
                document.Document.PositionAsGml = geometry.ConvertToGml();
                document.Document.PositionGeometryMethod = MapGeometryMethod(message.Message.GeometryMethod);
                document.Document.PositionSpecification = MapGeometrySpecification(message.Message.GeometrySpecification);

                await context.AddressDocuments.AddAsync(document, ct);

                List<BaseRegistriesCloudEventAttribute> attributes = [
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.StreetNameId, null, message.Message.StreetNamePersistentLocalId),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.StatusName, null, document.Document.Status),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.HouseNumber, null, document.Document.HouseNumber),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.PostalCode, null, document.Document.PostalCode),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.OfficiallyAssigned, null, document.Document.OfficiallyAssigned),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.PositionGeometryMethod, null, document.Document.PositionGeometryMethod),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.PositionSpecification, null, document.Document.PositionSpecification),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.Position, null, CreatePositionValues(geometry))
                ];

                if (!string.IsNullOrEmpty(document.Document.BoxNumber))
                    attributes.Add(new BaseRegistriesCloudEventAttribute(AddressAttributeNames.BoxNumber, null, document.Document.BoxNumber));

                await AddCloudEvent(message, document, context, attributes, AddressEventTypes.CreateV1);
            });

            When<Envelope<AddressWasProposedBecauseOfReaddress>>(async (context, message, ct) =>
            {
                var document = new AddressDocument(
                    message.Message.AddressPersistentLocalId,
                    message.Message.StreetNamePersistentLocalId,
                    message.Message.HouseNumber,
                    message.Message.BoxNumber,
                    message.Message.PostalCode,
                    message.Message.Provenance.Timestamp);

                var geometry = GmlHelpers.ParseGeometry(message.Message.ExtendedWkbGeometry);
                document.Document.ExtendedWkbGeometry = message.Message.ExtendedWkbGeometry;
                document.Document.PositionAsGml = geometry.ConvertToGml();
                document.Document.PositionGeometryMethod = MapGeometryMethod(message.Message.GeometryMethod);
                document.Document.PositionSpecification = MapGeometrySpecification(message.Message.GeometrySpecification);

                await context.AddressDocuments.AddAsync(document, ct);

                List<BaseRegistriesCloudEventAttribute> attributes = [
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.StreetNameId, null, message.Message.StreetNamePersistentLocalId),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.StatusName, null, AdresStatus.Voorgesteld),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.HouseNumber, null, document.Document.HouseNumber),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.PostalCode, null, document.Document.PostalCode),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.OfficiallyAssigned, null, document.Document.OfficiallyAssigned),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.PositionGeometryMethod, null, document.Document.PositionGeometryMethod),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.PositionSpecification, null, document.Document.PositionSpecification),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.Position, null, CreatePositionValues(geometry))
                ];

                if (!string.IsNullOrEmpty(document.Document.BoxNumber))
                    attributes.Add(new BaseRegistriesCloudEventAttribute(AddressAttributeNames.BoxNumber, null, document.Document.BoxNumber));

                await AddCloudEvent(message, document, context, attributes, AddressEventTypes.CreateV1);
            });

            When<Envelope<AddressWasApproved>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.AddressPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = AdresStatus.InGebruik;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.StatusName, oldStatus, AdresStatus.InGebruik)
                ]);
            });

            When<Envelope<AddressWasCorrectedFromApprovedToProposed>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.AddressPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = AdresStatus.Voorgesteld;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.StatusName, oldStatus, AdresStatus.Voorgesteld)
                ]);
            });

            When<Envelope<AddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.AddressPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = AdresStatus.Voorgesteld;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.StatusName, oldStatus, AdresStatus.Voorgesteld)
                ]);
            });

            When<Envelope<AddressWasCorrectedFromRejectedToProposed>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.AddressPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = AdresStatus.Voorgesteld;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.StatusName, oldStatus, AdresStatus.Voorgesteld)
                ]);
            });

            When<Envelope<AddressWasCorrectedFromRetiredToCurrent>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.AddressPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = AdresStatus.InGebruik;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.StatusName, oldStatus, AdresStatus.InGebruik)
                ]);
            });

            When<Envelope<AddressWasRejected>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.AddressPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = AdresStatus.Afgekeurd;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.StatusName, oldStatus, AdresStatus.Afgekeurd)
                ]);
            });

            When<Envelope<AddressWasRejectedBecauseHouseNumberWasRejected>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.AddressPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = AdresStatus.Afgekeurd;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.StatusName, oldStatus, AdresStatus.Afgekeurd)
                ]);
            });

            When<Envelope<AddressWasRejectedBecauseHouseNumberWasRetired>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.AddressPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = AdresStatus.Afgekeurd;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.StatusName, oldStatus, AdresStatus.Afgekeurd)
                ]);
            });

            When<Envelope<AddressWasRejectedBecauseStreetNameWasRejected>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.AddressPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = AdresStatus.Afgekeurd;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.StatusName, oldStatus, AdresStatus.Afgekeurd)
                ]);
            });

            When<Envelope<AddressWasRejectedBecauseStreetNameWasRetired>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.AddressPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = AdresStatus.Afgekeurd;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.StatusName, oldStatus, AdresStatus.Afgekeurd)
                ]);
            });

            When<Envelope<AddressWasRejectedBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.AddressPersistentLocalId, ct);

                if (message.Message.NewAddressPersistentLocalId.HasValue)
                {
                    var newDocument = await FindDocument(context, message.Message.NewAddressPersistentLocalId.Value, ct);
                    var transformEvent = new AddressCloudTransformEvent
                    {
                        TransformValues =
                        [
                            new AddressCloudTransformEventValue
                            {
                                From = document.PersistentLocalId.ToString(),
                                To = message.Message.NewAddressPersistentLocalId.Value.ToString()
                            }
                        ],
                        NisCodes = [
                            await GetNisCodeByStreetNamePersistentLocalId(document.Document.StreetNamePersistentLocalId),
                            await GetNisCodeByStreetNamePersistentLocalId(newDocument.Document.StreetNamePersistentLocalId)
                        ]
                    };

                    await AddTransformCloudEvent(message,
                        [document.PersistentLocalId, message.Message.NewAddressPersistentLocalId.Value],
                        context, transformEvent);
                }

                var oldStatus = document.Document.Status;
                document.Document.Status = AdresStatus.Afgekeurd;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.StatusName, oldStatus, AdresStatus.Afgekeurd)
                ]);
            });

            When<Envelope<AddressWasRejectedBecauseOfReaddress>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.AddressPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = AdresStatus.Afgekeurd;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.StatusName, oldStatus, AdresStatus.Afgekeurd)
                ]);
            });

            When<Envelope<AddressWasRetiredV2>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.AddressPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = AdresStatus.Gehistoreerd;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.StatusName, oldStatus, AdresStatus.Gehistoreerd)
                ]);
            });

            When<Envelope<AddressWasRetiredBecauseHouseNumberWasRetired>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.AddressPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = AdresStatus.Gehistoreerd;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.StatusName, oldStatus, AdresStatus.Gehistoreerd)
                ]);
            });

            When<Envelope<AddressWasRetiredBecauseStreetNameWasRejected>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.AddressPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = AdresStatus.Gehistoreerd;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.StatusName, oldStatus, AdresStatus.Gehistoreerd)
                ]);
            });

            When<Envelope<AddressWasRetiredBecauseStreetNameWasRetired>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.AddressPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = AdresStatus.Gehistoreerd;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.StatusName, oldStatus, AdresStatus.Gehistoreerd)
                ]);
            });

            When<Envelope<AddressWasRetiredBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.AddressPersistentLocalId, ct);

                if (message.Message.NewAddressPersistentLocalId.HasValue)
                {
                    var newDocument = await FindDocument(context, message.Message.NewAddressPersistentLocalId.Value, ct);
                    var transformEvent = new AddressCloudTransformEvent
                    {
                        TransformValues =
                        [
                            new AddressCloudTransformEventValue
                            {
                                From = document.PersistentLocalId.ToString(),
                                To = message.Message.NewAddressPersistentLocalId.Value.ToString()
                            }
                        ],
                        NisCodes = [
                            await GetNisCodeByStreetNamePersistentLocalId(document.Document.StreetNamePersistentLocalId),
                            await GetNisCodeByStreetNamePersistentLocalId(newDocument.Document.StreetNamePersistentLocalId)
                        ]
                    };

                    await AddTransformCloudEvent(message,
                        [document.PersistentLocalId, message.Message.NewAddressPersistentLocalId.Value],
                        context, transformEvent);
                }

                var oldStatus = document.Document.Status;
                document.Document.Status = AdresStatus.Gehistoreerd;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.StatusName, oldStatus, AdresStatus.Gehistoreerd)
                ]);
            });

            When<Envelope<AddressWasRetiredBecauseOfReaddress>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.AddressPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = AdresStatus.Gehistoreerd;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.StatusName, oldStatus, AdresStatus.Gehistoreerd)
                ]);
            });

            When<Envelope<AddressPostalCodeWasChangedV2>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.AddressPersistentLocalId, ct);
                var oldPostalCode = document.Document.PostalCode;
                document.Document.PostalCode = message.Message.PostalCode;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.PostalCode, oldPostalCode, message.Message.PostalCode)
                ]);

                foreach (var boxNumberPersistentLocalId in message.Message.BoxNumberPersistentLocalIds)
                {
                    var boxDocument = await FindDocument(context, boxNumberPersistentLocalId, ct);
                    var oldBoxPostalCode = boxDocument.Document.PostalCode;
                    boxDocument.Document.PostalCode = message.Message.PostalCode;
                    boxDocument.LastChangedOn = message.Message.Provenance.Timestamp;

                    await AddCloudEvent(message, boxDocument, context, [
                        new BaseRegistriesCloudEventAttribute(AddressAttributeNames.PostalCode, oldBoxPostalCode, message.Message.PostalCode)
                    ]);
                }
            });

            When<Envelope<AddressPostalCodeWasCorrectedV2>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.AddressPersistentLocalId, ct);
                var oldPostalCode = document.Document.PostalCode;
                document.Document.PostalCode = message.Message.PostalCode;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.PostalCode, oldPostalCode, message.Message.PostalCode)
                ]);

                foreach (var boxNumberPersistentLocalId in message.Message.BoxNumberPersistentLocalIds)
                {
                    var boxDocument = await FindDocument(context, boxNumberPersistentLocalId, ct);
                    var oldBoxPostalCode = boxDocument.Document.PostalCode;
                    boxDocument.Document.PostalCode = message.Message.PostalCode;
                    boxDocument.LastChangedOn = message.Message.Provenance.Timestamp;

                    await AddCloudEvent(message, boxDocument, context, [
                        new BaseRegistriesCloudEventAttribute(AddressAttributeNames.PostalCode, oldBoxPostalCode, message.Message.PostalCode)
                    ]);
                }
            });

            When<Envelope<AddressHouseNumberWasCorrectedV2>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.AddressPersistentLocalId, ct);
                var oldHouseNumber = document.Document.HouseNumber;
                document.Document.HouseNumber = message.Message.HouseNumber;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.HouseNumber, oldHouseNumber, message.Message.HouseNumber)
                ]);

                foreach (var boxNumberPersistentLocalId in message.Message.BoxNumberPersistentLocalIds)
                {
                    var boxDocument = await FindDocument(context, boxNumberPersistentLocalId, ct);
                    var oldBoxHouseNumber = boxDocument.Document.HouseNumber;
                    boxDocument.Document.HouseNumber = message.Message.HouseNumber;
                    boxDocument.LastChangedOn = message.Message.Provenance.Timestamp;

                    await AddCloudEvent(message, boxDocument, context, [
                        new BaseRegistriesCloudEventAttribute(AddressAttributeNames.HouseNumber, oldBoxHouseNumber, message.Message.HouseNumber)
                    ]);
                }
            });

            When<Envelope<AddressBoxNumberWasCorrectedV2>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.AddressPersistentLocalId, ct);
                var oldBoxNumber = document.Document.BoxNumber;
                document.Document.BoxNumber = message.Message.BoxNumber;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.BoxNumber, oldBoxNumber, message.Message.BoxNumber)
                ]);
            });

            When<Envelope<AddressBoxNumbersWereCorrected>>(async (context, message, ct) =>
            {
                foreach (var (addressPersistentLocalId, boxNumber) in message.Message.AddressBoxNumbers)
                {
                    var document = await FindDocument(context, addressPersistentLocalId, ct);
                    var oldBoxNumber = document.Document.BoxNumber;
                    document.Document.BoxNumber = boxNumber;
                    document.LastChangedOn = message.Message.Provenance.Timestamp;

                    await AddCloudEvent(message, document, context, [
                        new BaseRegistriesCloudEventAttribute(AddressAttributeNames.BoxNumber, oldBoxNumber, boxNumber)
                    ]);
                }
            });

            When<Envelope<AddressPositionWasChanged>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.AddressPersistentLocalId, ct);
                var oldMethod = document.Document.PositionGeometryMethod;
                var oldSpecification = document.Document.PositionSpecification;
                var oldEwkb = document.Document.ExtendedWkbGeometry;

                var newGeometry = GmlHelpers.ParseGeometry(message.Message.ExtendedWkbGeometry);
                document.Document.ExtendedWkbGeometry = message.Message.ExtendedWkbGeometry;
                document.Document.PositionAsGml = newGeometry.ConvertToGml();
                document.Document.PositionGeometryMethod = MapGeometryMethod(message.Message.GeometryMethod);
                document.Document.PositionSpecification = MapGeometrySpecification(message.Message.GeometrySpecification);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                var attributes = new List<BaseRegistriesCloudEventAttribute>();

                if (oldMethod != document.Document.PositionGeometryMethod)
                    attributes.Add(new BaseRegistriesCloudEventAttribute(AddressAttributeNames.PositionGeometryMethod, oldMethod, document.Document.PositionGeometryMethod));

                if (oldSpecification != document.Document.PositionSpecification)
                    attributes.Add(new BaseRegistriesCloudEventAttribute(AddressAttributeNames.PositionSpecification, oldSpecification, document.Document.PositionSpecification));

                var oldPositionValues = oldEwkb is not null ? CreatePositionValues(GmlHelpers.ParseGeometry(oldEwkb)) : null;
                attributes.Add(new BaseRegistriesCloudEventAttribute(AddressAttributeNames.Position, oldPositionValues, CreatePositionValues(newGeometry)));

                await AddCloudEvent(message, document, context, attributes);
            });

            When<Envelope<AddressPositionWasCorrectedV2>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.AddressPersistentLocalId, ct);
                var oldMethod = document.Document.PositionGeometryMethod;
                var oldSpecification = document.Document.PositionSpecification;
                var oldEwkb = document.Document.ExtendedWkbGeometry;

                var newGeometry = GmlHelpers.ParseGeometry(message.Message.ExtendedWkbGeometry);
                document.Document.ExtendedWkbGeometry = message.Message.ExtendedWkbGeometry;
                document.Document.PositionAsGml = newGeometry.ConvertToGml();
                document.Document.PositionGeometryMethod = MapGeometryMethod(message.Message.GeometryMethod);
                document.Document.PositionSpecification = MapGeometrySpecification(message.Message.GeometrySpecification);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                var attributes = new List<BaseRegistriesCloudEventAttribute>();

                if (oldMethod != document.Document.PositionGeometryMethod)
                    attributes.Add(new BaseRegistriesCloudEventAttribute(AddressAttributeNames.PositionGeometryMethod, oldMethod, document.Document.PositionGeometryMethod));

                if (oldSpecification != document.Document.PositionSpecification)
                    attributes.Add(new BaseRegistriesCloudEventAttribute(AddressAttributeNames.PositionSpecification, oldSpecification, document.Document.PositionSpecification));

                var oldPositionValues = oldEwkb is not null ? CreatePositionValues(GmlHelpers.ParseGeometry(oldEwkb)) : null;
                attributes.Add(new BaseRegistriesCloudEventAttribute(AddressAttributeNames.Position, oldPositionValues, CreatePositionValues(newGeometry)));

                await AddCloudEvent(message, document, context, attributes);
            });

            When<Envelope<AddressWasDeregulated>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.AddressPersistentLocalId, ct);
                var oldValue = document.Document.OfficiallyAssigned;
                document.Document.OfficiallyAssigned = false;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                var oldStatus = document.Document.Status;
                document.Document.Status = AdresStatus.InGebruik;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                List<BaseRegistriesCloudEventAttribute> baseRegistriesCloudEventAttributes = [
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.OfficiallyAssigned, oldValue, false),
                ];

                if(oldStatus != AdresStatus.InGebruik)
                    baseRegistriesCloudEventAttributes.Add(new BaseRegistriesCloudEventAttribute(AddressAttributeNames.StatusName, oldStatus, AdresStatus.InGebruik));

                await AddCloudEvent(message, document, context, baseRegistriesCloudEventAttributes);
            });

            When<Envelope<AddressWasRegularized>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.AddressPersistentLocalId, ct);
                var oldValue = document.Document.OfficiallyAssigned;
                document.Document.OfficiallyAssigned = true;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.OfficiallyAssigned, oldValue, true)
                ]);
            });

            When<Envelope<AddressDeregulationWasCorrected>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.AddressPersistentLocalId, ct);
                var oldValue = document.Document.OfficiallyAssigned;
                document.Document.OfficiallyAssigned = true;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.OfficiallyAssigned, oldValue, true)
                ]);
            });

            When<Envelope<AddressRegularizationWasCorrected>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.AddressPersistentLocalId, ct);
                var oldValue = document.Document.OfficiallyAssigned;
                document.Document.OfficiallyAssigned = false;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.OfficiallyAssigned, oldValue, false)
                ]);
            });

            When<Envelope<AddressHouseNumberWasReaddressed>>(async (context, message, ct) =>
            {
                // Look-up if we need to produce the missing, new StreetNameWasReaddressed event.
                var houseNumberWasReaddressedPositions = AddressHouseNumberWasReaddressedPositionsToStreetNameWasReaddressed.List
                    .SingleOrDefault(x => x.EndPosition == message.Position);
                if (houseNumberWasReaddressedPositions is null)
                {
                    return;
                }

                // Combine all AddressHouseNumberWasReaddressed events belonging to the same readdress action
                // and produce the new StreetNameWasReaddressed event.
                var streamPages = new List<ReadStreamPage>();
                var streamPage = await streamStore.ReadStreamForwards(
                    new StreetNameStreamId(new StreetNamePersistentLocalId(message.Message.StreetNamePersistentLocalId)),
                    houseNumberWasReaddressedPositions.BeginPosition,
                    houseNumberWasReaddressedPositions.EndPosition - houseNumberWasReaddressedPositions.BeginPosition + 1,
                    cancellationToken: ct);
                streamPages.Add(streamPage);

                while (!streamPage.IsEnd)
                {
                    streamPage = await streamPage.ReadNext(ct);
                    streamPages.Add(streamPage);
                }

                var eventsToBatch = new List<AddressHouseNumberWasReaddressed>();
                foreach (var streamMessage in streamPages.SelectMany(x => x.Messages).OrderBy(x => x.Position))
                {
                    if (streamMessage.Type != AddressHouseNumberWasReaddressed.EventName) continue;
                    var @event = await streamMessage.GetJsonDataAs<AddressHouseNumberWasReaddressed>(cancellationToken: ct);
                    eventsToBatch.Add(@event);
                }

                if (eventsToBatch.Count == 0)
                {
                    return;
                }

                var readdressedData = eventsToBatch
                    .Select(x => new AddressHouseNumberReaddressedData(
                        x.AddressPersistentLocalId,
                        x.ReaddressedHouseNumber,
                        x.ReaddressedBoxNumbers.Select(y => y).ToList()))
                    .ToList();

                // first retrieve all documents to ensure they exist
                var documentsByAddressId = new Dictionary<int, AddressDocument>();
                foreach (var readdressedHouseNumber in readdressedData)
                {
                    var houseNumberDocument = await FindDocument(context, readdressedHouseNumber.AddressPersistentLocalId, ct);
                    documentsByAddressId[readdressedHouseNumber.AddressPersistentLocalId] = houseNumberDocument;

                    foreach (var readdressedBoxNumber in readdressedHouseNumber.ReaddressedBoxNumbers)
                    {
                        var boxNumberDocument = await FindDocument(context, readdressedBoxNumber.DestinationAddressPersistentLocalId, ct);
                        documentsByAddressId[readdressedBoxNumber.DestinationAddressPersistentLocalId] = boxNumberDocument;
                    }
                }

                // build transform event
                var transformValues = new List<AddressCloudTransformEventValue>();
                var addressPersistentLocalIds = new List<int>();

                foreach (var readdressedHouseNumber in readdressedData)
                {
                    var readdressed = readdressedHouseNumber.ReaddressedHouseNumber;
                    transformValues.Add(new AddressCloudTransformEventValue
                    {
                        From = readdressed.SourceAddressPersistentLocalId.ToString(),
                        To = readdressed.DestinationAddressPersistentLocalId.ToString()
                    });
                    addressPersistentLocalIds.Add(readdressed.DestinationAddressPersistentLocalId);

                    foreach (var readdressedBoxNumber in readdressedHouseNumber.ReaddressedBoxNumbers)
                    {
                        transformValues.Add(new AddressCloudTransformEventValue
                        {
                            From = readdressedBoxNumber.SourceAddressPersistentLocalId.ToString(),
                            To = readdressedBoxNumber.DestinationAddressPersistentLocalId.ToString()
                        });
                        addressPersistentLocalIds.Add(readdressedBoxNumber.DestinationAddressPersistentLocalId);
                    }
                }

                var nisCode = await GetNisCodeByStreetNamePersistentLocalId(message.Message.StreetNamePersistentLocalId);
                var transformEvent = new AddressCloudTransformEvent
                {
                    TransformValues = transformValues,
                    NisCodes = [nisCode]
                };

                await AddTransformCloudEvent(message, addressPersistentLocalIds, context, transformEvent);

                // update documents
                foreach (var readdressedHouseNumber in readdressedData)
                {
                    var houseNumberDocument = documentsByAddressId[readdressedHouseNumber.AddressPersistentLocalId];
                    var readdressed = readdressedHouseNumber.ReaddressedHouseNumber;

                    var attributes = UpdateBaseRegistriesCloudEventAttributes(houseNumberDocument, readdressed);

                    await AddCloudEvent(message, houseNumberDocument, context, attributes);

                    foreach (var readdressedBoxNumber in readdressedHouseNumber.ReaddressedBoxNumbers)
                    {
                        var boxNumberDocument = documentsByAddressId[readdressedBoxNumber.DestinationAddressPersistentLocalId];
                        var boxNumberAttributes = UpdateBaseRegistriesCloudEventAttributes(boxNumberDocument, readdressedBoxNumber);

                        await AddCloudEvent(message, boxNumberDocument, context, boxNumberAttributes);
                    }
                }

                return;

                List<BaseRegistriesCloudEventAttribute> UpdateBaseRegistriesCloudEventAttributes(AddressDocument addressDocument, ReaddressedAddressData readdressed)
                {
                    var oldPostalCode = addressDocument.Document.PostalCode;
                    var oldHouseNumber = addressDocument.Document.HouseNumber;
                    var oldBoxNumber = addressDocument.Document.BoxNumber;
                    var oldStatus = addressDocument.Document.Status;
                    var oldOfficiallyAssigned = addressDocument.Document.OfficiallyAssigned;
                    var oldGeometryMethod = addressDocument.Document.PositionGeometryMethod;
                    var oldGeometrySpecification = addressDocument.Document.PositionSpecification;
                    var oldEwkb = addressDocument.Document.ExtendedWkbGeometry;

                    addressDocument.Document.PostalCode = readdressed.SourcePostalCode;
                    addressDocument.Document.HouseNumber = readdressed.DestinationHouseNumber;
                    addressDocument.Document.Status = MapStatus(readdressed.SourceStatus);
                    addressDocument.Document.OfficiallyAssigned = readdressed.SourceIsOfficiallyAssigned;
                    addressDocument.LastChangedOn = message.Message.Provenance.Timestamp;

                    var attributes = new List<BaseRegistriesCloudEventAttribute>();
                    if(oldHouseNumber != addressDocument.Document.HouseNumber)
                        attributes.Add(new BaseRegistriesCloudEventAttribute(AddressAttributeNames.HouseNumber, oldHouseNumber, readdressed.DestinationHouseNumber));
                    if(oldBoxNumber != addressDocument.Document.BoxNumber)
                        attributes.Add(new BaseRegistriesCloudEventAttribute(AddressAttributeNames.BoxNumber, oldBoxNumber, addressDocument.Document.BoxNumber));
                    if(oldPostalCode != addressDocument.Document.PostalCode)
                        attributes.Add(new BaseRegistriesCloudEventAttribute(AddressAttributeNames.PostalCode, oldPostalCode, readdressed.SourcePostalCode));
                    if(oldStatus != addressDocument.Document.Status)
                        attributes.Add(new BaseRegistriesCloudEventAttribute(AddressAttributeNames.StatusName, oldStatus, addressDocument.Document.Status));
                    if(oldOfficiallyAssigned != addressDocument.Document.OfficiallyAssigned)
                        attributes.Add(new BaseRegistriesCloudEventAttribute(AddressAttributeNames.OfficiallyAssigned, oldOfficiallyAssigned, addressDocument.Document.OfficiallyAssigned));
                    if(oldGeometryMethod != addressDocument.Document.PositionGeometryMethod)
                        attributes.Add(new BaseRegistriesCloudEventAttribute(AddressAttributeNames.PositionGeometryMethod, oldGeometryMethod, addressDocument.Document.PositionGeometryMethod));
                    if(oldGeometrySpecification != addressDocument.Document.PositionSpecification)
                        attributes.Add(new BaseRegistriesCloudEventAttribute(AddressAttributeNames.PositionSpecification, oldGeometrySpecification, addressDocument.Document.PositionSpecification));
                    if(oldEwkb != addressDocument.Document.ExtendedWkbGeometry)
                    {
                        var oldPositionValues = oldEwkb is not null ? CreatePositionValues(GmlHelpers.ParseGeometry(oldEwkb)) : null;
                        var newPositionValues = CreatePositionValues(GmlHelpers.ParseGeometry(addressDocument.Document.ExtendedWkbGeometry));
                        attributes.Add(new BaseRegistriesCloudEventAttribute(AddressAttributeNames.Position, oldPositionValues, newPositionValues));
                    }

                    return attributes;
                }
            });

            When<Envelope<AddressWasRemovedV2>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.AddressPersistentLocalId, ct);
                document.IsRemoved = true;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [], AddressEventTypes.DeleteV1);
            });

            When<Envelope<AddressWasRemovedBecauseStreetNameWasRemoved>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.AddressPersistentLocalId, ct);
                document.IsRemoved = true;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [], AddressEventTypes.DeleteV1);
            });

            When<Envelope<AddressWasRemovedBecauseHouseNumberWasRemoved>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.AddressPersistentLocalId, ct);
                document.IsRemoved = true;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [], AddressEventTypes.DeleteV1);
            });

            When<Envelope<AddressRemovalWasCorrected>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.AddressPersistentLocalId, ct);
                document.IsRemoved = false;
                document.Document.Status = MapStatus(message.Message.Status);
                document.Document.HouseNumber = message.Message.HouseNumber;
                document.Document.BoxNumber = message.Message.BoxNumber;
                document.Document.PostalCode = message.Message.PostalCode ?? string.Empty;
                document.Document.OfficiallyAssigned = message.Message.OfficiallyAssigned;

                var geometry = GmlHelpers.ParseGeometry(message.Message.ExtendedWkbGeometry);
                document.Document.ExtendedWkbGeometry = message.Message.ExtendedWkbGeometry;
                document.Document.PositionAsGml = geometry.ConvertToGml();
                document.Document.PositionGeometryMethod = MapGeometryMethod(message.Message.GeometryMethod);
                document.Document.PositionSpecification = MapGeometrySpecification(message.Message.GeometrySpecification);

                document.LastChangedOn = message.Message.Provenance.Timestamp;

                List<BaseRegistriesCloudEventAttribute> attributes = [
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.StreetNameId, null, message.Message.StreetNamePersistentLocalId),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.StatusName, null, document.Document.Status),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.HouseNumber, null, document.Document.HouseNumber),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.PostalCode, null, document.Document.PostalCode),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.OfficiallyAssigned, null, document.Document.OfficiallyAssigned),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.PositionGeometryMethod, null, document.Document.PositionGeometryMethod),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.PositionSpecification, null, document.Document.PositionSpecification),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.Position, null, CreatePositionValues(geometry))
                ];

                if (!string.IsNullOrEmpty(document.Document.BoxNumber))
                    attributes.Add(new BaseRegistriesCloudEventAttribute(AddressAttributeNames.BoxNumber, null, document.Document.BoxNumber));

                await AddCloudEvent(message, document, context, attributes, AddressEventTypes.CreateV1);
            });

            // StreetName events that update address versions
            When<Envelope<StreetNameNamesWereChanged>>(async (context, message, ct) =>
            {
                foreach (var addressPersistentLocalId in message.Message.AddressPersistentLocalIds)
                {
                    var document = await FindDocument(context, addressPersistentLocalId, ct);
                    document.LastChangedOn = message.Message.Provenance.Timestamp;

                    await AddCloudEvent(message, document, context, []);
                }
            });

            When<Envelope<StreetNameNamesWereCorrected>>(async (context, message, ct) =>
            {
                foreach (var addressPersistentLocalId in message.Message.AddressPersistentLocalIds)
                {
                    var document = await FindDocument(context, addressPersistentLocalId, ct);
                    document.LastChangedOn = message.Message.Provenance.Timestamp;

                    await AddCloudEvent(message, document, context, []);
                }
            });

            When<Envelope<StreetNameHomonymAdditionsWereCorrected>>(async (context, message, ct) =>
            {
                foreach (var addressPersistentLocalId in message.Message.AddressPersistentLocalIds)
                {
                    var document = await FindDocument(context, addressPersistentLocalId, ct);
                    document.LastChangedOn = message.Message.Provenance.Timestamp;

                    await AddCloudEvent(message, document, context, []);
                }
            });

            When<Envelope<StreetNameHomonymAdditionsWereRemoved>>(async (context, message, ct) =>
            {
                foreach (var addressPersistentLocalId in message.Message.AddressPersistentLocalIds)
                {
                    var document = await FindDocument(context, addressPersistentLocalId, ct);
                    document.LastChangedOn = message.Message.Provenance.Timestamp;

                    await AddCloudEvent(message, document, context, []);
                }
            });

            When<Envelope<StreetNameWasReaddressed>>(async (context, message, ct) =>
            {
                // first retrieve all documents to ensure they exist
                var documentsByAddressId = new Dictionary<int, AddressDocument>();
                foreach (var readdressedHouseNumber in message.Message.ReaddressedHouseNumbers)
                {
                    var houseNumberDocument = await FindDocument(context, readdressedHouseNumber.AddressPersistentLocalId, ct);
                    documentsByAddressId[readdressedHouseNumber.AddressPersistentLocalId] = houseNumberDocument;

                    foreach (var readdressedBoxNumber in readdressedHouseNumber.ReaddressedBoxNumbers)
                    {
                        var boxNumberDocument = await FindDocument(context, readdressedBoxNumber.DestinationAddressPersistentLocalId, ct);
                        documentsByAddressId[readdressedBoxNumber.DestinationAddressPersistentLocalId] = boxNumberDocument;
                    }
                }

                // build transform event
                var transformValues = new List<AddressCloudTransformEventValue>();
                var addressPersistentLocalIds = new List<int>();

                foreach (var readdressedHouseNumber in message.Message.ReaddressedHouseNumbers)
                {
                    var readdressed = readdressedHouseNumber.ReaddressedHouseNumber;
                    transformValues.Add(new AddressCloudTransformEventValue
                    {
                        From = readdressed.SourceAddressPersistentLocalId.ToString(),
                        To = readdressed.DestinationAddressPersistentLocalId.ToString()
                    });
                    addressPersistentLocalIds.Add(readdressed.DestinationAddressPersistentLocalId);

                    foreach (var readdressedBoxNumber in readdressedHouseNumber.ReaddressedBoxNumbers)
                    {
                        transformValues.Add(new AddressCloudTransformEventValue
                        {
                            From = readdressedBoxNumber.SourceAddressPersistentLocalId.ToString(),
                            To = readdressedBoxNumber.DestinationAddressPersistentLocalId.ToString()
                        });
                        addressPersistentLocalIds.Add(readdressedBoxNumber.DestinationAddressPersistentLocalId);
                    }
                }

                var nisCode = await GetNisCodeByStreetNamePersistentLocalId(message.Message.StreetNamePersistentLocalId);
                var transformEvent = new AddressCloudTransformEvent
                {
                    TransformValues = transformValues,
                    NisCodes = [nisCode]
                };

                await AddTransformCloudEvent(message, addressPersistentLocalIds, context, transformEvent);

                // update documents
                foreach (var readdressedHouseNumber in message.Message.ReaddressedHouseNumbers)
                {
                    var houseNumberDocument = documentsByAddressId[readdressedHouseNumber.AddressPersistentLocalId];
                    var readdressed = readdressedHouseNumber.ReaddressedHouseNumber;

                    var attributes = UpdateBaseRegistriesCloudEventAttributes(houseNumberDocument, readdressed);

                    await AddCloudEvent(message, houseNumberDocument, context, attributes);

                    foreach (var readdressedBoxNumber in readdressedHouseNumber.ReaddressedBoxNumbers)
                    {
                        var boxNumberDocument = documentsByAddressId[readdressedBoxNumber.DestinationAddressPersistentLocalId];
                        var boxNumberAttributes = UpdateBaseRegistriesCloudEventAttributes(boxNumberDocument, readdressedBoxNumber);

                        await AddCloudEvent(message, boxNumberDocument, context, boxNumberAttributes);
                    }
                }

                return;

                List<BaseRegistriesCloudEventAttribute> UpdateBaseRegistriesCloudEventAttributes(AddressDocument addressDocument, ReaddressedAddressData readdressed)
                {
                    var oldPostalCode = addressDocument.Document.PostalCode;
                    var oldHouseNumber = addressDocument.Document.HouseNumber;
                    var oldBoxNumber = addressDocument.Document.BoxNumber;
                    var oldStatus = addressDocument.Document.Status;
                    var oldOfficiallyAssigned = addressDocument.Document.OfficiallyAssigned;
                    var oldGeometryMethod = addressDocument.Document.PositionGeometryMethod;
                    var oldGeometrySpecification = addressDocument.Document.PositionSpecification;
                    var oldEwkb = addressDocument.Document.ExtendedWkbGeometry;

                    addressDocument.Document.PostalCode = readdressed.SourcePostalCode;
                    addressDocument.Document.HouseNumber = readdressed.DestinationHouseNumber;
                    addressDocument.Document.Status = MapStatus(readdressed.SourceStatus);
                    addressDocument.Document.OfficiallyAssigned = readdressed.SourceIsOfficiallyAssigned;
                    addressDocument.LastChangedOn = message.Message.Provenance.Timestamp;

                    var attributes = new List<BaseRegistriesCloudEventAttribute>();
                    if(oldHouseNumber != addressDocument.Document.HouseNumber)
                        attributes.Add(new BaseRegistriesCloudEventAttribute(AddressAttributeNames.HouseNumber, oldHouseNumber, readdressed.DestinationHouseNumber));
                    if(oldBoxNumber != addressDocument.Document.BoxNumber)
                        attributes.Add(new BaseRegistriesCloudEventAttribute(AddressAttributeNames.BoxNumber, oldBoxNumber, addressDocument.Document.BoxNumber));
                    if(oldPostalCode != addressDocument.Document.PostalCode)
                        attributes.Add(new BaseRegistriesCloudEventAttribute(AddressAttributeNames.PostalCode, oldPostalCode, readdressed.SourcePostalCode));
                    if(oldStatus != addressDocument.Document.Status)
                        attributes.Add(new BaseRegistriesCloudEventAttribute(AddressAttributeNames.StatusName, oldStatus, addressDocument.Document.Status));
                    if(oldOfficiallyAssigned != addressDocument.Document.OfficiallyAssigned)
                        attributes.Add(new BaseRegistriesCloudEventAttribute(AddressAttributeNames.OfficiallyAssigned, oldOfficiallyAssigned, addressDocument.Document.OfficiallyAssigned));
                    if(oldGeometryMethod != addressDocument.Document.PositionGeometryMethod)
                        attributes.Add(new BaseRegistriesCloudEventAttribute(AddressAttributeNames.PositionGeometryMethod, oldGeometryMethod, addressDocument.Document.PositionGeometryMethod));
                    if(oldGeometrySpecification != addressDocument.Document.PositionSpecification)
                        attributes.Add(new BaseRegistriesCloudEventAttribute(AddressAttributeNames.PositionSpecification, oldGeometrySpecification, addressDocument.Document.PositionSpecification));
                    if(oldEwkb != addressDocument.Document.ExtendedWkbGeometry)
                    {
                        var oldPositionValues = oldEwkb is not null ? CreatePositionValues(GmlHelpers.ParseGeometry(oldEwkb)) : null;
                        var newPositionValues = CreatePositionValues(GmlHelpers.ParseGeometry(addressDocument.Document.ExtendedWkbGeometry));
                        attributes.Add(new BaseRegistriesCloudEventAttribute(AddressAttributeNames.Position, oldPositionValues, newPositionValues));
                    }

                    return attributes;
                }
            });

            // StreetName events that don't affect address documents
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
            When<Envelope<StreetNameWasRenamed>>(DoNothing);
        }

        private static async Task<AddressDocument> FindDocument(FeedContext context, int persistentLocalId, CancellationToken ct)
        {
            var document = await context.AddressDocuments.FindAsync([persistentLocalId], cancellationToken: ct);
            if (document is null)
                throw new InvalidOperationException($"Could not find document for address {persistentLocalId}");
            return document;
        }

        private async Task AddCloudEvent<T>(
            Envelope<T> message,
            AddressDocument document,
            FeedContext context,
            List<BaseRegistriesCloudEventAttribute> attributes,
            string eventType = AddressEventTypes.UpdateV1)
            where T : IHasProvenance, IMessage
        {
            context.Entry(document).Property(x => x.Document).IsModified = true;

            var page = await context.CalculatePage();
            var addressFeedItem = new AddressFeedItem(
                position: message.Position,
                page: page)
            {
                Application = message.Message.Provenance.Application,
                Modification = message.Message.Provenance.Modification,
                Operator = message.Message.Provenance.Operator,
                Organisation = message.Message.Provenance.Organisation,
                Reason = message.Message.Provenance.Reason
            };
            await context.AddressFeed.AddAsync(addressFeedItem);
            await context.AddressFeedItemAddresses.AddAsync(
                new AddressFeedItemAddress(addressFeedItem.Id, document.PersistentLocalId));

            var nisCode = await GetNisCodeByStreetNamePersistentLocalId(document.Document.StreetNamePersistentLocalId);

            var cloudEvent = _changeFeedService.CreateCloudEventWithData(
                addressFeedItem.Id,
                message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                eventType,
                document.PersistentLocalId.ToString(),
                document.LastChangedOnAsDateTimeOffset,
                [nisCode],
                attributes,
                message.EventName,
                message.Metadata["CommandId"].ToString()!);

            addressFeedItem.CloudEventAsString = _changeFeedService.SerializeCloudEvent(cloudEvent);
            await CheckToUpdateCache(page, context);
        }

        private async Task<string> GetNisCodeByStreetNamePersistentLocalId(int streetNamePersistentLocalId)
        {
            await using var streetNameConsumerContext = await _streetNameConsumerContextFactory.CreateDbContextAsync();
            var nisCode = await streetNameConsumerContext.StreetNameLatestItems.AsNoTracking()
                .Where(x => x.PersistentLocalId == streetNamePersistentLocalId)
                .Select(x => x.NisCode)
                .FirstOrDefaultAsync();

            if (nisCode is null)
                throw new InvalidOperationException("Could not find NIS code for street name with persistent local id " +
                                                    streetNamePersistentLocalId);

            return nisCode;
        }

        private async Task AddTransformCloudEvent<T>(
            Envelope<T> message,
            List<int> addressPersistentLocalIds,
            FeedContext context,
            AddressCloudTransformEvent transformEvent)
            where T : IHasProvenance, IMessage
        {
            var page = await context.CalculatePage();
            var addressFeedItem = new AddressFeedItem(
                position: message.Position,
                page: page)
            {
                Application = message.Message.Provenance.Application,
                Modification = message.Message.Provenance.Modification,
                Operator = message.Message.Provenance.Operator,
                Organisation = message.Message.Provenance.Organisation,
                Reason = message.Message.Provenance.Reason
            };
            await context.AddressFeed.AddAsync(addressFeedItem);

            foreach (var addressPersistentLocalId in addressPersistentLocalIds)
            {
                await context.AddressFeedItemAddresses.AddAsync(
                    new AddressFeedItemAddress(addressFeedItem.Id, addressPersistentLocalId));
            }

            var cloudEvent = _changeFeedService.CreateCloudEvent(
                addressFeedItem.Id,
                message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                AddressEventTypes.TransformV1,
                transformEvent,
                _changeFeedService.DataSchemaUriTransform,
                message.EventName,
                message.Metadata["CommandId"].ToString()!);

            addressFeedItem.CloudEventAsString = _changeFeedService.SerializeCloudEvent(cloudEvent);
            await CheckToUpdateCache(page, context);
        }

        private async Task CheckToUpdateCache(int page, FeedContext context)
        {
            await _changeFeedService.CheckToUpdateCacheAsync(
                page,
                context,
                async p =>
                {
                    var localCount = context.AddressFeed.Local
                        .Count(x => x.Page == page && context.Entry(x).State == EntityState.Added);
                    return await context.AddressFeed.CountAsync(x => x.Page == p) + localCount - 1; //exclude current item
                });
        }

        private static AdresStatus MapStatus(AddressStatus status)
        {
            return status switch
            {
                AddressStatus.Proposed => AdresStatus.Voorgesteld,
                AddressStatus.Current => AdresStatus.InGebruik,
                AddressStatus.Retired => AdresStatus.Gehistoreerd,
                AddressStatus.Rejected => AdresStatus.Afgekeurd,
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
            };
        }

        private static PositieGeometrieMethode MapGeometryMethod(GeometryMethod geometryMethod)
        {
            return geometryMethod switch
            {
                GeometryMethod.AppointedByAdministrator => PositieGeometrieMethode.AangeduidDoorBeheerder,
                GeometryMethod.DerivedFromObject => PositieGeometrieMethode.AfgeleidVanObject,
                GeometryMethod.Interpolated => PositieGeometrieMethode.Geinterpoleerd,
                _ => throw new ArgumentOutOfRangeException(nameof(geometryMethod), geometryMethod, null)
            };
        }

        private static PositieSpecificatie MapGeometrySpecification(GeometrySpecification geometrySpecification)
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

        private static List<AddressPositionCloudEventValue> CreatePositionValues(Geometry positionGeometry)
        {
            var list = new List<AddressPositionCloudEventValue>();
            var gml = positionGeometry.ConvertToGml();
            switch (positionGeometry.SRID)
            {
                case SystemReferenceId.SridLambert72:
                {
                    list.Add(new AddressPositionCloudEventValue(gml, SystemReferenceId.SrsNameLambert72));

                    var lambert08Geometry = positionGeometry.TransformFromLambert72To08();
                    list.Add(new AddressPositionCloudEventValue(lambert08Geometry.ConvertToGml(), SystemReferenceId.SrsNameLambert2008));
                    break;
                }
                case SystemReferenceId.SridLambert2008:
                {
                    var lambert72Geometry = positionGeometry.TransformFromLambert08To72();
                    list.Add(new AddressPositionCloudEventValue(lambert72Geometry.ConvertToGml(), SystemReferenceId.SrsNameLambert72));
                    list.Add(new AddressPositionCloudEventValue(gml, SystemReferenceId.SrsNameLambert2008));
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(positionGeometry), positionGeometry, null);
            }

            return list;
        }

        private static Task DoNothing<T>(FeedContext context, Envelope<T> envelope, CancellationToken ct) where T : IMessage => Task.CompletedTask;
    }
}
