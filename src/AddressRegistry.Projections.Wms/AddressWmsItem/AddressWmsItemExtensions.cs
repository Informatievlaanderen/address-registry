// ReSharper disable CompareOfFloatsByEqualityOperator

namespace AddressRegistry.Projections.Wms.AddressWmsItem
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Microsoft.EntityFrameworkCore;

    public static class AddressWmsItemExtensions
    {
        public static async Task FindAndUpdateAddressDetail(this WmsContext context,
            int addressPersistentLocalId,
            Action<AddressWmsItem> updateAddressAction,
            CancellationToken ct,
            bool updateHouseNumberLabelsBeforeAddressUpdate = false,
            bool updateHouseNumberLabelsAfterAddressUpdate = false,
            bool allowUpdateRemovedAddress = false)
        {
            var address = await context.FindAddressDetail(addressPersistentLocalId, ct, allowUpdateRemovedAddress);

            if (updateHouseNumberLabelsBeforeAddressUpdate)
            {
                await context.UpdateHouseNumberLabels(address, ct);
            }

            updateAddressAction(address);

            if (updateHouseNumberLabelsAfterAddressUpdate)
            {
                await context.UpdateHouseNumberLabels(address, ct, includeAddressInUpdate: true);
            }
        }

        private static async Task<AddressWmsItem> FindAddressDetail(
            this WmsContext context,
            int addressPersistentLocalId,
            CancellationToken ct,
            bool allowRemovedAddress = false)
        {
            // NOTE: We cannot depend on SQL computed columns when facing with bulk insert that needs to perform queries.
            var address = await context.AddressWmsItems.FindAsync(addressPersistentLocalId, cancellationToken: ct);

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

        public static async Task UpdateHouseNumberLabels(this WmsContext context,
            AddressWmsItem address,
            CancellationToken ct,
            bool includeAddressInUpdate = false)
        {
            var addressesWithSharedPosition = await context.FindAddressesWithSharedPosition(
                address.AddressPersistentLocalId,
                address.Position,
                address.Status,
                ct);

            if (includeAddressInUpdate)
            {
                addressesWithSharedPosition.Add(address);
            }

            var label = addressesWithSharedPosition.CalculateHouseNumberLabel();

            foreach (var item in addressesWithSharedPosition)
            {
                item.SetHouseNumberLabel(label);
            }
        }

        private static async Task<IList<AddressWmsItem>> FindAddressesWithSharedPosition(
            this WmsContext context,
            int addressPersistentLocalId,
            NetTopologySuite.Geometries.Point position,
            string? status,
            CancellationToken ct)
        {
            return context
                .AddressWmsItems
                .Local
                .Where(i => i.PositionX == position.X
                    && i.PositionY == position.Y
                    && i.AddressPersistentLocalId != addressPersistentLocalId
                    && i.Status == status 
                    && !i.Removed)                              
                .Union(await context.AddressWmsItems 
                    .Where(i => i.PositionX == position.X
                        && i.PositionY == position.Y
                        && i.AddressPersistentLocalId != addressPersistentLocalId
                        && i.Status == status
                        && !i.Removed)
                    .ToListAsync(ct))
                .ToList();
        }

        private static string? CalculateHouseNumberLabel(this IEnumerable<AddressWmsItem> addresses)
        {
            var houseNumberComparer = new HouseNumberComparer();

            var orderedAddresses = addresses
                .Where(i => !i.Removed)
                .OrderBy(i => i.HouseNumber, houseNumberComparer)
                .ToList();

            if (!orderedAddresses.Any())
            {
                return null;
            }

            var smallestNumber = orderedAddresses.First().HouseNumber;
            var highestNumber = orderedAddresses.Last().HouseNumber;

            return smallestNumber != highestNumber
                ? $"{smallestNumber}-{highestNumber}"
                : smallestNumber;
        }

        private static ProjectionItemNotFoundException<AddressWmsItemProjections> DatabaseItemNotFound(int addressPersistentLocalId) =>
            new ProjectionItemNotFoundException<AddressWmsItemProjections>(addressPersistentLocalId.ToString());
    }
}
