namespace AddressRegistry.Projections.Extract.AddressExtract
{
    using Address.Events;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using NetTopologySuite.IO;
    using System;
    using System.Text;
    using Address.Events.Crab;
    using AddressLinkExtract;
    using Microsoft.Extensions.Options;

    public class AddressLinkExtractProjection : ConnectedProjection<ExtractContext>
    {
        private readonly Encoding _encoding;

        public AddressLinkExtractProjection(IOptions<ExtractConfig> extractConfig, Encoding encoding, WKBReader wkbReader)
        {
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));

            When<Envelope<AddressWasRegistered>>(async (context, message, ct) =>
            {
                await context.AddressLinkExtract.AddAsync(new AddressLinkExtractItem
                {
                    AddressId = message.Message.AddressId,
                    StreetNameId = message.Message.StreetNameId,
                    Complete = false,
                    DbaseRecord = new AddressLinkDbaseRecord().ToBytes(_encoding),
                }, cancellationToken: ct);
            });

            When<Envelope<AddressBecameComplete>>(async (context, message, ct) =>
            {
                var item = await context.AddressLinkExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                item.Complete = true;
            });

            When<Envelope<AddressStreetNameWasChanged>>(async (context, message, ct) =>
            {
                var item = await context.AddressLinkExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                item.StreetNameId = message.Message.StreetNameId;
            });

            When<Envelope<AddressStreetNameWasCorrected>>(async (context, message, ct) =>
            {
                var item = await context.AddressLinkExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                item.StreetNameId = message.Message.StreetNameId;
            });

            When<Envelope<AddressBecameIncomplete>>(async (context, message, ct) =>
            {
                var item = await context.AddressLinkExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                if (item != null) // in rare cases were we might get this event after an AddressWasRemoved event, we can just ignore it
                    item.Complete = false;
            });

            When<Envelope<AddressHouseNumberWasChanged>>(async (context, message, ct) =>
            {
                var item = await context.AddressLinkExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                item.HouseNumber = message.Message.HouseNumber;
            });

            When<Envelope<AddressHouseNumberWasCorrected>>(async (context, message, ct) =>
            {
                var item = await context.AddressLinkExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                item.HouseNumber = message.Message.HouseNumber;
            });

            When<Envelope<AddressPersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                var item = await context.AddressLinkExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                if (item != null) // in rare cases were we might get this event after an AddressWasRemoved event, we can just ignore it
                {
                    UpdateDbaseRecordField(item, record => { record.adresid.Value = message.Message.PersistentLocalId; });
                    item.PersistentLocalId = message.Message.PersistentLocalId;
                }
            });

            When<Envelope<AddressPostalCodeWasChanged>>(async (context, message, ct) =>
            {
                var item = await context.AddressLinkExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                item.PostalCode = message.Message.PostalCode;
            });

            When<Envelope<AddressPostalCodeWasCorrected>>(async (context, message, ct) =>
            {
                var item = await context.AddressLinkExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                if (item != null)
                    item.PostalCode = message.Message.PostalCode;
            });

            When<Envelope<AddressPostalCodeWasRemoved>>(async (context, message, ct) =>
            {
                var item = await context.AddressLinkExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                if (item != null)
                    item.PostalCode = null;
            });

            When<Envelope<AddressWasRemoved>>(async (context, message, ct) =>
            {
                var address = await context.AddressLinkExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                context.AddressLinkExtract.Remove(address);
            });

            When<Envelope<AddressBoxNumberWasChanged>>(async (context, message, ct) =>
            {
                var item = await context.AddressLinkExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                item.BoxNumber = message.Message.BoxNumber;
            });

            When<Envelope<AddressBoxNumberWasCorrected>>(async (context, message, ct) =>
            {
                var item = await context.AddressLinkExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                item.BoxNumber = message.Message.BoxNumber;
            });

            When<Envelope<AddressBoxNumberWasRemoved>>(async (context, message, ct) =>
            {
                var item = await context.AddressLinkExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                item.BoxNumber = null;
            });

            When<Envelope<AddressBecameCurrent>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressBecameNotOfficiallyAssigned>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressOfficialAssignmentWasRemoved>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressPositionWasCorrected>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressPositionWasRemoved>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressStatusWasCorrectedToRemoved>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressStatusWasRemoved>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressWasCorrectedToCurrent>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressWasCorrectedToNotOfficiallyAssigned>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressWasCorrectedToOfficiallyAssigned>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressWasCorrectedToProposed>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressWasCorrectedToRetired>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressWasOfficiallyAssigned>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressWasPositioned>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressWasProposed>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressWasRetired>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressHouseNumberWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressHouseNumberStatusWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressHouseNumberPositionWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressHouseNumberMailCantonWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressSubaddressWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressSubaddressPositionWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressSubaddressStatusWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
        }

        private void UpdateDbaseRecordField(AddressLinkExtractItem item, Action<AddressLinkDbaseRecord> update)
        {
            var record = new AddressLinkDbaseRecord();
            record.FromBytes(item.DbaseRecord, _encoding);
            update(record);
            item.DbaseRecord = record.ToBytes(_encoding);
        }

        private static void DoNothing() { }
    }
}
