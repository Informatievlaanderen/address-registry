namespace AddressRegistry.Api.Oslo.AddressMatch.V2.Matching
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AddressRegistry.Consumer.Read.Municipality.Projections;
    using AddressRegistry.Consumer.Read.StreetName.Projections;
    using AddressRegistry.Projections.Legacy.AddressDetailV2;
    using AddressRegistry.Projections.Syndication.PostalInfo;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Caching.Memory;

    public interface ILatestQueries
    {
        IDictionary<string, MunicipalityLatestItem> GetAllLatestMunicipalities();
        IEnumerable<StreetNameLatestItem> GetAllLatestStreetNames();
        IDictionary<int, StreetNameLatestItem> GetAllLatestStreetNamesByPersistentLocalId();
        IEnumerable<StreetNameLatestItem> GetLatestStreetNamesBy(params string[] municipalityNames);
        StreetNameLatestItem? FindLatestStreetNameById(int streetNamePersistentLocalId);
        IEnumerable<AddressDetailItemV2> GetLatestAddressesBy(int streetNamePersistentLocalId, string? houseNumber, string? boxNumber);
        IEnumerable<PostalInfoLatestItem> GetAllPostalInfo();
    }

    public sealed class CachedLatestQueriesDecorator : CachedService, ILatestQueries
    {
        private readonly AddressMatchContextV2 _context;
        private static readonly TimeSpan AllStreetNamesCacheDuration = TimeSpan.FromDays(1);
        private static readonly TimeSpan AllMunicipalitiesCacheDuration = TimeSpan.FromDays(1);
        private static readonly TimeSpan AllPostalInfoCacheDuration = TimeSpan.FromDays(1);
        private const string AllMunicipalitiesCacheKey = "GetAllLatestMunicipalities";
        private const string AllStreetNamesCacheKey = "GetAllLatestStreetNames";
        private const string AllPostalInfoCacheKey = "GetAllLatestPostalInfo";

        public CachedLatestQueriesDecorator(
            IMemoryCache memoryCache,
            AddressMatchContextV2 context)
            : base(memoryCache) => _context = context;

        public IDictionary<string, MunicipalityLatestItem> GetAllLatestMunicipalities() =>
            GetOrAdd(
                AllMunicipalitiesCacheKey,
                () => _context.MunicipalityLatestItems.ToDictionary(x => x.NisCode, x => x),
                AllMunicipalitiesCacheDuration);

        public IDictionary<int, StreetNameLatestItem> GetAllLatestStreetNamesByPersistentLocalId() =>
            GetOrAdd(
                    AllStreetNamesCacheKey,
                    () => new StreetNameCache(GetLatestStreetNameItems().ToList()),
                    AllStreetNamesCacheDuration)
                .ByPersistentLocalId;

        public IEnumerable<StreetNameLatestItem> GetAllLatestStreetNames() =>
            GetAllLatestStreetNamesByPersistentLocalId().Values;

        public IEnumerable<StreetNameLatestItem> GetLatestStreetNamesBy(params string[] municipalityNames)
        {
            var lowerMunicipalityNames = municipalityNames
                .Select(x => x.RemoveDiacritics())
                .ToList();

            var nisCodes = GetAllLatestMunicipalities()
                .Where(x =>
                    lowerMunicipalityNames.Contains(x.Value.NameDutchSearch) || // English/German probably not needed as AddressMatch is relevant to flanders
                    lowerMunicipalityNames.Contains(x.Value.NameFrenchSearch))
                .Select(x => x.Key);

            return GetOrAddLatestStreetNames(
                streetNames => nisCodes
                    .SelectMany(g => streetNames.ByMunicipalityNisCode.TryGetValue(g, out var value)
                        ? value
                        : Array.Empty<StreetNameLatestItem>())
                    .AsQueryable(),
                () => GetLatestStreetNameItems()
                    .Where(x => nisCodes.Contains(x.NisCode))
                    .ToList()
                    .AsEnumerable());
        }

        public StreetNameLatestItem? FindLatestStreetNameById(int streetNamePersistentLocalId) =>
            GetOrAddLatestStreetNames(
                streetNames => streetNames
                    .ByPersistentLocalId[streetNamePersistentLocalId],
                () => GetLatestStreetNameItems().FirstOrDefault(
                    x => x.PersistentLocalId == streetNamePersistentLocalId));

        public IEnumerable<AddressDetailItemV2> GetLatestAddressesBy(int streetNamePersistentLocalId, string? houseNumber, string? boxNumber)
        {
            var streetName = FindLatestStreetNameById(streetNamePersistentLocalId);

            // no caching for addresses
            var query = _context
                .AddressDetailV2
                .Where(x => !x.Removed)
                .Where(x => x.StreetNamePersistentLocalId == streetName.PersistentLocalId);

            if (!string.IsNullOrEmpty(houseNumber))
                query = query.Where(x => x.HouseNumber != null && (x.HouseNumber == houseNumber || EF.Functions.Like(x.HouseNumber, houseNumber + "[^0-9]%")));

            if (!string.IsNullOrEmpty(boxNumber))
                query = query.Where(x => x.BoxNumber == boxNumber);

            return query;
        }

        public IEnumerable<PostalInfoLatestItem> GetAllPostalInfo() =>
            GetOrAdd(
                AllPostalInfoCacheKey,
                () => _context.PostalInfoLatestItems.ToList(),
                AllPostalInfoCacheDuration);

        private IQueryable<StreetNameLatestItem> GetLatestStreetNameItems()
            => _context
                .StreetNameLatestItems
                .Where(x => !x.IsRemoved);

        private T GetOrAddLatestStreetNames<T>(
            Func<StreetNameCache, T> ifCacheHit,
            Func<T> ifCacheNotHit)
            => GetOrAdd(
                AllStreetNamesCacheKey,
                () => new StreetNameCache(GetLatestStreetNameItems().ToList()),
                AllStreetNamesCacheDuration,
                ifCacheHit,
                ifCacheNotHit);
    }
}
