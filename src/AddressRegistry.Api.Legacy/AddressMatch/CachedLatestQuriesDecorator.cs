namespace AddressRegistry.Api.Legacy.AddressMatch
{
    using Microsoft.Extensions.Caching.Memory;
    using Projections.Legacy;
    using Projections.Legacy.AddressDetail;
    using Projections.Syndication;
    using Projections.Syndication.Municipality;
    using Projections.Syndication.StreetName;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public interface ILatestQueries
    {
        IEnumerable<MunicipalityLatestItem> GetAllLatestMunicipalities();
        IEnumerable<StreetNameLatestItem> GetLatestStreetNamesBy(params string[] municipalityNames);
        IEnumerable<StreetNameLatestItem> GetAllLatestStreetNames();
        IEnumerable<AddressDetailItem> GetLatestAddressesBy(string streetNameOsloId, string houseNumber, string boxNumber);
        StreetNameLatestItem FindLatestStreetNameById(string streetNameOsloId);
        IEnumerable<AddressDetailItem> FindLatestAddressesByCrabSubaddressIds(IEnumerable<int> crabSubaddressIds);
        IEnumerable<AddressDetailItem> FindLatestAddressesByCrabHouseNumberIds(IEnumerable<int> crabHouseNumberIds);
    }

    public class CachedLatestQueriesDecorator : CachedService, ILatestQueries
    {
        private readonly LegacyContext _legacyContext;
        private readonly SyndicationContext _syndicationContext;
        private static readonly TimeSpan AllStreetNamesCacheDuration = TimeSpan.FromDays(1);
        private static readonly TimeSpan AllMunicipalitiesCacheDuration = TimeSpan.FromDays(1);
        private const string AllStreetNamesCacheKey = "GetAllLatestStreetNames";
        private const string AllMunicipalitiesCacheKey = "GetAllLatestMunicipalities";


        public CachedLatestQueriesDecorator(IMemoryCache memoryCache, LegacyContext legacyContext, SyndicationContext syndicationContext)
            : base(memoryCache)
        {
            _legacyContext = legacyContext;
            _syndicationContext = syndicationContext;
        }

        public StreetNameLatestItem FindLatestStreetNameById(string streetNameOsloId)
        {
            return GetOrAddLatestStreetNames(
                streetNames => streetNames.SelectMany(kvp => kvp.Value).Single(s => s.OsloId == streetNameOsloId),
                () => _syndicationContext.StreetNameLatestItems.FirstOrDefault(x => x.OsloId == streetNameOsloId));
        }

        public IEnumerable<MunicipalityLatestItem> GetAllLatestMunicipalities()
        {
            return GetOrAdd(AllMunicipalitiesCacheKey,
                () => _syndicationContext.MunicipalityLatestItems.ToList(),
                AllMunicipalitiesCacheDuration);
        }

        public IEnumerable<StreetNameLatestItem> GetAllLatestStreetNames()
        {
            return GetOrAdd(AllStreetNamesCacheKey,
                () => ConvertToCachedModel(_syndicationContext.StreetNameLatestItems.ToList()),
                AllStreetNamesCacheDuration).SelectMany(kvp => kvp.Value);
        }

        public IEnumerable<AddressDetailItem> GetLatestAddressesBy(string streetNameOsloId, string houseNumber, string boxNumber)
        {
            var streetName = FindLatestStreetNameById(streetNameOsloId);

            //no caching for addresses
            var query = _legacyContext.AddressDetail.Where(x => x.HouseNumber == houseNumber && x.BoxNumber == boxNumber);
            if (streetName != null)
                query = query.Where(x => x.StreetNameId == streetName.StreetNameId);

            return query;
        }

        public IEnumerable<AddressDetailItem> FindLatestAddressesByCrabSubaddressIds(IEnumerable<int> crabSubaddressIds)
        {
            //no caching for addresses
            return _legacyContext.AddressDetail
                .Where(detailItem =>
                    _legacyContext.CrabIdToOsloIds
                        .Where(osloIdItem => crabSubaddressIds.Contains(osloIdItem.SubaddressId.Value))
                        .Select(y => y.AddressId)
                        .Contains(detailItem.AddressId));
        }

        public IEnumerable<AddressDetailItem> FindLatestAddressesByCrabHouseNumberIds(IEnumerable<int> crabHouseNumberIds)
        {
            //no caching for addresses
            return _legacyContext.AddressDetail
                .Where(detailItem =>
                    _legacyContext.CrabIdToOsloIds
                        .Where(osloIdItem => crabHouseNumberIds.Contains(osloIdItem.HouseNumberId.Value))
                        .Select(y => y.AddressId)
                        .Contains(detailItem.AddressId));
        }

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
                () => _syndicationContext.StreetNameLatestItems.Where(x => nisCodes.Contains(x.NisCode)));
        }


        private T GetOrAddLatestStreetNames<T>(Func<Dictionary<string, IEnumerable<StreetNameLatestItem>>, T> ifCacheHit, Func<T> ifCacheNotHit)
        {
            return GetOrAdd(AllStreetNamesCacheKey,
                () => ConvertToCachedModel(_syndicationContext.StreetNameLatestItems.ToList()),
                AllStreetNamesCacheDuration,
                ifCacheHit,
                ifCacheNotHit);
        }

        private Dictionary<string, IEnumerable<StreetNameLatestItem>> ConvertToCachedModel(IEnumerable<StreetNameLatestItem> query)
        {
            return query.GroupBy(s => s.NisCode).ToDictionary(s => s.Key, s => s.AsEnumerable());
        }

    }
}
