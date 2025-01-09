namespace AddressRegistry.Projections.Wfs.AddressWfsV2
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressWfs;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;

    public static class AddressWfsV2Extensions
    {
        public static async Task<AddressWfsV2Item> FindAndUpdateAddressDetail(
            this WfsContext context,
            int addressPersistentLocalId,
            Action<AddressWfsV2Item> updateFunc,
            CancellationToken ct)
        {
            var address = await context
                .AddressWfsV2Items
                .FindAsync(addressPersistentLocalId, cancellationToken: ct);

            if (address == null)
                throw DatabaseItemNotFound(addressPersistentLocalId);

            updateFunc(address);

            return address;
        }

        private static ProjectionItemNotFoundException<AddressWfsProjections> DatabaseItemNotFound(int addressPersistentLocalId)
            => new ProjectionItemNotFoundException<AddressWfsProjections>(addressPersistentLocalId.ToString());
    }
}
