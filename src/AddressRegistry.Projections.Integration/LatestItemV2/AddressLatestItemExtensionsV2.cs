namespace AddressRegistry.Projections.Integration.LatestItemV2
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;

    public static class AddressLatestItemExtensionsV2
    {
            public static async Task<AddressLatestItemV2> FindAndUpdateAddressLatestItem(this IntegrationContext context,
                int persistentLocalId,
                long position,
                Action<AddressLatestItemV2> updateFunc,
                CancellationToken ct)
            {
                var addressLatestItem = await context
                    .AddressLatestItemsV2
                    .FindAsync(persistentLocalId, cancellationToken: ct);

                if (addressLatestItem == null)
                    throw DatabaseItemNotFound(new PersistentLocalId(persistentLocalId));

                updateFunc(addressLatestItem);

                return addressLatestItem;
            }

            private static ProjectionItemNotFoundException<AddressLatestItemProjectionsV2> DatabaseItemNotFound(PersistentLocalId persistentLocalId)
                => new ProjectionItemNotFoundException<AddressLatestItemProjectionsV2>(persistentLocalId);
    }
}
