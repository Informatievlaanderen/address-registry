namespace AddressRegistry.Api.Legacy.AddressMatch.V1.Matching
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using AddressRegistry.Projections.Legacy.AddressDetail;
    using AddressRegistry.Projections.Legacy.AddressMatch;
    using AddressRegistry.Projections.Syndication.StreetName;
    using Microsoft.Extensions.Caching.Memory;

    public interface IKadRrService
    {
        IEnumerable<StreetNameLatestItem?> GetStreetNamesByKadStreet(
            string kadStreetCode,
            string nisCode);

        StreetNameLatestItem? GetStreetNameByRrStreet(
            string rrStreetCode,
            string postalCode);

        IEnumerable<AddressDetailItem> GetAddressesBy(
            string houseNumber,
            string index,
            string rrStreetCode,
            string postalCode);
    }

    public class CrabKadRrService : CachedService, IKadRrService
    {
        private readonly ILatestQueries _latestQueries;
        private readonly AddressMatchContext _context;
        private static readonly TimeSpan AllKadStreetMappingsCacheDuration = TimeSpan.FromDays(1);
        private static readonly TimeSpan AllRrStreetMappingsCacheDuration = TimeSpan.FromDays(1);
        private const string GetStreetNamesByKadStreetCacheKey = "GetStreetNamesByKadStreet";
        private const string GetStreetNamesByRrStreetCacheKey = "GetStreetNamesByRrStreet";

        public CrabKadRrService(IMemoryCache memoryCache, ILatestQueries latestQueries, AddressMatchContext context)
            : base(memoryCache)
        {
            _latestQueries = latestQueries;
            _context = context;
        }

        public IEnumerable<StreetNameLatestItem?> GetStreetNamesByKadStreet(string kadStreetCode, string nisCode)
            => GetOrAdd(
                GetStreetNamesByKadStreetCacheKey,
                GetAllKadStreetNamesMappingsFromCrab,
                AllKadStreetMappingsCacheDuration,
                mappings => mappings
                    .Where(m => m.KadStreetNameCode == kadStreetCode && m.NisCode == nisCode)
                    .Select(Map),
                () => GetStreetNamesByKadStreetFromCrab(kadStreetCode, nisCode));

        public StreetNameLatestItem? GetStreetNameByRrStreet(string rrStreetCode, string postalCode)
            => GetOrAdd(
                GetStreetNamesByRrStreetCacheKey,
                GetAllRrStreetNamesMappingsFromCrab,
                AllRrStreetMappingsCacheDuration,
                mappings => Map(mappings
                    .SingleOrDefault(m => m.StreetCode == rrStreetCode && m.PostalCode == postalCode)),
                () => GetStreetNamesByRrStreetFromCrab(rrStreetCode, postalCode));

        public IEnumerable<AddressDetailItem> GetAddressesBy(
            string houseNumber,
            string index,
            string rrStreetCode,
            string postalCode)
        {
            var mappings = _context
                .RRAddresses
                .Where(x =>
                    x.RRHouseNumber == houseNumber &&
                    x.RRIndex == index &&
                    x.StreetCode == rrStreetCode &&
                    x.PostalCode == postalCode);

            var addressesBySubaddressIds = _latestQueries
                .FindLatestAddressesByCrabSubaddressIds(mappings
                    .Where(rram => rram.AddressType == "1")
                    .Select(rram => rram.AddressId))
                .ToList();

            var addressesByHouseNumberIds = _latestQueries
                .FindLatestAddressesByCrabHouseNumberIds(mappings
                    .Where(rram => rram.AddressType == "2")
                    .Select(rram => rram.AddressId))
                .ToList();

            return addressesBySubaddressIds.Concat(addressesByHouseNumberIds);
        }

        private IEnumerable<StreetNameLatestItem?> GetStreetNamesByKadStreetFromCrab(string kadStreetCode, string nisCode)
            => _context
                .KadStreetNames
                .Where(x => x.KadStreetNameCode == kadStreetCode && x.NisCode == nisCode)
                .ToList()
                .Select(Map);

        private IEnumerable<KadStreetName> GetAllKadStreetNamesMappingsFromCrab()
            => _context.KadStreetNames.ToList();

        private StreetNameLatestItem? GetStreetNamesByRrStreetFromCrab(string rrStreetCode, string postalCode)
            => Map(
                _context
                    .RRStreetNames
                    .SingleOrDefault(x => x.StreetCode == rrStreetCode && x.PostalCode == postalCode));

        private IEnumerable<RRStreetName> GetAllRrStreetNamesMappingsFromCrab()
            => _context.RRStreetNames.ToList();

        private StreetNameLatestItem? Map(KadStreetName source)
            => source == null
                ? null
                : FindById(source.StreetNameId);

        private StreetNameLatestItem? Map(RRStreetName source)
            => source == null
                ? null
                : FindById(source.StreetNameId);

        private StreetNameLatestItem FindById(int persistentLocalId)
            => _latestQueries
                .FindLatestStreetNameById(persistentLocalId.ToString(CultureInfo.InvariantCulture));
    }
}
