namespace AddressRegistry.Projections.Wms.AddressDetail
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Internal;

    public static class AddressDetailExtensions
    {
        private static async Task<AddressDetailItem> FindAddressDetail(
            this WmsContext context,
            Guid addressId,
            CancellationToken ct)
        {
            var address = await context
                .Get<AddressDetailItem>()
                .FindAsync(addressId, cancellationToken: ct);

            if (address == null)
                throw DatabaseItemNotFound(addressId);

            return address;
        }

        public static async Task<AddressDetailItem> FindAndUpdateAddressDetail(
            this WmsContext context,
            Guid addressId,
            Action<AddressDetailItem> updateFunc,
            CancellationToken ct)
        {
            var address = await context.FindAddressDetail(addressId, ct);
            updateFunc(address);
            return address;
        }

        public static async
            Task<
                (AddressDetailItem address,
                IList<AddressDetailItem> previousSharedPositionAddresses,
                IList<AddressDetailItem> newSharedPositionAddresses
                )> FindAndUpdatePreviousAndNewPositionAddresses(
                this WmsContext context,
                Guid addressId,
                NetTopologySuite.Geometries.Point? newPosition,
                Action<AddressDetailItem, string?> updateFuncAddressDetail,
                Action<IList<AddressDetailItem>, string?>? updateFuncPreviousPositionAddresses,
                Action<IList<AddressDetailItem>, string?>? updateFuncNewSharedAddressDetails,
                CancellationToken ct)
        {
            var address = await context.FindAddressDetail(addressId, ct);

            // Find common addresses with previous position and invoke update
            var previousPosition = address.Position;
            var previousSharedPositionAddresses =
                await context.FindSharedPositionAddresses(addressId, previousPosition, ct);
            var previousPositionHouseNumberLabel = previousSharedPositionAddresses.GetHouseNumberLabel();
            updateFuncPreviousPositionAddresses?.Invoke(previousSharedPositionAddresses, previousPositionHouseNumberLabel);

            // Find common addresses with new position and invoke update
            var newSharedPositionAddresses =
                await context.FindSharedPositionAddresses(addressId, newPosition, ct);

            var addresses = newSharedPositionAddresses;
            addresses.Add(address);
            var newPositionHouseNumberLabel = addresses.GetHouseNumberLabel();
            updateFuncNewSharedAddressDetails?.Invoke(newSharedPositionAddresses, newPositionHouseNumberLabel);

            //Update address with matching addressId
            updateFuncAddressDetail(address, newPositionHouseNumberLabel);

            return (address, previousSharedPositionAddresses, newSharedPositionAddresses);
        }

        public static async Task<IList<AddressDetailItem>> FindAndUpdateAddressesBySharedPosition(
                this WmsContext context,
                Guid addressId,
                Action<IList<AddressDetailItem>, string?> updateFuncAddresses,
                CancellationToken ct)
        {
            var address = await context.FindAddressDetail(addressId, ct);
            var addresses = await context.FindSharedPositionAddresses(addressId, address.Position, ct);
            addresses.Add(address);
            var houseNumberLabel = addresses.GetHouseNumberLabel();
            updateFuncAddresses(addresses, houseNumberLabel);
            return addresses;
        }

        private static async Task<IList<AddressDetailItem>> FindSharedPositionAddresses(
            this WmsContext context,
            Guid addressId,
            NetTopologySuite.Geometries.Point? position,
            CancellationToken ct)
        {
            var empty = new List<AddressDetailItem>();

            if (position == null)
                return empty;

            return await context
                .AddressDetail
                .Where(i =>
                    i.Position!.Distance(position) < 1  &&
                    i.AddressId != addressId &&
                    i.Removed == false)
                .ToListAsync(ct);
        }

        private static string? GetHouseNumberLabel(this IList<AddressDetailItem>? addresses)
        {
            if (addresses == null)
                return null;

            var orderedAddresses = addresses
                .Where(i => !string.IsNullOrWhiteSpace(i.HouseNumber) && i.Removed == false)
                .OrderBy(i => i.HouseNumber)
                .ToList();

            var smallestNumber = orderedAddresses.FirstOrDefault()?.HouseNumber;
            var highestNumber = orderedAddresses.LastOrDefault()?.HouseNumber;

            var houseNumber = smallestNumber;
            if (string.IsNullOrEmpty(highestNumber) && smallestNumber != highestNumber)
                houseNumber = $"{smallestNumber}-{highestNumber}";

            return houseNumber;
        }

        private static ProjectionItemNotFoundException<AddressDetailProjections> DatabaseItemNotFound(Guid addressId)
            => new ProjectionItemNotFoundException<AddressDetailProjections>(addressId.ToString("D"));
    }
}
