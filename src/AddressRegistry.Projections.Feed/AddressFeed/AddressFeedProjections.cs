namespace AddressRegistry.Projections.Feed.AddressFeed
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.ChangeFeed;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Contract;
    using Microsoft.EntityFrameworkCore;
    using StreetName;
    using StreetName.Events;

    [ConnectedProjectionName("Feed endpoint adres")]
    [ConnectedProjectionDescription("Projectie die de adres data voor de adres cloudevent feed voorziet.")]
    public class AddressFeedProjections : ConnectedProjection<FeedContext>
    {
        private readonly IChangeFeedService _changeFeedService;

        public AddressFeedProjections(IChangeFeedService changeFeedService)
        {
            _changeFeedService = changeFeedService;

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
                await context.AddressDocuments.AddAsync(document, ct);

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.StreetNameId, null, message.Message.StreetNamePersistentLocalId),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.StatusName, null, document.Document.Status),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.HouseNumber, null, document.Document.HouseNumber),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.PostalCode, null, document.Document.PostalCode),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.OfficiallyAssigned, null, document.Document.OfficiallyAssigned)
                ], AddressEventTypes.CreateV1);
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
                await context.AddressDocuments.AddAsync(document, ct);

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.StreetNameId, null, message.Message.StreetNamePersistentLocalId),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.StatusName, null, AdresStatus.Voorgesteld),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.HouseNumber, null, document.Document.HouseNumber),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.PostalCode, null, document.Document.PostalCode),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.OfficiallyAssigned, null, document.Document.OfficiallyAssigned)
                ], AddressEventTypes.CreateV1);
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
                await context.AddressDocuments.AddAsync(document, ct);

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.StreetNameId, null, message.Message.StreetNamePersistentLocalId),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.StatusName, null, document.Document.Status),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.HouseNumber, null, document.Document.HouseNumber),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.PostalCode, null, document.Document.PostalCode),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.OfficiallyAssigned, null, document.Document.OfficiallyAssigned)
                ], AddressEventTypes.CreateV1);

                await AddTransformCloudEvent(message, document, context,
                    new AddressCloudTransformEvent
                    {
                        From = [message.Message.MergedAddressPersistentLocalId.ToString()],
                        To = [document.PersistentLocalId.ToString()]
                    });
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
                await context.AddressDocuments.AddAsync(document, ct);

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.StreetNameId, null, message.Message.StreetNamePersistentLocalId),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.StatusName, null, AdresStatus.Voorgesteld),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.HouseNumber, null, document.Document.HouseNumber),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.PostalCode, null, document.Document.PostalCode),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.OfficiallyAssigned, null, document.Document.OfficiallyAssigned)
                ], AddressEventTypes.CreateV1);
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
                var oldStatus = document.Document.Status;
                document.Document.Status = AdresStatus.Afgekeurd;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.StatusName, oldStatus, AdresStatus.Afgekeurd)
                ]);

                var transformEvent = new AddressCloudTransformEvent
                {
                    From = [document.PersistentLocalId.ToString()],
                    To = message.Message.NewAddressPersistentLocalId.HasValue
                        ? [message.Message.NewAddressPersistentLocalId.Value.ToString()]
                        : []
                };

                await AddTransformCloudEvent(message, document, context, transformEvent);
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
                var oldStatus = document.Document.Status;
                document.Document.Status = AdresStatus.Gehistoreerd;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.StatusName, oldStatus, AdresStatus.Gehistoreerd)
                ]);

                var transformEvent = new AddressCloudTransformEvent
                {
                    From = [document.PersistentLocalId.ToString()],
                    To = message.Message.NewAddressPersistentLocalId.HasValue
                        ? [message.Message.NewAddressPersistentLocalId.Value.ToString()]
                        : []
                };

                await AddTransformCloudEvent(message, document, context, transformEvent);
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
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.PositionGeometryMethod, null, message.Message.GeometryMethod),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.PositionSpecification, null, message.Message.GeometrySpecification)
                ]);
            });

            When<Envelope<AddressPositionWasCorrectedV2>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.AddressPersistentLocalId, ct);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.PositionGeometryMethod, null, message.Message.GeometryMethod),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.PositionSpecification, null, message.Message.GeometrySpecification)
                ]);
            });

            When<Envelope<AddressWasDeregulated>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.AddressPersistentLocalId, ct);
                var oldValue = document.Document.OfficiallyAssigned;
                document.Document.OfficiallyAssigned = false;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.OfficiallyAssigned, oldValue, false)
                ]);
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
                var houseNumberDocument = await FindDocument(context, message.Message.AddressPersistentLocalId, ct);
                var readdressed = message.Message.ReaddressedHouseNumber;

                var oldPostalCode = houseNumberDocument.Document.PostalCode;
                var oldHouseNumber = houseNumberDocument.Document.HouseNumber;
                var oldStatus = houseNumberDocument.Document.Status;

                houseNumberDocument.Document.PostalCode = readdressed.SourcePostalCode;
                houseNumberDocument.Document.HouseNumber = readdressed.DestinationHouseNumber;
                houseNumberDocument.Document.Status = MapStatus(readdressed.SourceStatus);
                houseNumberDocument.Document.OfficiallyAssigned = readdressed.SourceIsOfficiallyAssigned;
                houseNumberDocument.LastChangedOn = message.Message.Provenance.Timestamp;

                List<BaseRegistriesCloudEventAttribute> attributes = [
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.PostalCode, oldPostalCode, readdressed.SourcePostalCode),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.HouseNumber, oldHouseNumber, readdressed.DestinationHouseNumber),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.StatusName, oldStatus, houseNumberDocument.Document.Status)
                ];

                await AddCloudEvent(message, houseNumberDocument, context, attributes);

                foreach (var readdressedBoxNumber in message.Message.ReaddressedBoxNumbers)
                {
                    var boxNumberDocument = await FindDocument(context, readdressedBoxNumber.DestinationAddressPersistentLocalId, ct);
                    var oldBoxPostalCode = boxNumberDocument.Document.PostalCode;
                    var oldBoxHouseNumber = boxNumberDocument.Document.HouseNumber;
                    var oldBoxStatus = boxNumberDocument.Document.Status;

                    boxNumberDocument.Document.PostalCode = readdressedBoxNumber.SourcePostalCode;
                    boxNumberDocument.Document.HouseNumber = readdressedBoxNumber.DestinationHouseNumber;
                    boxNumberDocument.Document.BoxNumber = readdressedBoxNumber.SourceBoxNumber;
                    boxNumberDocument.Document.Status = MapStatus(readdressedBoxNumber.SourceStatus);
                    boxNumberDocument.Document.OfficiallyAssigned = readdressedBoxNumber.SourceIsOfficiallyAssigned;
                    boxNumberDocument.LastChangedOn = message.Message.Provenance.Timestamp;

                    List<BaseRegistriesCloudEventAttribute> boxAttributes = [
                        new BaseRegistriesCloudEventAttribute(AddressAttributeNames.PostalCode, oldBoxPostalCode, readdressedBoxNumber.SourcePostalCode),
                        new BaseRegistriesCloudEventAttribute(AddressAttributeNames.HouseNumber, oldBoxHouseNumber, readdressedBoxNumber.DestinationHouseNumber),
                        new BaseRegistriesCloudEventAttribute(AddressAttributeNames.StatusName, oldBoxStatus, boxNumberDocument.Document.Status)
                    ];

                    await AddCloudEvent(message, boxNumberDocument, context, boxAttributes);
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
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.StatusName, null, document.Document.Status),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.HouseNumber, null, document.Document.HouseNumber),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.PostalCode, null, document.Document.PostalCode),
                    new BaseRegistriesCloudEventAttribute(AddressAttributeNames.OfficiallyAssigned, null, document.Document.OfficiallyAssigned)
                ]);
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
            When<Envelope<StreetNameWasReaddressed>>(DoNothing);
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
                page: page,
                persistentLocalId: document.PersistentLocalId)
            {
                Application = message.Message.Provenance.Application,
                Modification = message.Message.Provenance.Modification,
                Operator = message.Message.Provenance.Operator,
                Organisation = message.Message.Provenance.Organisation,
                Reason = message.Message.Provenance.Reason
            };
            await context.AddressFeed.AddAsync(addressFeedItem);

            var cloudEvent = _changeFeedService.CreateCloudEventWithData(
                addressFeedItem.Id,
                message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                eventType,
                document.PersistentLocalId.ToString(),
                document.LastChangedOnAsDateTimeOffset,
                [document.Document.StreetNamePersistentLocalId.ToString()],
                attributes,
                message.EventName,
                message.Metadata["CommandId"].ToString()!);

            addressFeedItem.CloudEventAsString = _changeFeedService.SerializeCloudEvent(cloudEvent);
            await CheckToUpdateCache(page, context);
        }

        private async Task AddTransformCloudEvent<T>(
            Envelope<T> message,
            AddressDocument document,
            FeedContext context,
            AddressCloudTransformEvent transformEvent)
            where T : IHasProvenance, IMessage
        {
            var page = await context.CalculatePage();
            var addressFeedItem = new AddressFeedItem(
                position: message.Position,
                page: page,
                persistentLocalId: document.PersistentLocalId)
            {
                Application = message.Message.Provenance.Application,
                Modification = message.Message.Provenance.Modification,
                Operator = message.Message.Provenance.Operator,
                Organisation = message.Message.Provenance.Organisation,
                Reason = message.Message.Provenance.Reason
            };
            await context.AddressFeed.AddAsync(addressFeedItem);

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
                async p => await context.AddressFeed.CountAsync(x => x.Page == p));
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

        private static Task DoNothing<T>(FeedContext context, Envelope<T> envelope, CancellationToken ct) where T : IMessage => Task.CompletedTask;
    }
}
