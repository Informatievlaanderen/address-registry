// ReSharper disable CompareOfFloatsByEqualityOperator

namespace AddressRegistry.Projections.Wms.AddressWmsItemV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Microsoft.EntityFrameworkCore;

    public static class AddressWmsItemV3Extensions
    {
        public static async Task FindAndUpdateAddressDetailV3(this WmsContext context,
            int addressPersistentLocalId,
            Action<AddressWmsItemV3> updateAddressAction,
            CancellationToken ct,
            bool updateHouseNumberLabelsBeforeAddressUpdate,
            bool updateHouseNumberLabelsAfterAddressUpdate,
            bool allowUpdateRemovedAddress = false)
        {
            var address = await context.FindAddressDetailV3(addressPersistentLocalId, ct, allowUpdateRemovedAddress);

            if (updateHouseNumberLabelsBeforeAddressUpdate)
            {
                await context.UpdateHouseNumberLabelsV3(address, ct);
            }

            updateAddressAction(address);

            if (updateHouseNumberLabelsAfterAddressUpdate)
            {
                await context.UpdateHouseNumberLabelsV3(address, ct, includeAddressInUpdate: true);
            }
        }

        public static async Task<AddressWmsItemV3> FindAddressDetailV3(
            this WmsContext context,
            int addressPersistentLocalId,
            CancellationToken ct,
            bool allowRemovedAddress = false)
        {
            // NOTE: We cannot depend on SQL computed columns when facing with bulk insert that needs to perform queries.
            var address = await context.AddressWmsItemsV3.FindAsync(addressPersistentLocalId, cancellationToken: ct);

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

        public static async Task UpdateHouseNumberLabelsV3(this WmsContext context,
            AddressWmsItemV3 address,
            CancellationToken ct,
            bool includeAddressInUpdate = false)
        {
            var addressesWithSharedPosition = await context.FindAddressesWithSharedPositionAndTheirBoxNumbers(
                address.AddressPersistentLocalId,
                address.Position,
                address.Status,
                ct);

            if (includeAddressInUpdate)
            {
                addressesWithSharedPosition.Add(address);
            }

            var hasBoxNumber = addressesWithSharedPosition.Any(x => x.ParentAddressPersistentLocalId is not null);

            var label = addressesWithSharedPosition.CalculateHouseNumberLabel();

            foreach (var item in addressesWithSharedPosition)
            {
                item.SetHouseNumberLabel(
                    label,
                    hasBoxNumber
                        ? WmsAddressLabelType.HouseNumberWithBoxNumbersOnSamePosition
                        : WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
            }
        }

        private static async Task<IList<AddressWmsItemV3>> FindAddressesWithSharedPositionAndTheirBoxNumbers(
            this WmsContext context,
            int addressPersistentLocalId,
            NetTopologySuite.Geometries.Point position,
            string? status,
            CancellationToken ct)
        {
            //TODO-rik: should we filter on only housenumbers?
            var localFilteredItems = context
                .AddressWmsItemsV3
                .Local
                .Where(i => i.PositionX == position.X
                            && i.PositionY == position.Y
                            && i.AddressPersistentLocalId != addressPersistentLocalId
                            && i.Status == status
                            //&& i.BoxNumber == null
                            && !i.Removed)
                .ToList();

            var localItems = context.AddressWmsItemsV3.Local.ToList();

            var dbItems = await context.AddressWmsItemsV3
                .Where(i => i.PositionX == position.X
                            && i.PositionY == position.Y
                            && i.AddressPersistentLocalId != addressPersistentLocalId
                            && i.Status == status
                            //&& i.BoxNumber == null
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

            var houseNumbers = union
                .Select(x => x.AddressPersistentLocalId)
                .ToList();

            //houseNumbers.Add(addressPersistentLocalId);

            var localFilteredBoxNumberItems = context
                .AddressWmsItemsV3
                .Local
                .Where(i => i.ParentAddressPersistentLocalId != null
                            && houseNumbers.Contains(i.ParentAddressPersistentLocalId.Value)
                            && i.AddressPersistentLocalId != addressPersistentLocalId
                            && i.Status == status
                            && !i.Removed)
                .ToList();

            var localBoxNumberItems = context.AddressWmsItemsV3.Local.ToList();

            var dbBoxNumberItems = await context.AddressWmsItemsV3
                .Where(i => i.ParentAddressPersistentLocalId != null
                            && houseNumbers.Contains(i.ParentAddressPersistentLocalId.Value)
                            && i.AddressPersistentLocalId != addressPersistentLocalId
                            && i.Status == status
                            && !i.Removed)
                .ToListAsync(ct);

            var verifiedBoxNumberDbItems = new List<AddressWmsItemV3>();
            foreach (var dbItem in dbBoxNumberItems)
            {
                if (localFilteredBoxNumberItems.Any(x => x.AddressPersistentLocalId == dbItem.AddressPersistentLocalId))
                {
                    continue;
                }

                if (localBoxNumberItems.Any(x =>
                        x.AddressPersistentLocalId == dbItem.AddressPersistentLocalId))
                {
                    continue;
                }

                verifiedBoxNumberDbItems.Add(dbItem);
            }

            var boxNumbersUnion = localFilteredBoxNumberItems
                .Union(verifiedBoxNumberDbItems)
                .ToList();

            return union
                .Union(boxNumbersUnion)
                .ToList();
        }

        private static string? CalculateHouseNumberLabel(this IEnumerable<AddressWmsItemV3> addresses)
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

        private static ProjectionItemNotFoundException<AddressWmsItemV3Projections> DatabaseItemNotFound(int addressPersistentLocalId) =>
            new ProjectionItemNotFoundException<AddressWmsItemV3Projections>(addressPersistentLocalId.ToString());
    }
}
