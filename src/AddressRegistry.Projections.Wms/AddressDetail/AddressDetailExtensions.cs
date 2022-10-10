// ReSharper disable CompareOfFloatsByEqualityOperator

namespace AddressRegistry.Projections.Wms.AddressDetail
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Microsoft.EntityFrameworkCore;
    using StreetName;

    public static class AddressDetailExtensions
    {
        public static async Task FindAndUpdateAddressDetail(this WmsContext context,
            Guid addressId,
            Action<AddressDetailItem> updateAddressAction,
            CancellationToken ct,
            bool updateHouseNumberLabelsBeforeAddressUpdate = false,
            bool updateHouseNumberLabelsAfterAddressUpdate = false,
            bool allowUpdateRemovedAddress = false)
        {
            var address = await context.FindAddressDetail(addressId, ct, allowUpdateRemovedAddress);

            if (updateHouseNumberLabelsBeforeAddressUpdate)
            {
                await context.UpdateHouseNumberLabels(address, ct);
            }

            updateAddressAction(address);

            if (updateHouseNumberLabelsAfterAddressUpdate && address.Position is not null)
            {
                await context.UpdateHouseNumberLabels(address, ct, includeAddressInUpdate: true);
            }
        }

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

        private static async Task UpdateHouseNumberLabels(this WmsContext context,
            AddressDetailItem address,
            CancellationToken ct,
            bool includeAddressInUpdate = false)
        {
            if (address.Position is null)
            {
                return;
            }

            var addressesWithSharedPosition = await context.FindAddressesWithSharedPosition(
                address.AddressId,
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

        private static async Task<IList<AddressDetailItem>> FindAddressesWithSharedPosition(
            this WmsContext context,
            Guid addressId,
            NetTopologySuite.Geometries.Point position,
            string? status,
            CancellationToken ct)
        {
            return context
                .AddressDetail
                .Local
                .Where(i =>
                    i.PositionX == position.X
                        && i.PositionY == position.Y
                        && i.AddressId != addressId
                        && i.Status == status
                        && !i.Removed
                        && i.Complete)
                .Union(await context.AddressDetail
                    .Where(i => i.PositionX == position.X
                        && i.PositionY == position.Y
                        && i.AddressId != addressId
                        && i.Status == status
                        && !i.Removed
                        && i.Complete)
                    .ToListAsync(ct))
                .ToList();
        }

        private static string? CalculateHouseNumberLabel(this IEnumerable<AddressDetailItem> addresses)
        {
            var houseNumberComparer = new HouseNumberComparer();

            var orderedAddresses = addresses
                .Where(i => !string.IsNullOrWhiteSpace(i.HouseNumber) && !i.Removed)
                .OrderBy(i => i.HouseNumber, houseNumberComparer!)
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

        private static ProjectionItemNotFoundException<AddressDetailProjections> DatabaseItemNotFound(Guid addressId) =>
            new ProjectionItemNotFoundException<AddressDetailProjections>(addressId.ToString("D"));
    }

    public class HouseNumberComparer : IComparer<string>
    {
        public int Compare(string? x, string? y)
        {
            if (ReferenceEquals(x, null) && ReferenceEquals(y, null))
                return 0;
            if (ReferenceEquals(x, null))
                return -1;
            if (ReferenceEquals(y, null))
                return 1;

            var digitsOfX = int.Parse(HouseNumber.HouseNumberDigits.Match(x).Value);
            var digitsOfY = int.Parse(HouseNumber.HouseNumberDigits.Match(y).Value);

            if (digitsOfX != digitsOfY) return digitsOfX.CompareTo(digitsOfY);

            var lettersOfX = HouseNumber.HouseNumberLetters.Match(x).Value;
            var lettersOfY = HouseNumber.HouseNumberLetters.Match(y).Value;

            return string.Compare(lettersOfX, lettersOfY, StringComparison.Ordinal);
        }
    }
}
