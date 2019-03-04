namespace AddressRegistry.Projections.Legacy.AddressList
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;

    public static class AddressListExtensions
    {
        public static async Task<AddressListItem> FindAndUpdateAddressListItem(
            this LegacyContext context,
            Guid addressId,
            Action<AddressListItem> updateFunc,
            CancellationToken ct)
        {
            var address = await context
                .AddressList
                .FindAsync(addressId, cancellationToken: ct);

            if (address == null)
                throw DatabaseItemNotFound(addressId);

            updateFunc(address);

            return address;
        }

        private static ProjectionItemNotFoundException<AddressListProjections> DatabaseItemNotFound(Guid address)
            => new ProjectionItemNotFoundException<AddressListProjections>(address.ToString("D"));
    }
}
