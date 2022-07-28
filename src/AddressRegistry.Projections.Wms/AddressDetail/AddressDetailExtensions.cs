namespace AddressRegistry.Projections.Wms.AddressDetail
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public static class AddressDetailExtensions
    {
        private static async Task<AddressDetailItem> FindAddressDetail(
            this WmsContext context,
            Guid addressId,
            CancellationToken ct,
            bool allowRemovedAddress = false)
        {
            // NOTE: We cannot depend on SQL computed columns when facing with bulk insert that needs to perform queries.
            var address = await context.AddressDetail.FindAsync(addressId, cancellationToken: ct);

            if (address == null)
            {
                throw DatabaseItemNotFound(addressId);
            }

            // exclude soft deleted entries, unless allowed
            if (!address.Removed || allowRemovedAddress)
            {
                return address;
            }

            throw DatabaseItemNotFound(addressId);
        }

        public static async Task<AddressDetailItem> FindAndUpdateAddressDetail(
            this WmsContext context,
            Guid addressId,
            Action<AddressDetailItem> updateFunc,
            CancellationToken ct,
            bool allowUpdateRemovedAddress = false)
        {
            var address = await context.FindAddressDetail(addressId, ct, allowUpdateRemovedAddress);
            updateFunc(address);
            return address;
        }

        private static ProjectionItemNotFoundException<AddressDetailProjections> DatabaseItemNotFound(Guid addressId) =>
            new ProjectionItemNotFoundException<AddressDetailProjections>(addressId.ToString("D"));

    }
}
