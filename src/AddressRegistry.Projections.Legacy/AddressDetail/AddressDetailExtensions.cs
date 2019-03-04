namespace AddressRegistry.Projections.Legacy.AddressDetail
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public static class AddressDetailExtensions
    {
        public static async Task<AddressDetailItem> FindAndUpdateAddressDetail(
            this LegacyContext context,
            Guid addressId,
            Action<AddressDetailItem> updateFunc,
            CancellationToken ct)
        {
            var address = await context
                .AddressDetail
                .FindAsync(addressId, cancellationToken: ct);

            if (address == null)
                throw DatabaseItemNotFound(addressId);

            updateFunc(address);

            return address;
        }

        private static ProjectionItemNotFoundException<AddressDetailProjections> DatabaseItemNotFound(Guid addressId)
            => new ProjectionItemNotFoundException<AddressDetailProjections>(addressId.ToString("D"));
    }
}
