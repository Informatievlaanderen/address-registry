namespace AddressRegistry.Projections.Legacy.AddressListV2
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressList;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;

    public static class AddressListExtensionsV2
    {
        public static async Task<AddressListItemV2> FindAndUpdateAddressListItemV2(
            this LegacyContext context,
            int addressPersistentLocalId,
            Action<AddressListItemV2> updateFunc,
            CancellationToken ct)
        {
            var address = await context
                .AddressListV2
                .FindAsync(addressPersistentLocalId, cancellationToken: ct);

            if (address == null)
                throw DatabaseItemNotFound(addressPersistentLocalId);

            updateFunc(address);

            return address;
        }

        private static ProjectionItemNotFoundException<AddressListProjectionsV2> DatabaseItemNotFound(int addressPersistentLocalId)
            => new ProjectionItemNotFoundException<AddressListProjectionsV2>(addressPersistentLocalId.ToString());
    }
}
