namespace AddressRegistry.Projections.Extract.AddressCrabHouseNumberIdExtract
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

    [ConnectedProjectionName("Extract CRAB-huisnummers")]
    [ConnectedProjectionDescription("Projectie die de CRAB-huisnummers data voor het CRAB-huisnummers extract voorziet.")]
    public class AddressCrabHouseNumberIdExtractProjection : ConnectedProjection<ExtractContext>
    {
        private readonly Encoding _encoding;

        public AddressCrabHouseNumberIdExtractProjection(Encoding encoding)
        {
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));

            When<Envelope<AddressWasRegistered>>(async (context, message, ct) =>
            {
                await context.AddressCrabHouseNumberIdExtract.AddAsync(new AddressCrabHouseNumberIdExtractItem
                {
                    AddressId = message.Message.AddressId,
                    DbaseRecord = new AddressCrabHouseNumberIdDbaseRecord().ToBytes(_encoding),
                }, cancellationToken: ct);
            });

            When<Envelope<AddressBecameComplete>>(async (context, message, ct) =>
            {
                var item = await context.AddressCrabHouseNumberIdExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                UpdateDbaseRecordField(item, record => { record.isvolledig.Value = true; });
            });

            When<Envelope<AddressBecameIncomplete>>(async (context, message, ct) =>
            {
                var item = await context.AddressCrabHouseNumberIdExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                UpdateDbaseRecordField(item, record => { record.isvolledig.Value = false; });
            });

            When<Envelope<AddressPersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                var item = await context.AddressCrabHouseNumberIdExtract.FindAsync(message.Message.AddressId, cancellationToken: ct);
                item.PersistentLocalId = message.Message.PersistentLocalId;
                UpdateDbaseRecordField(item, record => { record.adresid.Value = message.Message.PersistentLocalId; });
            });

            When<Envelope<AddressHouseNumberWasImportedFromCrab>>(async (context, message, ct) =>
            {
                Guid addressId = AddressId.CreateFor(new CrabHouseNumberId(message.Message.HouseNumberId));
                var item = await context.AddressCrabHouseNumberIdExtract.FindAsync(addressId, cancellationToken: ct);

                if (item != null)
                {
                    item.CrabHouseNumberId = message.Message.HouseNumberId;
                    UpdateDbaseRecordField(item, record => { record.crabhnrid.Value = message.Message.HouseNumberId; });
                }
            });
        }

        private void UpdateDbaseRecordField(AddressCrabHouseNumberIdExtractItem item, Action<AddressCrabHouseNumberIdDbaseRecord> update)
        {
            var record = new AddressCrabHouseNumberIdDbaseRecord();
            record.FromBytes(item.DbaseRecord, _encoding);
            update(record);
            item.DbaseRecord = record.ToBytes(_encoding);
        }
    }
}
