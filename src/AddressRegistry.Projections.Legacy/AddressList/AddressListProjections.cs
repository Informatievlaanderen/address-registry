namespace AddressRegistry.Projections.Legacy.AddressList
{
    using Address.Events;
    using Address.Events.Crab;
    using Address.ValueObjects;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using NodaTime;

    [ConnectedProjectionName("API endpoint lijst adressen")]
    [ConnectedProjectionDescription("Projectie die de adressen data voor de adressen lijst voorziet.")]
    public class AddressListProjections : ConnectedProjection<LegacyContext>
    {
        public AddressListProjections()
        {
            When<Envelope<AddressWasRegistered>>(async (context, message, ct) =>
            {
                await context
                    .AddressList
                    .AddAsync(
                        new AddressListItem
                        {
                            AddressId = message.Message.AddressId,
                            StreetNameId = message.Message.StreetNameId,
                            HouseNumber = message.Message.HouseNumber,
                        }, ct);
            });

            When<Envelope<AddressBecameComplete>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressListItem(
                    message.Message.AddressId,
                    item =>
                    {
                        item.Complete = true;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<AddressBecameIncomplete>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressListItem(
                    message.Message.AddressId,
                    item =>
                    {
                        item.Complete = false;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<AddressHouseNumberWasChanged>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressListItem(
                    message.Message.AddressId,
                    item =>
                    {
                        item.HouseNumber = message.Message.HouseNumber;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<AddressHouseNumberWasCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressListItem(
                    message.Message.AddressId,
                    item =>
                    {
                        item.HouseNumber = message.Message.HouseNumber;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<AddressPersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressListItem(
                    message.Message.AddressId,
                    item => item.PersistentLocalId = message.Message.PersistentLocalId,
                    ct);
            });

            When<Envelope<AddressPostalCodeWasChanged>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressListItem(
                    message.Message.AddressId,
                    item =>
                    {
                        item.PostalCode = message.Message.PostalCode;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<AddressPostalCodeWasCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressListItem(
                    message.Message.AddressId,
                    item =>
                    {
                        item.PostalCode = message.Message.PostalCode;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<AddressPostalCodeWasRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressListItem(
                    message.Message.AddressId,
                    item =>
                    {
                        item.PostalCode = null;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<AddressStreetNameWasChanged>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressListItem(
                    message.Message.AddressId,
                    item =>
                    {
                        item.StreetNameId = message.Message.StreetNameId;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<AddressStreetNameWasCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressListItem(
                    message.Message.AddressId,
                    item =>
                    {
                        item.StreetNameId = message.Message.StreetNameId;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<AddressWasRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressListItem(
                    message.Message.AddressId,
                    item =>
                    {
                        item.Removed = true;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<AddressBoxNumberWasChanged>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressListItem(
                    message.Message.AddressId,
                    item =>
                    {
                        item.BoxNumber = message.Message.BoxNumber;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });
            When<Envelope<AddressBoxNumberWasCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressListItem(
                    message.Message.AddressId,
                    item =>
                    {
                        item.BoxNumber = message.Message.BoxNumber;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });
            When<Envelope<AddressBoxNumberWasRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressListItem(
                    message.Message.AddressId,
                    item =>
                    {
                        item.BoxNumber = null;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<AddressBecameCurrent>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressListItem(
                    message.Message.AddressId,
                    item =>
                    {
                        item.Status = AddressStatus.Current;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<AddressWasCorrectedToCurrent>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressListItem(
                    message.Message.AddressId,
                    item =>
                    {
                        item.Status = AddressStatus.Current;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<AddressWasProposed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressListItem(
                    message.Message.AddressId,
                    item =>
                    {
                        item.Status = AddressStatus.Proposed;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<AddressWasCorrectedToProposed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressListItem(
                    message.Message.AddressId,
                    item =>
                    {
                        item.Status = AddressStatus.Proposed;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<AddressWasRetired>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressListItem(
                    message.Message.AddressId,
                    item =>
                    {
                        item.Status = AddressStatus.Retired;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<AddressWasCorrectedToRetired>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressListItem(
                    message.Message.AddressId,
                    item =>
                    {
                        item.Status = AddressStatus.Retired;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<AddressStatusWasRemoved>>(async (context, message, ct) => {
                await context.FindAndUpdateAddressListItem(
                    message.Message.AddressId,
                    item =>
                    {
                        item.Status = null;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<AddressStatusWasCorrectedToRemoved>>(async (context, message, ct) => {
                await context.FindAndUpdateAddressListItem(
                    message.Message.AddressId,
                    item =>
                    {
                        item.Status = null;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<AddressWasPositioned>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressPositionWasCorrected>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressPositionWasRemoved>>(async (context, message, ct) => DoNothing());

            When<Envelope<AddressWasOfficiallyAssigned>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressWasCorrectedToOfficiallyAssigned>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressBecameNotOfficiallyAssigned>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressWasCorrectedToNotOfficiallyAssigned>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressOfficialAssignmentWasRemoved>>(async (context, message, ct) => DoNothing());

            When<Envelope<AddressHouseNumberWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressHouseNumberStatusWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressHouseNumberPositionWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressHouseNumberMailCantonWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressSubaddressWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressSubaddressPositionWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressSubaddressStatusWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
        }

        private static void UpdateVersionTimestamp(AddressListItem addressListItem, Instant timestamp)
            => addressListItem.VersionTimestamp = timestamp;

        private static void DoNothing() { }
    }
}
