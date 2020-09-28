namespace AddressRegistry.Projections.Syndication.AddressLink
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Extracts;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Syndication;
    using BuildingUnit;
    using Parcel;

    public class AddressLinkSyndicationProjections : AtomEntryProjectionHandlerModule<AddressEvent, SyndicationItem<Address>, SyndicationContext>
    {
        private readonly Encoding _encoding;

        public AddressLinkSyndicationProjections(Encoding encoding)
        {
            _encoding = encoding;

            When(AddressEvent.AddressWasRegistered, AddSyndicationItemEntry);
            When(AddressEvent.AddressBecameComplete, AddSyndicationItemEntry);
            When(AddressEvent.AddressBecameIncomplete, AddSyndicationItemEntry);
            When(AddressEvent.AddressPersistentLocalIdentifierWasAssigned, AddSyndicationItemEntry);
            When(AddressEvent.AddressWasRemoved, RemoveSyndicationItemEntry);

            When(AddressEvent.AddressBoxNumberWasChanged, AddSyndicationItemEntry);
            When(AddressEvent.AddressBoxNumberWasCorrected, AddSyndicationItemEntry);
            When(AddressEvent.AddressBoxNumberWasRemoved, AddSyndicationItemEntry);

            When(AddressEvent.AddressHouseNumberWasChanged, AddSyndicationItemEntry);
            When(AddressEvent.AddressHouseNumberWasCorrected, AddSyndicationItemEntry);

            When(AddressEvent.AddressPostalCodeWasChanged, AddSyndicationItemEntry);
            When(AddressEvent.AddressPostalCodeWasCorrected, AddSyndicationItemEntry);
            When(AddressEvent.AddressPostalCodeWasRemoved, AddSyndicationItemEntry);

            When(AddressEvent.AddressStreetNameWasChanged, AddSyndicationItemEntry);
            When(AddressEvent.AddressStreetNameWasCorrected, AddSyndicationItemEntry);
        }

        private async Task AddSyndicationItemEntry(AtomEntry<SyndicationItem<Address>> entry, SyndicationContext context, CancellationToken ct)
        {
            var latestItem = await context
                .AddressLinkAddresses
                .FindAsync(entry.Content.Object.AddressId);

            if (latestItem == null)
            {
                latestItem = new AddressLinkSyndicationItem
                {
                    AddressId = entry.Content.Object.AddressId,
                    Version = entry.Content.Object.Identificator?.Versie,
                    Position = long.Parse(entry.FeedEntry.Id),
                    PersistentLocalId = entry.Content.Object.Identificator?.ObjectId,
                    IsComplete = entry.Content.Object.IsComplete,
                    BoxNumber = entry.Content.Object.BoxNumber,
                    HouseNumber = entry.Content.Object.HouseNumber,
                    PostalCode = entry.Content.Object.PostalCode,
                    StreetNameId = entry.Content.Object.SteetnameId
                };

                await context
                      .AddressLinkAddresses
                      .AddAsync(latestItem, ct);
            }
            else
            {
                latestItem.Version = entry.Content.Object.Identificator?.Versie;
                latestItem.Position = long.Parse(entry.FeedEntry.Id);
                latestItem.PersistentLocalId = entry.Content.Object.Identificator?.ObjectId;
                latestItem.IsComplete = entry.Content.Object.IsComplete;
                latestItem.BoxNumber = entry.Content.Object.BoxNumber;
                latestItem.HouseNumber = entry.Content.Object.HouseNumber;
                latestItem.PostalCode = entry.Content.Object.PostalCode;
                latestItem.StreetNameId = entry.Content.Object.SteetnameId;
            }

            var addressBuildingUnitLinkExtractItems =
                context.AddressBuildingUnitLinkExtract
                    .Where(x => x.AddressId == latestItem.AddressId)
                    .AsEnumerable()
                    .Concat(context.AddressBuildingUnitLinkExtract.Local.Where(x => x.AddressId == latestItem.AddressId));

            foreach (var addressBuildingUnitLinkExtractItem in addressBuildingUnitLinkExtractItems)
            {
                addressBuildingUnitLinkExtractItem.AddressPersistentLocalId = latestItem.PersistentLocalId;
                var completeAddress = AddressBuildingUnitLinkProjections.CreateCompleteAddress(latestItem, context);

                UpdateBuildingUnitDbaseRecordField(addressBuildingUnitLinkExtractItem, record =>
                {
                    if (!string.IsNullOrEmpty(latestItem.PersistentLocalId))
                        record.adresid.Value = Convert.ToInt32(latestItem.PersistentLocalId);
                        
                    record.voladres.Value = completeAddress;
                });
            }

            var addressAddressParcelLinkExtractItems =
                context.AddressParcelLinkExtract
                    .Where(x => x.AddressId == latestItem.AddressId)
                    .AsEnumerable()
                    .Concat(context.AddressParcelLinkExtract.Local.Where(x => x.AddressId == latestItem.AddressId));

            foreach (var addressParcelLinkExtractItem in addressAddressParcelLinkExtractItems)
            {
                addressParcelLinkExtractItem.AddressPersistentLocalId = latestItem.PersistentLocalId;
                var completeAddress = AddressBuildingUnitLinkProjections.CreateCompleteAddress(latestItem, context);

                UpdateParcelDbaseRecordField(addressParcelLinkExtractItem, record =>
                {
                    if (!string.IsNullOrEmpty(latestItem.PersistentLocalId))
                        record.adresid.Value = Convert.ToInt32(latestItem.PersistentLocalId);
                        
                    record.voladres.Value = completeAddress;
                });
            }
        }

        private static async Task RemoveSyndicationItemEntry(AtomEntry<SyndicationItem<Address>> entry, SyndicationContext context, CancellationToken ct)
        {
            var latestItem =
                await context
                    .AddressLinkAddresses
                    .FindAsync(entry.Content.Object.AddressId);

            latestItem.Version = entry.Content.Object.Identificator?.Versie;
            latestItem.Position = long.Parse(entry.FeedEntry.Id);
            latestItem.PersistentLocalId = entry.Content.Object.Identificator?.ObjectId;
            latestItem.IsComplete = entry.Content.Object.IsComplete;
            latestItem.IsRemoved = true;

            context.AddressBuildingUnitLinkExtract.RemoveRange(context.AddressBuildingUnitLinkExtract.Where(x => x.AddressId == entry.Content.Object.AddressId));
            context.AddressParcelLinkExtract.RemoveRange(context.AddressParcelLinkExtract.Where(x => x.AddressId == entry.Content.Object.AddressId));
        }

        private void UpdateBuildingUnitDbaseRecordField(AddressBuildingUnitLinkExtractItem item, Action<AddressLinkDbaseRecord> update)
        {
            var record = new AddressLinkDbaseRecord();
            record.FromBytes(item.DbaseRecord, _encoding);
            update(record);
            item.DbaseRecord = record.ToBytes(_encoding);
        }

        private void UpdateParcelDbaseRecordField(AddressParcelLinkExtractItem item, Action<AddressLinkDbaseRecord> update)
        {
            var record = new AddressLinkDbaseRecord();
            record.FromBytes(item.DbaseRecord, _encoding);
            update(record);
            item.DbaseRecord = record.ToBytes(_encoding);
        }
    }
}
