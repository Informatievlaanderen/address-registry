namespace AddressRegistry.Api.Oslo.Address.Search
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Consumer.Read.Municipality;
    using Consumer.Read.Municipality.Projections;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Caching.Memory;
    using StreetName;

    public interface IMunicipalityCache
    {
        string? GetNisCodeByName(string name);
        bool NisCodeExists(string nisCode);
    }

    public class MunicipalityCache: IMunicipalityCache
    {
        private const string CacheKeyPrefix = "municipality_";

        private readonly IDbContextFactory<MunicipalityConsumerContext> _municipalityConsumerContextFactory;
        private readonly IMemoryCache _memoryCache;

        public MunicipalityCache(
            IDbContextFactory<MunicipalityConsumerContext> municipalityConsumerContextFactory,
            IMemoryCache memoryCache)
        {
            _municipalityConsumerContextFactory = municipalityConsumerContextFactory;
            _memoryCache = memoryCache;
        }

        public string? GetNisCodeByName(string name)
        {
            if (NisCode.IsValid(name))
            {
                return null;
            }

            return _memoryCache.TryGetValue(CreateCacheKey(name), out var nisCode) ? (string)nisCode! : null;
        }

        public bool NisCodeExists(string nisCode)
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
            await using var context = await _municipalityConsumerContextFactory.CreateDbContextAsync();

            var municipalities = await context.MunicipalityLatestItems.ToListAsync();

            foreach (var municipality in municipalities.Where(x => x.Status == MunicipalityStatus.Retired))
            {
                _memoryCache.Remove(CreateCacheKey(municipality.NisCode));

                if (!string.IsNullOrWhiteSpace(municipality.NameDutch))
                {
                    _memoryCache.Remove(CreateCacheKey(municipality.NameDutch));
                }

                if (!string.IsNullOrWhiteSpace(municipality.NameFrench))
                {
                    _memoryCache.Remove(CreateCacheKey(municipality.NameFrench));
                }

                if (!string.IsNullOrWhiteSpace(municipality.NameEnglish))
                {
                    _memoryCache.Remove(CreateCacheKey(municipality.NameEnglish));
                }

                if (!string.IsNullOrWhiteSpace(municipality.NameGerman))
                {
                    _memoryCache.Remove(CreateCacheKey(municipality.NameGerman));
                }
            }

            foreach (var municipality in municipalities.Where(x => x.Status != MunicipalityStatus.Retired))
            {
                _memoryCache.Set(CreateCacheKey(municipality.NisCode), true);

                if (!string.IsNullOrWhiteSpace(municipality.NameDutch))
                {
                    _memoryCache.Set(CreateCacheKey(municipality.NameDutch), municipality.NisCode);
                }

                if (!string.IsNullOrWhiteSpace(municipality.NameFrench))
                {
                    _memoryCache.Set(CreateCacheKey(municipality.NameFrench), municipality.NisCode);
                }

                if (!string.IsNullOrWhiteSpace(municipality.NameEnglish))
                {
                    _memoryCache.Set(CreateCacheKey(municipality.NameEnglish), municipality.NisCode);
                }

                if (!string.IsNullOrWhiteSpace(municipality.NameGerman))
                {
                    _memoryCache.Set(CreateCacheKey(municipality.NameGerman), municipality.NisCode);
                }
            }
        }
    }
}
