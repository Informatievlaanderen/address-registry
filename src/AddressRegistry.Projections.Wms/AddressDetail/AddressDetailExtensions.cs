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
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;

    public static class AddressDetailExtensions
    {
        private static string? STAsText(this Point point)
            => point != null ? WKTWriter.ToPoint(point.Coordinate) : null;

        private static async Task<AddressDetailItem> FindAddressDetail(
            this WmsContext context,
            Guid addressId,
            CancellationToken ct,
            bool allowRemovedAddress = false)
        {
            // NOTE: We cannot depend on SQL computed columns when facing with bulk insert that needs to perform queries.
            var address = await context.AddressDetail.FindAsync(addressId, cancellationToken: ct);

            if (address == null)
                throw DatabaseItemNotFound(addressId);

            // exclude soft deleted entries, unless allowed
            if(!address.Removed || allowRemovedAddress)
                return address;

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
            => new(addressId.ToString("D"));

    }
}
