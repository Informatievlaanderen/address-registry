// ReSharper disable CompareOfFloatsByEqualityOperator

namespace AddressRegistry.Projections.Wms.AddressWmsItemV4
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;

    public static class AddressWmsItemV4Extensions
    {
        public static async Task FindAndUpdateAddressDetailV4(
            this WmsContext context,
            int addressPersistentLocalId,
            Action<AddressWmsItemV4> updateAddressAction,
            IHouseNumberLabelUpdater houseNumberLabelUpdater,
            bool updateHouseNumberLabelsBeforeAddressUpdate,
            bool updateHouseNumberLabelsAfterAddressUpdate,
            bool allowUpdateRemovedAddress = false,
            CancellationToken ct = default)
        {
            var address = await context.FindAddressDetailV4(addressPersistentLocalId, ct, allowUpdateRemovedAddress);

            if (updateHouseNumberLabelsBeforeAddressUpdate)
            {
                await houseNumberLabelUpdater.UpdateHouseNumberLabels(context, address, ct);
            }

            updateAddressAction(address);

            if (updateHouseNumberLabelsAfterAddressUpdate)
            {
                await houseNumberLabelUpdater.UpdateHouseNumberLabels(context, address, ct, includeAddressInUpdate: true);
            }
        }

        public static async Task<AddressWmsItemV4> FindAddressDetailV4(
            this WmsContext context,
            int addressPersistentLocalId,
            CancellationToken ct,
            bool allowRemovedAddress = false)
        {
            // NOTE: We cannot depend on SQL computed columns when facing with bulk insert that needs to perform queries.
            var address = await context.AddressWmsItemsV4.FindAsync(addressPersistentLocalId, cancellationToken: ct);

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

        private static ProjectionItemNotFoundException<AddressWmsItemV4Projections> DatabaseItemNotFound(int addressPersistentLocalId) =>
            new ProjectionItemNotFoundException<AddressWmsItemV4Projections>(addressPersistentLocalId.ToString());
    }
}
