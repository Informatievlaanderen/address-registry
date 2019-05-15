namespace AddressRegistry.Projections.Syndication.Parcel
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Syndication;

    public class ParcelAddressMatchProjections : AtomEntryProjectionHandlerModule<ParcelEvent, Parcel, SyndicationContext>
    {
        public ParcelAddressMatchProjections()
        {
            When(ParcelEvent.ParcelWasRegistered, AddSyndicationItemEntry);
            When(ParcelEvent.ParcelWasRemoved, RemoveParcel);
            When(ParcelEvent.ParcelWasRealized, AddSyndicationItemEntry);
            When(ParcelEvent.ParcelWasCorrectedToRealized, AddSyndicationItemEntry);
            When(ParcelEvent.ParcelWasRetired, AddSyndicationItemEntry);
            When(ParcelEvent.ParcelWasCorrectedToRetired, AddSyndicationItemEntry);
            When(ParcelEvent.ParcelAddressWasAttached, AddSyndicationItemEntry);
            When(ParcelEvent.ParcelAddressWasDettached, AddSyndicationItemEntry);
        }

        private static async Task RemoveParcel(AtomEntry<Parcel> entry, SyndicationContext context, CancellationToken ct)
        {
            var parcelAddressMatchLatestItems =
                context
                    .ParcelAddressMatchLatestItems
                    .Where(x => x.ParcelId == entry.Content.Id)
                    .Concat(context.ParcelAddressMatchLatestItems.Local.Where(x => x.ParcelId == entry.Content.Id));

            context.ParcelAddressMatchLatestItems.RemoveRange(parcelAddressMatchLatestItems);
        }

        private static async Task AddSyndicationItemEntry(AtomEntry<Parcel> entry, SyndicationContext context, CancellationToken ct)
        {
            var parcelAddressMatchLatestItems =
                context
                    .ParcelAddressMatchLatestItems
                    .Where(x => x.ParcelId == entry.Content.Id)
                    .Concat(context.ParcelAddressMatchLatestItems.Local.Where(x => x.ParcelId == entry.Content.Id));

            var itemsToRemove = new List<ParcelAddressMatchLatestItem>();
            foreach (var parcelAddressMatchLatestItem in parcelAddressMatchLatestItems)
            {
                if (!entry.Content.AddressIds.Contains(parcelAddressMatchLatestItem.AddressId))
                    itemsToRemove.Add(parcelAddressMatchLatestItem);
            }

            context.ParcelAddressMatchLatestItems.RemoveRange(itemsToRemove);

            foreach (var addressId in entry.Content.AddressIds)
            {
                if (!parcelAddressMatchLatestItems.Any(x => x.AddressId == addressId))
                    await context.ParcelAddressMatchLatestItems.AddAsync(new ParcelAddressMatchLatestItem
                    {
                        ParcelId = entry.Content.Id,
                        AddressId = addressId,
                        ParcelOsloId = entry.Content.Identificator.ObjectId
                    }, ct);
            }
        }
    }
}
