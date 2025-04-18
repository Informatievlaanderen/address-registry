namespace AddressRegistry.Api.Oslo.AddressMatch.V2.Matching
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Consumer.Read.Municipality.Projections;
    using Consumer.Read.Postal.Projections;
    using Consumer.Read.StreetName.Projections;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Caching.Memory;
    using Projections.AddressMatch;
    using Projections.AddressMatch.AddressDetailV2WithParent;

    public interface ILatestQueries
    {
        IDictionary<string, MunicipalityLatestItem> GetAllLatestMunicipalities();
        IDictionary<int, StreetNameLatestItem> GetAllLatestStreetNamesByPersistentLocalId();
        IEnumerable<StreetNameLatestItem> GetLatestStreetNamesBy(params string[] municipalityNames);
        StreetNameLatestItem? FindLatestStreetNameById(int streetNamePersistentLocalId);
        IEnumerable<AddressDetailItemV2WithParent> GetLatestAddressesBy(int streetNamePersistentLocalId, string? houseNumber, string? boxNumber);
        IEnumerable<PostalLatestItem> GetAllPostalInfo();
    }

    public sealed class CachedLatestQueriesDecorator : CachedService, ILatestQueries
    {
        private static readonly object StreetNameCacheLock = new object();
        private static readonly object MunicipalityCacheLock = new object();
        private static readonly object PostalCacheLock = new object();

        private readonly AddressMatchContextV2 _context;
        private readonly AddressMatchContext _addressMatchContext;
        private static readonly TimeSpan AllStreetNamesCacheDuration = TimeSpan.FromDays(1);
        private static readonly TimeSpan AllMunicipalitiesCacheDuration = TimeSpan.FromDays(1).Add(TimeSpan.FromHours(2));
        private static readonly TimeSpan AllPostalInfoCacheDuration = TimeSpan.FromDays(1).Add(TimeSpan.FromHours(1));
        private const string AllMunicipalitiesCacheKey = "GetAllLatestMunicipalities";
        private const string AllStreetNamesCacheKey = "GetAllLatestStreetNames";
        private const string AllPostalInfoCacheKey = "GetAllLatestPostalInfo";

        public CachedLatestQueriesDecorator(
            IMemoryCache memoryCache,
            AddressMatchContextV2 context,
            AddressMatchContext addressMatchContext)
            : base(memoryCache)
        {
            _context = context;
            _addressMatchContext = addressMatchContext;
        }

        public IDictionary<string, MunicipalityLatestItem> GetAllLatestMunicipalities() =>
            GetOrAdd(
                AllMunicipalitiesCacheKey,
                () => _context.MunicipalityLatestItems.ToDictionary(x => x.NisCode, x => x),
                AllMunicipalitiesCacheDuration,
                MunicipalityCacheLock)!;

        public IDictionary<int, StreetNameLatestItem> GetAllLatestStreetNamesByPersistentLocalId() =>
            GetOrAdd(
                    AllStreetNamesCacheKey,
                    () => new StreetNameCache(GetLatestStreetNameItems().ToList()),
                    AllStreetNamesCacheDuration,
                    StreetNameCacheLock)!
                .ByPersistentLocalId;

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

        public IEnumerable<AddressDetailItemV2WithParent> GetLatestAddressesBy(int streetNamePersistentLocalId, string? houseNumber, string? boxNumber)
        {
            var streetName = FindLatestStreetNameById(streetNamePersistentLocalId);

            // no caching for addresses
            var query = _addressMatchContext
                .AddressDetailV2WithParent
                .Where(x => !x.Removed)
                .Where(x => x.StreetNamePersistentLocalId == streetName.PersistentLocalId);

            if (!string.IsNullOrEmpty(houseNumber))
                query = query.Where(x => x.HouseNumber != null && (x.HouseNumber == houseNumber || EF.Functions.Like(x.HouseNumber, houseNumber + "[^0-9]%")));

            if (!string.IsNullOrEmpty(boxNumber))
                query = query.Where(x => x.BoxNumber == boxNumber);

            return query;
        }

        public IEnumerable<PostalLatestItem> GetAllPostalInfo() =>
            GetOrAdd(
                AllPostalInfoCacheKey,
                () => _context.PostalInfoLatestItems.ToList(),
                AllPostalInfoCacheDuration,
                PostalCacheLock)!;

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
                ifCacheNotHit,
                StreetNameCacheLock);
    }
}
