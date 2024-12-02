namespace AddressRegistry.Api.Oslo.Address.Search
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Consumer.Read.Postal;
    using Consumer.Read.Postal.Projections;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Caching.Memory;
    using StreetName;

    public interface IPostalCache
    {
        string? GetPostalCodeByName(string name);
        bool IsPostalCodeValid(string postalCode);
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

        public string? GetPostalCodeByName(string name)
        {
            return _memoryCache.TryGetValue(CreateCacheKey(name), out var nisCode) ? (string)nisCode! : null;
        }

        public bool IsPostalCodeValid(string nisCode)
        {
            return NisCode.IsValid(nisCode) && _memoryCache.TryGetValue(CreateCacheKey(nisCode), out _);
        }

        private string CreateCacheKey(string value) => $"{CacheKeyPrefix}{value}".ToLower();

        public async Task InitializeCache()
        {
            var entry = _memoryCache.CreateEntry(CreateCacheKey("$manager"));
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(4);
            entry.PostEvictionCallbacks.Add(new PostEvictionCallbackRegistration
            {
                EvictionCallback = (_, _, _, _) => UpdateCachedMunicipalities().GetAwaiter().GetResult()
            });

            await UpdateCachedMunicipalities();
        }

        private async Task UpdateCachedMunicipalities()
        {
            await using var context = await _postalConsumerContextFactory.CreateDbContextAsync();

            var municipalities = await context.PostalLatestItems.ToListAsync();

            foreach (var postal in municipalities.Where(x => x.Status == PostalStatus.Retired))
            {
                _memoryCache.Remove(CreateCacheKey(postal.NisCode));

                if (!string.IsNullOrWhiteSpace(postal.NameDutch))
                {
                    _memoryCache.Remove(CreateCacheKey(postal.NameDutch));
                }

                if (!string.IsNullOrWhiteSpace(postal.NameFrench))
                {
                    _memoryCache.Remove(CreateCacheKey(postal.NameFrench));
                }

                if (!string.IsNullOrWhiteSpace(postal.NameEnglish))
                {
                    _memoryCache.Remove(CreateCacheKey(postal.NameEnglish));
                }

                if (!string.IsNullOrWhiteSpace(postal.NameGerman))
                {
                    _memoryCache.Remove(CreateCacheKey(postal.NameGerman));
                }
            }

            foreach (var postal in municipalities.Where(x => x.Status != PostalStatus.Retired))
            {
                _memoryCache.Set(CreateCacheKey(postal.NisCode), true);

                if (!string.IsNullOrWhiteSpace(postal.NameDutch))
                {
                    _memoryCache.Set(CreateCacheKey(postal.NameDutch), postal.NisCode);
                }

                if (!string.IsNullOrWhiteSpace(postal.NameFrench))
                {
                    _memoryCache.Set(CreateCacheKey(postal.NameFrench), postal.NisCode);
                }

                if (!string.IsNullOrWhiteSpace(postal.NameEnglish))
                {
                    _memoryCache.Set(CreateCacheKey(postal.NameEnglish), postal.NisCode);
                }

                if (!string.IsNullOrWhiteSpace(postal.NameGerman))
                {
                    _memoryCache.Set(CreateCacheKey(postal.NameGerman), postal.NisCode);
                }
            }
        }
    }
}
