namespace AddressRegistry.Projections.Legacy.AddressSyndication
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;

    public static class AddressSyndicationExtensions
    {
        public static async Task<AddressSyndicationItem> LatestPosition(
            this LegacyContext context,
            Guid addressId,
            CancellationToken ct)
            => context
                   .AddressSyndication
                   .Local
                   .Where(x => x.AddressId == addressId)
                   .OrderByDescending(x => x.Position)
                   .FirstOrDefault()
               ?? await context
                   .AddressSyndication
                   .Where(x => x.AddressId == addressId)
                   .OrderByDescending(x => x.Position)
                   .FirstOrDefaultAsync(ct);
    }
}
