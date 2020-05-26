namespace AddressRegistry.Projections.Syndication.Parcel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressLink;
    using Be.Vlaanderen.Basisregisters.GrAr.Extracts;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Syndication;
    using BuildingUnit;

    public class AddressParcelLinkProjections : AtomEntryProjectionHandlerModule<ParcelEvent, SyndicationItem<Parcel>, SyndicationContext>
    {
        private readonly Encoding _encoding;

        public AddressParcelLinkProjections(Encoding encoding)
        {
            _encoding = encoding;

            When(ParcelEvent.ParcelWasRegistered, AddSyndicationItemEntry);
            When(ParcelEvent.ParcelWasRemoved, RemoveParcel);
            When(ParcelEvent.ParcelAddressWasAttached, AddSyndicationItemEntry);
            When(ParcelEvent.ParcelAddressWasDetached, AddSyndicationItemEntry);
        }

        private static async Task RemoveParcel(AtomEntry<SyndicationItem<Parcel>> entry, SyndicationContext context, CancellationToken ct)
        {
            var addressParcelLinkExtractItems =
                context
                    .AddressParcelLinkExtract
                    .Where(x => x.ParcelId == entry.Content.Object.Id)
                    .AsEnumerable()
                    .Concat(context.AddressParcelLinkExtract.Local.Where(x => x.ParcelId == entry.Content.Object.Id))
                    .ToList();

            context.AddressParcelLinkExtract.RemoveRange(addressParcelLinkExtractItems);
        }

        private async Task AddSyndicationItemEntry(AtomEntry<SyndicationItem<Parcel>> entry, SyndicationContext context, CancellationToken ct)
        {
            var addressParcelLinkExtractItems =
                context
                    .AddressParcelLinkExtract
                    .Where(x => x.ParcelId == entry.Content.Object.Id)
                    .AsEnumerable()
                    .Concat(context.AddressParcelLinkExtract.Local.Where(x => x.ParcelId == entry.Content.Object.Id))
                    .ToList();

            var itemsToRemove = new List<AddressParcelLinkExtractItem>();
            foreach (var addressParcelLinkExtractItem in addressParcelLinkExtractItems)
            {
                if (!entry.Content.Object.AddressIds.Contains(addressParcelLinkExtractItem.AddressId))
                    itemsToRemove.Add(addressParcelLinkExtractItem);
            }

            context.AddressParcelLinkExtract.RemoveRange(itemsToRemove);

            foreach (var addressId in entry.Content.Object.AddressIds)
            {
                var addressItem = addressParcelLinkExtractItems.FirstOrDefault(x => x.AddressId == addressId);
                if (addressItem == null)
                {
                    await context.AddressParcelLinkExtract.AddAsync(
                        await CreateAddressParcelLinkExtractItem(entry, addressId, context), ct);
                }
                else
                {
                    addressItem.ParcelPersistentLocalId = entry.Content.Object.Identificator.ObjectId;
                    UpdateDbaseRecordField(addressItem, record => record.adresobjid.Value = entry.Content.Object.Identificator.ObjectId);
                }
            }
        }

        private async Task<AddressParcelLinkExtractItem> CreateAddressParcelLinkExtractItem(AtomEntry<SyndicationItem<Parcel>> entry, Guid addressId, SyndicationContext context)
        {
            var address = await context.AddressLinkAddresses.FindAsync(addressId);

            var parcel = entry.Content.Object;
            var dbaseRecord = CreateDbaseRecord(parcel, address, context);
            return new AddressParcelLinkExtractItem
            {
                AddressId = addressId,
                ParcelId = entry.Content.Object.Id,
                ParcelPersistentLocalId = parcel.Identificator.ObjectId,
                DbaseRecord = dbaseRecord, //Add address info
                AddressComplete = address?.IsComplete ?? false,
                AddressPersistentLocalId = address?.PersistentLocalId,
            };
        }

        private byte[] CreateDbaseRecord(Parcel parcel, AddressLinkSyndicationItem address, SyndicationContext context)
        {
            var record = new AddressLinkDbaseRecord
            {
                objecttype = { Value = "Perceel" },
                adresobjid = { Value = string.IsNullOrEmpty(parcel.Identificator.ObjectId) ? "" : parcel.Identificator.ObjectId },
            };

            if (address != null)
            {
                if (!string.IsNullOrEmpty(address.PersistentLocalId))
                    record.adresid.Value = Convert.ToInt32(address.PersistentLocalId);

                record.voladres.Value = AddressBuildingUnitLinkProjections.CreateCompleteAddress(address, context);
            }

            return record.ToBytes(_encoding);
        }

        private void UpdateDbaseRecordField(AddressParcelLinkExtractItem item, Action<AddressLinkDbaseRecord> update)
        {
            var record = new AddressLinkDbaseRecord();
            record.FromBytes(item.DbaseRecord, _encoding);
            update(record);
            item.DbaseRecord = record.ToBytes(_encoding);
        }
    }
}
