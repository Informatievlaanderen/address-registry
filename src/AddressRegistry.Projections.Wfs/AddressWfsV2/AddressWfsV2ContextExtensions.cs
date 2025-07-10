namespace AddressRegistry.Projections.Wfs.AddressWfsV2
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;

    public static class AddressWfsV2Extensions
    {
        public static async Task<AddressWfsV2Item> FindAndUpdateAddressDetail(
            this WfsContext context,
            int addressPersistentLocalId,
            Action<AddressWfsV2Item> updateAddressAction,
            IHouseNumberLabelUpdater houseNumberLabelUpdater,
            bool updateHouseNumberLabelsBeforeAddressUpdate,
            bool updateHouseNumberLabelsAfterAddressUpdate,
            bool allowUpdateRemovedAddress = false,
            CancellationToken ct = default)
        {
            var address = await context.FindAddressDetail(addressPersistentLocalId, ct, allowUpdateRemovedAddress);

            if (updateHouseNumberLabelsBeforeAddressUpdate)
            {
                await houseNumberLabelUpdater.UpdateHouseNumberLabels(context, address, ct);
            }

            updateAddressAction(address);

            if (updateHouseNumberLabelsAfterAddressUpdate)
            {
                await houseNumberLabelUpdater.UpdateHouseNumberLabels(context, address, ct, includeAddressInUpdate: true);
            }

            return address;
        }

        public static async Task<AddressWfsV2Item> FindAddressDetail(
            this WfsContext context,
            int addressPersistentLocalId,
            CancellationToken ct,
            bool allowRemovedAddress = false)
        {
            // NOTE: We cannot depend on SQL computed columns when facing with bulk insert that needs to perform queries.
            var address = await context.AddressWfsV2Items.FindAsync(addressPersistentLocalId, cancellationToken: ct);

            if (address == null)
            {
                throw DatabaseItemNotFound(addressPersistentLocalId);
            }

            // exclude soft deleted entries, unless allowed
            if (!address.Removed || allowRemovedAddress)
            {
                return address;
            }

            throw DatabaseItemNotFound(addressPersistentLocalId);
        }

        private static ProjectionItemNotFoundException<AddressWfsV2Projections> DatabaseItemNotFound(int addressPersistentLocalId)
            => new ProjectionItemNotFoundException<AddressWfsV2Projections>(addressPersistentLocalId.ToString());
    }
}

