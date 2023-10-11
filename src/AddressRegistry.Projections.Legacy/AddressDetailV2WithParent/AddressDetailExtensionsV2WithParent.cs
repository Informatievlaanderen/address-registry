namespace AddressRegistry.Projections.Legacy.AddressDetailV2WithParent
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;

    public static class AddressDetailExtensionsV2WithParent
    {
        public static async Task<AddressDetailItemV2WithParent> FindAndUpdateAddressDetailV2(
            this LegacyContext context,
            int addressPersistentLocalId,
            Action<AddressDetailItemV2WithParent> updateFunc,
            CancellationToken ct)
        {
            var address = await context
                .AddressDetailV2WithParent
                .FindAsync(addressPersistentLocalId, cancellationToken: ct);

            if (address == null)
                throw DatabaseItemNotFound(addressPersistentLocalId);

            updateFunc(address);

            return address;
        }

        private static ProjectionItemNotFoundException<AddressDetailProjectionsV2WithParent> DatabaseItemNotFound(int addressPersistentLocalId)
            => new ProjectionItemNotFoundException<AddressDetailProjectionsV2WithParent>(addressPersistentLocalId.ToString());
    }
}
