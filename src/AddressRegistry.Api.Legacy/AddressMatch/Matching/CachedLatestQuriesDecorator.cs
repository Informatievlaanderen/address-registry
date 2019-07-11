namespace AddressRegistry.Api.Legacy.AddressMatch.Matching
{
    using Microsoft.Extensions.Caching.Memory;
    using Projections.Legacy;
    using Projections.Legacy.AddressDetail;
    using Projections.Syndication;
    using Projections.Syndication.Municipality;
    using Projections.Syndication.PostalInfo;
    using Projections.Syndication.StreetName;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public interface ILatestQueries
    {
        IEnumerable<MunicipalityLatestItem> GetAllLatestMunicipalities();
        IEnumerable<StreetNameLatestItem> GetLatestStreetNamesBy(params string[] municipalityNames);
        IEnumerable<StreetNameLatestItem> GetAllLatestStreetNames();
        IEnumerable<AddressDetailItem> GetLatestAddressesBy(string streetNamePersistentLocalId, string houseNumber, string boxNumber);
        StreetNameLatestItem FindLatestStreetNameById(string streetNamePersistentLocalId);
        IEnumerable<AddressDetailItem> FindLatestAddressesByCrabSubaddressIds(IEnumerable<int> crabSubaddressIds);
        IEnumerable<AddressDetailItem> FindLatestAddressesByCrabHouseNumberIds(IEnumerable<int> crabHouseNumberIds);
        IEnumerable<PostalInfoLatestItem> GetAllPostalInfo();
    }

    public class CachedLatestQueriesDecorator : CachedService, ILatestQueries
    {
        private readonly LegacyContext _legacyContext;
        private readonly SyndicationContext _syndicationContext;
        private static readonly TimeSpan AllStreetNamesCacheDuration = TimeSpan.FromDays(1);
        private static readonly TimeSpan AllMunicipalitiesCacheDuration = TimeSpan.FromDays(1);
        private static readonly TimeSpan AllPostalInfoCacheDuration = TimeSpan.FromDays(1);
        private const string AllStreetNamesCacheKey = "GetAllLatestStreetNames";
        private const string AllMunicipalitiesCacheKey = "GetAllLatestMunicipalities";
        private const string AllPostalInfoCacheKey = "GetAllLatestPostalInfo";

        public CachedLatestQueriesDecorator(IMemoryCache memoryCache, LegacyContext legacyContext, SyndicationContext syndicationContext)
            : base(memoryCache)
        {
            _legacyContext = legacyContext;
            _syndicationContext = syndicationContext;
        }

        public IEnumerable<PostalInfoLatestItem> GetAllPostalInfo() =>
            GetOrAdd(AllPostalInfoCacheKey,
                () => _syndicationContext.PostalInfoLatestItems.ToList(),
                AllPostalInfoCacheDuration);

        public StreetNameLatestItem FindLatestStreetNameById(string streetNamePersistentLocalId) =>
            GetOrAddLatestStreetNames(
                streetNames => streetNames.SelectMany(kvp => kvp.Value).Single(s => s.PersistentLocalId == streetNamePersistentLocalId),
                () => GetLatestStreetNameItems().FirstOrDefault(x => x.PersistentLocalId == streetNamePersistentLocalId));

        public IEnumerable<MunicipalityLatestItem> GetAllLatestMunicipalities() =>
            GetOrAdd(AllMunicipalitiesCacheKey,
                () => _syndicationContext.MunicipalityLatestItems.ToList(),
                AllMunicipalitiesCacheDuration);

        public IEnumerable<StreetNameLatestItem> GetAllLatestStreetNames() =>
            GetOrAdd(AllStreetNamesCacheKey,
                () => ConvertToCachedModel(GetLatestStreetNameItems().ToList()),
                AllStreetNamesCacheDuration).SelectMany(kvp => kvp.Value);

        private IQueryable<StreetNameLatestItem> GetLatestStreetNameItems()
        {
            var streetNameLatestItems = _syndicationContext
                .StreetNameLatestItems
                .Where(x => x.IsComplete);
            return streetNameLatestItems;
        }

        public IEnumerable<AddressDetailItem> GetLatestAddressesBy(string streetNamePersistentLocalId, string houseNumber, string boxNumber)
        {
            var streetName = FindLatestStreetNameById(streetNamePersistentLocalId);

            //no caching for addresses
            var query = _legacyContext
                .AddressDetail
                .Where(x => x.Complete && !x.Removed && x.PersistentLocalId.HasValue)
                .Where(x => x.HouseNumber == houseNumber && x.BoxNumber == boxNumber);
            if (streetName != null)
                query = query.Where(x => x.StreetNameId == streetName.StreetNameId);

            return query;
        }

        //no caching for addresses
        public IEnumerable<AddressDetailItem> FindLatestAddressesByCrabSubaddressIds(IEnumerable<int> crabSubaddressIds) =>
            _legacyContext.AddressDetail
                .Where(x => x.Complete && !x.Removed && x.PersistentLocalId.HasValue)
                .Where(detailItem =>
                    _legacyContext.CrabIdToPersistentLocalIds
                        .Where(id => crabSubaddressIds.Contains(id.SubaddressId.Value))
                        .Select(y => y.AddressId)
                        .Contains(detailItem.AddressId));

        //no caching for addresses
        public IEnumerable<AddressDetailItem> FindLatestAddressesByCrabHouseNumberIds(IEnumerable<int> crabHouseNumberIds) =>
            _legacyContext.AddressDetail
                .Where(x => x.Complete && !x.Removed && x.PersistentLocalId.HasValue)
                .Where(detailItem =>
                    _legacyContext.CrabIdToPersistentLocalIds
                        .Where(id => crabHouseNumberIds.Contains(id.HouseNumberId.Value))
                        .Select(y => y.AddressId)
                        .Contains(detailItem.AddressId));

        public IEnumerable<StreetNameLatestItem> GetLatestStreetNamesBy(params string[] municipalityNames)
        {
            var lowerMunicipalityNames = municipalityNames.Select(x => x.ToLowerInvariant()).ToList();
            var nisCodes = GetAllLatestMunicipalities()
                .Where(x =>
                    lowerMunicipalityNames.Contains(x.NameDutchSearch) || // English/German probably not needed as AddressMatch is relevant to flanders
                    lowerMunicipalityNames.Contains(x.NameFrenchSearch))
                .Select(x => x.NisCode);

            return GetOrAddLatestStreetNames(
                streetNames => nisCodes
                    .SelectMany(g => streetNames.ContainsKey(g) ? streetNames[g] : new StreetNameLatestItem[] { })
                    .AsQueryable(),
                () => GetLatestStreetNameItems().Where(x => nisCodes.Contains(x.NisCode)));
        }


        private T GetOrAddLatestStreetNames<T>(Func<Dictionary<string, IEnumerable<StreetNameLatestItem>>, T> ifCacheHit, Func<T> ifCacheNotHit) =>
            GetOrAdd(AllStreetNamesCacheKey,
                () => ConvertToCachedModel(GetLatestStreetNameItems().ToList()),
                AllStreetNamesCacheDuration,
                ifCacheHit,
                ifCacheNotHit);

        private Dictionary<string, IEnumerable<StreetNameLatestItem>> ConvertToCachedModel(IEnumerable<StreetNameLatestItem> query) =>
            query.GroupBy(s => s.NisCode).ToDictionary(s => s.Key, s => s.AsEnumerable());
    }
}
