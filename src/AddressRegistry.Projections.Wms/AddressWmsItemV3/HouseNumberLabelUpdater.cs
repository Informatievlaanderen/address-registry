﻿namespace AddressRegistry.Projections.Wms.AddressWmsItemV3
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;

    public interface IHouseNumberLabelUpdater
    {
        public Task UpdateHouseNumberLabels(
            WmsContext context,
            AddressWmsItemV3 address,
            CancellationToken ct,
            bool includeAddressInUpdate = false);
    }

    public sealed class HouseNumberLabelUpdater : IHouseNumberLabelUpdater
    {
        public async Task UpdateHouseNumberLabels(
            WmsContext context,
            AddressWmsItemV3 address,
            CancellationToken ct,
            bool includeAddressInUpdate = false)
        {
            var addressesWithSharedPosition = await FindAddressesWithSharedPositionAndTheirBoxNumbers(
                context,
                address.AddressPersistentLocalId,
                address.Position,
                address.Status,
                ct);

            if (includeAddressInUpdate)
            {
                addressesWithSharedPosition.Add(address);
            }

            var hasBoxNumber = addressesWithSharedPosition.Any(x => x.ParentAddressPersistentLocalId is not null);

            var label = CalculateHouseNumberLabel(addressesWithSharedPosition);

            foreach (var item in addressesWithSharedPosition)
            {
                item.SetHouseNumberLabel(
                    label,
                    hasBoxNumber
                        ? WmsAddressLabelType.HouseNumberWithBoxNumbersOnSamePosition
                        : WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
            }
        }

        private async Task<IList<AddressWmsItemV3>> FindAddressesWithSharedPositionAndTheirBoxNumbers(
            WmsContext context,
            int addressPersistentLocalId,
            NetTopologySuite.Geometries.Point position,
            string? status,
            CancellationToken ct)
        {
            var localFilteredItems = context
                .AddressWmsItemsV3
                .Local
                .Where(i => i.PositionX == position.X
                            && i.PositionY == position.Y
                            && i.AddressPersistentLocalId != addressPersistentLocalId
                            && i.Status == status
                            && !i.Removed)
                .ToList();

            var localItems = context.AddressWmsItemsV3.Local.ToList();

            var dbItems = await context.AddressWmsItemsV3
                .Where(i => i.PositionX == position.X
                            && i.PositionY == position.Y
                            && i.AddressPersistentLocalId != addressPersistentLocalId
                            && i.Status == status
                            && !i.Removed)
                .ToListAsync(ct);

            // We need to verify that the AddressWmsItemV3 retrieved from the DB is not already in the local cache.
            // If it is already in the local cache, but not in the localItems list, then its position or status was updated and is no longer shared.
            // An example:
            //  context.AddressWmsItemsV3.Local (local cache) contains items: A, B, C and D
            //  localItems returns: A, B and C
            //  dbItems returns: A and D
            // This implies that D was updated but not yet persisted to the database, but was updated in memory only.
            // Because localItems didn't return D, its position (or status) didn't match the specified position.
            var verifiedDbItems = new List<AddressWmsItemV3>();
            foreach (var dbItem in dbItems)
            {
                if (localFilteredItems.Any(x => x.AddressPersistentLocalId == dbItem.AddressPersistentLocalId))
                {
                    continue;
                }

                if (localItems.Any(x =>
                        x.AddressPersistentLocalId == dbItem.AddressPersistentLocalId))
                {
                    continue;
                }

                verifiedDbItems.Add(dbItem);
            }

            var union = localFilteredItems
                .Union(verifiedDbItems)
                .ToList();

            return union;
        }

        private static string? CalculateHouseNumberLabel(IEnumerable<AddressWmsItemV3> addresses)
        {
            var houseNumberComparer = new HouseNumberComparer();

            var groupedOrderedAddresses = addresses
                .Where(i => !i.Removed)
                .OrderBy(x => x.HouseNumber, houseNumberComparer)
                .GroupBy(x => x.StreetNamePersistentLocalId)
                .ToDictionary(x => x, y => y.ToList());

            if (groupedOrderedAddresses.Count == 0)
            {
                return null;
            }

            var stringBuilder = new StringBuilder();
            foreach (var (_, addressesByStreetName) in groupedOrderedAddresses)
            {
                var orderedAddresses = addressesByStreetName
                    .OrderBy(x => x.HouseNumber, houseNumberComparer)
                    .ToList();

                var smallestNumber = orderedAddresses.First().HouseNumber;
                var highestNumber = orderedAddresses.Last().HouseNumber;

                stringBuilder.Append(smallestNumber != highestNumber
                    ? $"{smallestNumber}-{highestNumber}"
                    : smallestNumber);

                stringBuilder.Append(" ; ");
            }

            return stringBuilder.ToString().TrimEnd().TrimEnd(';').TrimEnd(); // remove trailing space and semicolon
        }
    }
}
