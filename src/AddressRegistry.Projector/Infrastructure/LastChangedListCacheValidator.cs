namespace AddressRegistry.Projector.Infrastructure
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Projections.Legacy;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList;
    using Microsoft.EntityFrameworkCore;

    public sealed class LastChangedListCacheValidator : ICacheValidator
    {
        private readonly LegacyContext _legacyContext;
        private readonly string _projectionName;

        public LastChangedListCacheValidator(LegacyContext legacyContext, string projectionName)
        {
            _legacyContext = legacyContext;
            _projectionName = projectionName;
        }

        public async Task<bool> CanCache(long position, CancellationToken ct)
        {
            var projectionPosition = await _legacyContext
                .ProjectionStates
                .AsNoTracking()
                .Where(ps => ps.Name == _projectionName)
                .Select(ps => ps.Position)
                .FirstOrDefaultAsync(ct);

            return projectionPosition >= position;
        }
    }
}
