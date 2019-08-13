namespace AddressRegistry.Projections.Legacy.AddressSyndication
{
    using Address.Events;
    using Address.Events.Crab;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using NetTopologySuite.IO;
    using NodaTime;

    public class AddressSyndicationProjections : ConnectedProjection<LegacyContext>
    {
        public AddressSyndicationProjections(WKBReader wkbReader)
        {
            When<Envelope<AddressWasRegistered>>(async (context, message, ct) =>
            {
                var addressSyndicationItem = new AddressSyndicationItem
                {
                    Position = message.Position,
                    AddressId = message.Message.AddressId,
                    StreetNameId = message.Message.StreetNameId,
                    HouseNumber = message.Message.HouseNumber,
                    RecordCreatedAt = Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    LastChangedOn = Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    ChangeType = message.EventName
                };

                addressSyndicationItem.ApplyProvenance(message.Message.Provenance);
                addressSyndicationItem.SetEventData(message.Message);

                await context
                    .AddressSyndication
                    .AddAsync(addressSyndicationItem, ct);
            });

            When<Envelope<AddressBecameComplete>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.IsComplete = true,
                    ct);
            });

            When<Envelope<AddressBecameCurrent>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.Status = AddressStatus.Current,
                    ct);
            });

            When<Envelope<AddressBecameIncomplete>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.IsComplete = false,
                    ct);
            });

            When<Envelope<AddressBecameNotOfficiallyAssigned>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.IsOfficiallyAssigned = false,
                    ct);
            });

            When<Envelope<AddressBoxNumberWasChanged>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.BoxNumber = message.Message.BoxNumber,
                    ct);
            });

            When<Envelope<AddressBoxNumberWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.BoxNumber = message.Message.BoxNumber,
                    ct);
            });

            When<Envelope<AddressBoxNumberWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.BoxNumber = null,
                    ct);
            });

            When<Envelope<AddressHouseNumberWasChanged>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.HouseNumber = message.Message.HouseNumber,
                    ct);
            });

            When<Envelope<AddressHouseNumberWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.HouseNumber = message.Message.HouseNumber,
                    ct);
            });

            When<Envelope<AddressOfficialAssignmentWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.IsOfficiallyAssigned = false,
                    ct);
            });

            When<Envelope<AddressPositionWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x =>
                    {
                        x.PositionMethod = message.Message.GeometryMethod;
                        x.PositionSpecification = message.Message.GeometrySpecification;
                        x.PointPosition = message.Message.ExtendedWkbGeometry.ToByteArray();
                    },
                    ct);
            });

            When<Envelope<AddressPositionWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x =>
                    {
                        x.PositionMethod = null;
                        x.PositionSpecification = null;
                        x.PointPosition = null;
                    },
                    ct);
            });

            When<Envelope<AddressPostalCodeWasChanged>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.PostalCode = message.Message.PostalCode,
                    ct);
            });

            When<Envelope<AddressPostalCodeWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.PostalCode = message.Message.PostalCode,
                    ct);
            });

            When<Envelope<AddressPostalCodeWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.PostalCode = null,
                    ct);
            });

            When<Envelope<AddressStatusWasCorrectedToRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.Status = null,
                    ct);
            });

            When<Envelope<AddressStatusWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.Status = null,
                    ct);
            });

            When<Envelope<AddressStreetNameWasChanged>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.StreetNameId = message.Message.StreetNameId,
                    ct);
            });

            When<Envelope<AddressStreetNameWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.StreetNameId = message.Message.StreetNameId,
                    ct);
            });

            When<Envelope<AddressWasCorrectedToCurrent>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.Status = AddressStatus.Current,
                    ct);
            });

            When<Envelope<AddressWasCorrectedToOfficiallyAssigned>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.IsOfficiallyAssigned = true,
                    ct);
            });

            When<Envelope<AddressWasCorrectedToNotOfficiallyAssigned>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.IsOfficiallyAssigned = false,
                    ct);
            });

            When<Envelope<AddressWasCorrectedToProposed>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.Status = AddressStatus.Proposed,
                    ct);
            });

            When<Envelope<AddressWasCorrectedToRetired>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.Status = AddressStatus.Retired,
                    ct);
            });

            When<Envelope<AddressWasOfficiallyAssigned>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.IsOfficiallyAssigned = true,
                    ct);
            });

            When<Envelope<AddressWasPositioned>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x =>
                    {
                        x.PositionMethod = message.Message.GeometryMethod;
                        x.PositionSpecification = message.Message.GeometrySpecification;
                        x.PointPosition = message.Message.ExtendedWkbGeometry.ToByteArray();
                    },
                    ct);
            });

            When<Envelope<AddressWasProposed>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.Status = AddressStatus.Proposed,
                    ct);
            });

            When<Envelope<AddressWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => { },
                    ct);
            });

            When<Envelope<AddressWasRetired>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.Status = AddressStatus.Retired,
                    ct);
            });

            When<Envelope<AddressPersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                var addressSyndicationItem = await context.LatestPosition(message.Message.AddressId, ct);

                if (addressSyndicationItem == null)
                    throw AddressSyndicationExtensions.DatabaseItemNotFound(message.Message.AddressId);

                if (addressSyndicationItem.Position >= message.Position)
                    return;

                var newAddressSyndicationItem = addressSyndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x => x.PersistentLocalId = message.Message.PersistentLocalId);

                newAddressSyndicationItem.SetEventData(message.Message);

                await context
                    .AddressSyndication
                    .AddAsync(newAddressSyndicationItem, ct);
            });

            When<Envelope<AddressHouseNumberWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressHouseNumberStatusWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressHouseNumberPositionWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressHouseNumberMailCantonWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressSubaddressWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressSubaddressPositionWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressSubaddressStatusWasImportedFromCrab>>(async (context, message, ct) => DoNothing());

        }

        private static void DoNothing() { }
    }
}
