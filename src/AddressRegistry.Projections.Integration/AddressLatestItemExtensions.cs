namespace AddressRegistry.Projections.Integration
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;

    public static class AddressLatestItemExtensions
    {
            public static async Task<AddressLatestItem> FindAndUpdateAddressLatestItem(this IntegrationContext context,
                int persistentLocalId,
                Action<AddressLatestItem> updateFunc,
                CancellationToken ct)
            {
                var addressLatestItem = await context
                    .AddressLatestItems
                    .FindAsync(persistentLocalId, cancellationToken: ct);

                if (addressLatestItem == null)
                    throw DatabaseItemNotFound(new PersistentLocalId(persistentLocalId));

                updateFunc(addressLatestItem);

                return addressLatestItem;
            }

            private static ProjectionItemNotFoundException<AddressLatestItemProjections> DatabaseItemNotFound(PersistentLocalId persistentLocalId)
                => new ProjectionItemNotFoundException<AddressLatestItemProjections>(persistentLocalId);
    }
}
