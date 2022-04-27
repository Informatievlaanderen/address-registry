namespace AddressRegistry.Projections.Wfs.AddressWfs
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;

    public static class AddressWfsContextExtensions
    {
        public static async Task<AddressWfsItem> FindAndUpdateAddressDetail(
            this WfsContext context,
            int addressPersistentLocalId,
            Action<AddressWfsItem> updateFunc,
            CancellationToken ct)
        {
            var address = await context
                .AddressWfsItems
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
