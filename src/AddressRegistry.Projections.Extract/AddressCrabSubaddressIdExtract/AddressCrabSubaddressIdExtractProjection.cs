namespace AddressRegistry.Projections.Extract.AddressCrabSubaddressIdExtract
{
    using System;
    using System.Text;
    using Address;
    using Address.Events;
    using Address.Events.Crab;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.GrAr.Extracts;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;

    [ConnectedProjectionName("Extract CRAB-subadressen")]
    [ConnectedProjectionDescription("Projectie die de CRAB-subadressen data voor het CRAB-subadressen extract voorziet.")]
    public class AddressCrabSubaddressIdExtractProjection : ConnectedProjection<ExtractContext>
    {
        private readonly Encoding _encoding;

        public AddressCrabSubaddressIdExtractProjection(Encoding encoding)
        {
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));

            When<Envelope<AddressWasRegistered>>(async (context, message, ct) =>
            {
                await context.AddressCrabSubaddressIdExtract.AddAsync(new AddressCrabSubaddressIdExtractItem
                {
                    AddressId = message.Message.AddressId,
                    DbaseRecord = new AddressCrabSubaddressIdDbaseRecord().ToBytes(_encoding),
                }, cancellationToken: ct);
            });

            When<Envelope<AddressBecameComplete>>(async (context, message, ct) =>
            {
                var item = await context.AddressCrabSubaddressIdExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                UpdateDbaseRecordField(item, record => { record.isvolledig.Value = true; });
            });

            When<Envelope<AddressBecameIncomplete>>(async (context, message, ct) =>
            {
                var item = await context.AddressCrabSubaddressIdExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                UpdateDbaseRecordField(item, record => { record.isvolledig.Value = false; });
            });

            When<Envelope<AddressPersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                var item = await context.AddressCrabSubaddressIdExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                item.PersistentLocalId = message.Message.PersistentLocalId;
                UpdateDbaseRecordField(item, record => { record.adresid.Value = message.Message.PersistentLocalId; });
            });

            When<Envelope<AddressSubaddressWasImportedFromCrab>>(async (context, message, ct) =>
            {
                Guid addressId = AddressId.CreateFor(new CrabSubaddressId(message.Message.SubaddressId));
                var item = await context.AddressCrabSubaddressIdExtract.FindAsync(addressId, cancellationToken: ct);
                item.CrabSubaddressId = message.Message.SubaddressId;

                UpdateDbaseRecordField(item, record => { record.crabsubid.Value = message.Message.SubaddressId; });
            });
        }

        private void UpdateDbaseRecordField(AddressCrabSubaddressIdExtractItem item, Action<AddressCrabSubaddressIdDbaseRecord> update)
        {
            var record = new AddressCrabSubaddressIdDbaseRecord();
            record.FromBytes(item.DbaseRecord, _encoding);
            update(record);
            item.DbaseRecord = record.ToBytes(_encoding);
        }
    }
}
