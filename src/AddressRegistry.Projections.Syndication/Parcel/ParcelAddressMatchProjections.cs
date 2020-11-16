namespace AddressRegistry.Projections.Syndication.Parcel
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Syndication;

    public class ParcelAddressMatchProjections : AtomEntryProjectionHandlerModule<ParcelEvent, SyndicationItem<Parcel>, SyndicationContext>
    {
        public ParcelAddressMatchProjections()
        {
            When(ParcelEvent.ParcelWasRegistered, AddSyndicationItemEntry);
            When(ParcelEvent.ParcelWasRemoved, RemoveParcel);
            When(ParcelEvent.ParcelWasRecovered, AddSyndicationItemEntry);
            When(ParcelEvent.ParcelWasRealized, AddSyndicationItemEntry);
            When(ParcelEvent.ParcelWasCorrectedToRealized, AddSyndicationItemEntry);
            When(ParcelEvent.ParcelWasRetired, AddSyndicationItemEntry);
            When(ParcelEvent.ParcelWasCorrectedToRetired, AddSyndicationItemEntry);
            When(ParcelEvent.ParcelAddressWasAttached, AddSyndicationItemEntry);
            When(ParcelEvent.ParcelAddressWasDetached, AddSyndicationItemEntry);
        }

        private static async Task RemoveParcel(AtomEntry<SyndicationItem<Parcel>> entry, SyndicationContext context, CancellationToken ct)
        {
            var parcelAddressMatchLatestItems =
                context
                    .ParcelAddressMatchLatestItems
                    .Where(x => x.ParcelId == entry.Content.Object.Id)
                    .ToList()
                    .Concat(context.ParcelAddressMatchLatestItems.Local.Where(x => x.ParcelId == entry.Content.Object.Id));

            foreach (var parcelAddressMatchLatestItem in parcelAddressMatchLatestItems)
                parcelAddressMatchLatestItem.IsRemoved = true;
        }

        private static async Task AddSyndicationItemEntry(AtomEntry<SyndicationItem<Parcel>> entry, SyndicationContext context, CancellationToken ct)
        {
            var parcelAddressMatchLatestItems =
                context
                    .ParcelAddressMatchLatestItems
                    .Where(x => x.ParcelId == entry.Content.Object.Id)
                    .ToList()
                    .Concat(context.ParcelAddressMatchLatestItems.Local.Where(x => x.ParcelId == entry.Content.Object.Id))
                    .ToList();

            var itemsToRemove = new List<ParcelAddressMatchLatestItem>();
            foreach (var parcelAddressMatchLatestItem in parcelAddressMatchLatestItems)
            {
                if (!entry.Content.Object.AddressIds.Contains(parcelAddressMatchLatestItem.AddressId))
                    itemsToRemove.Add(parcelAddressMatchLatestItem);
            }

            foreach (var parcelAddressMatchLatestItem in itemsToRemove)
                parcelAddressMatchLatestItem.IsRemoved = true;

            foreach (var addressId in entry.Content.Object.AddressIds)
            {
                if (parcelAddressMatchLatestItems.All(x => x.AddressId != addressId))
                    await context.ParcelAddressMatchLatestItems.AddAsync(new ParcelAddressMatchLatestItem
                    {
                        ParcelId = entry.Content.Object.Id,
                        AddressId = addressId,
                        ParcelPersistentLocalId = entry.Content.Object.Identificator.ObjectId
                    }, ct);
            }
        }
    }
}
