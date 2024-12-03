namespace AddressRegistry.Api.Oslo.Address.Search
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Consumer.Read.Postal;
    using Consumer.Read.Postal.Projections;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Caching.Memory;

    public interface IPostalCache
    {
        string? GetNisCodeByPostalCode(string postalCode);
    }

    public class PostalCache: IPostalCache
    {
        private const string CacheKeyPrefix = "postal_";

        private readonly IDbContextFactory<PostalConsumerContext> _postalConsumerContextFactory;
        private readonly IMemoryCache _memoryCache;

        public PostalCache(
            IDbContextFactory<PostalConsumerContext> postalConsumerContextFactory,
            IMemoryCache memoryCache)
        {
            _postalConsumerContextFactory = postalConsumerContextFactory;
            _memoryCache = memoryCache;
        }

        public string? GetNisCodeByPostalCode(string postalCode)
        {
            return _memoryCache.TryGetValue(CreateCacheKey(postalCode), out var nisCode) ? (string)nisCode! : null;
        }

        private string CreateCacheKey(string value) => $"{CacheKeyPrefix}{value}".ToLower();

        public async Task InitializeCache()
        {
            var entry = _memoryCache.CreateEntry(CreateCacheKey("$manager"));
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(4);
            entry.PostEvictionCallbacks.Add(new PostEvictionCallbackRegistration
            {
                EvictionCallback = (_, _, _, _) => UpdateCachedPostals().GetAwaiter().GetResult()
            });

            await UpdateCachedPostals();
        }

        private async Task UpdateCachedPostals()
        {
            await using var context = await _postalConsumerContextFactory.CreateDbContextAsync();

            var postals = await context.PostalLatestItems.ToListAsync();

            foreach (var postal in postals.Where(x => x.Status == PostalStatus.Retired))
            {
                _memoryCache.Remove(CreateCacheKey(postal.PostalCode));
            }

            foreach (var postal in postals.Where(x => x.Status != PostalStatus.Retired && x.NisCode is not null))
            {
                _memoryCache.Set(CreateCacheKey(postal.PostalCode), postal.NisCode);
            }
        }
    }
}
