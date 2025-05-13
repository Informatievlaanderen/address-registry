namespace AddressRegistry.Projections.Api.AddressDetail
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;

    public static class AddressDetailExtensions
    {
        public static async Task<AddressDetailItem> FindAndUpdateAddressDetail(
            this ApiContext context,
            int addressPersistentLocalId,
            Action<AddressDetailItem> updateFunc,
            CancellationToken ct)
        {
            var address = await context
                .AddressDetails
                .FindAsync(addressPersistentLocalId, cancellationToken: ct);

            if (address == null)
                throw DatabaseItemNotFound(addressPersistentLocalId);

            updateFunc(address);

            return address;
        }

        private static ProjectionItemNotFoundException<AddressDetailProjections> DatabaseItemNotFound(int addressPersistentLocalId)
            => new ProjectionItemNotFoundException<AddressDetailProjections>(addressPersistentLocalId.ToString());
    }
}
