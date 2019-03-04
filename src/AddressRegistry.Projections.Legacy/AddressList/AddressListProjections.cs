namespace AddressRegistry.Projections.Legacy.AddressList
{
    using Address.Events;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using NodaTime;

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

            When<Envelope<AddressOsloIdWasAssigned>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressListItem(
                    message.Message.AddressId,
                    item => item.OsloId = message.Message.OsloId,
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
        }

        private static void UpdateVersionTimestamp(AddressListItem addressListItem, Instant timestamp)
            => addressListItem.VersionTimestamp = timestamp;
    }
}
