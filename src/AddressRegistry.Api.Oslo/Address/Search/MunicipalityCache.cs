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

                string?[] nameValues = [
                    municipality.NameDutch,
                    municipality.NameFrench,
                    municipality.NameEnglish,
                    municipality.NameGerman
                ];

                foreach (var name in nameValues.Where(x => !string.IsNullOrWhiteSpace(x)))
                {
                    _memoryCache.Remove(CreateCacheKey(name!));
                }
            }

            foreach (var municipality in municipalities.Where(x => x.Status != MunicipalityStatus.Retired))
            {
                _memoryCache.Set(CreateCacheKey(municipality.NisCode), true);

                string?[] nameValues = [
                    municipality.NameDutch,
                    municipality.NameFrench,
                    municipality.NameEnglish,
                    municipality.NameGerman
                ];

                foreach (var name in nameValues.Where(x => !string.IsNullOrWhiteSpace(x)))
                {
                    _memoryCache.Set(CreateCacheKey(name!), municipality.NisCode);
                }
            }
        }
    }
}
