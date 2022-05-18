namespace AddressRegistry.Projections.Legacy.AddressDetailV2
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;

    public static class AddressDetailExtensionsV2
    {
        public static async Task<AddressDetailItemV2> FindAndUpdateAddressDetailV2(
            this LegacyContext context,
            int addressPersistentLocalId,
            Action<AddressDetailItemV2> updateFunc,
            CancellationToken ct)
        {
            var address = await context
                .AddressDetailV2
                .FindAsync(addressPersistentLocalId, cancellationToken: ct);

            if (address == null)
                throw DatabaseItemNotFound(addressPersistentLocalId);

            updateFunc(address);

            return address;
        }

        private static ProjectionItemNotFoundException<AddressDetailProjectionsV2> DatabaseItemNotFound(int addressPersistentLocalId)
            => new ProjectionItemNotFoundException<AddressDetailProjectionsV2>(addressPersistentLocalId.ToString());
    }
}
