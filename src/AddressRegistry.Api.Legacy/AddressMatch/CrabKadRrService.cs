namespace AddressRegistry.Api.Legacy.AddressMatch
{
    using Microsoft.Extensions.Caching.Memory;
    using Projections.Legacy;
    using Projections.Legacy.AddressDetail;
    using Projections.Legacy.AddressMatch;
    using Projections.Syndication.StreetName;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    public interface IKadRrService
    {
        IEnumerable<StreetNameLatestItem> GetStreetNamesByKadStreet(string kadStreetCode, string nisCode);
        StreetNameLatestItem GetStreetNameByRrStreet(string rrStreetCode, string postalCode);
        IEnumerable<AddressDetailItem> GetAddressesBy(string houseNumber, string index, string rrStreetCode, string postalCode);
    }

    public class CrabKadRrService : CachedService, IKadRrService
    {
        private readonly ILatestQueries _latestQueries;
        private readonly LegacyContext _legacyContext;
        private static readonly TimeSpan AllKadStreetMappingsCacheDuration = TimeSpan.FromDays(1);
        private static readonly TimeSpan AllRrStreetMappingsCacheDuration = TimeSpan.FromDays(1);

        public CrabKadRrService(IMemoryCache memoryCache, ILatestQueries latestQueries, LegacyContext legacyContext)
            : base(memoryCache)
        {
            _latestQueries = latestQueries;
            _legacyContext = legacyContext;
        }

        public IEnumerable<StreetNameLatestItem> GetStreetNamesByKadStreet(string kadStreetCode, string nisCode)
        {
            return GetOrAdd("GetStreetNamesByKadStreet", GetAllKadStreetNamesMappingsFromCrab, AllKadStreetMappingsCacheDuration,
                mappings => mappings.Where(m => m.KadStreetNameCode == kadStreetCode && m.NisCode == nisCode).Select(Map),
                () => GetStreetNamesByKadStreetFromCrab(kadStreetCode, nisCode));
        }

        public StreetNameLatestItem GetStreetNameByRrStreet(string rrStreetCode, string postalCode)
        {
            return GetOrAdd("GetStreetNamesByRrStreet", GetAllRrStreetNamesMappingsFromCrab, AllRrStreetMappingsCacheDuration,
                mappings => Map(mappings.SingleOrDefault(m => m.StreetCode == rrStreetCode && m.PostalCode == postalCode)),
                () => GetStreetNamesByRrStreetFromCrab(rrStreetCode, postalCode));
        }

        public IEnumerable<AddressDetailItem> GetAddressesBy(string houseNumber, string index, string rrStreetCode, string postalCode)
        {
            var mappings = _legacyContext.RRAddresses.Where(x =>
                x.RRHouseNumber == houseNumber &&
                x.RRIndex == index &&
                x.StreetCode == rrStreetCode &&
                x.PostalCode == postalCode);

            return _latestQueries
                .FindLatestAddressesByCrabSubaddressIds(mappings.Where(rram => rram.AddressType == "1").Select(rram => rram.AddressId))
                .Concat(
                    _latestQueries.FindLatestAddressesByCrabHouseNumberIds(mappings.Where(rram => rram.AddressType == "2").Select(rram => rram.AddressId)));
        }

        private IEnumerable<StreetNameLatestItem> GetStreetNamesByKadStreetFromCrab(string kadStreetCode, string nisCode)
        {
            return _legacyContext
                    .KadStreetNames
                    .Where(x => x.KadStreetNameCode == kadStreetCode && x.NisCode == nisCode)
                    .ToList()
                    .Select(Map);
        }

        private IEnumerable<KadStreetName> GetAllKadStreetNamesMappingsFromCrab()
        {
            return _legacyContext.KadStreetNames.ToList();
        }

        private StreetNameLatestItem GetStreetNamesByRrStreetFromCrab(string rrStreetCode, string postalCode)
        {
            var streetName = _legacyContext.RRStreetNames.SingleOrDefault(x => x.StreetCode == rrStreetCode && x.PostalCode == postalCode);

            if (streetName == null)
                return null;

            return Map(streetName);
        }

        private IEnumerable<RRStreetName> GetAllRrStreetNamesMappingsFromCrab()
        {
            return _legacyContext.RRStreetNames.ToList();
        }

        private StreetNameLatestItem Map(KadStreetName source)
        {
            if (source == null)
                return null;

            return FindById(source.StreetNameId);
        }

        private StreetNameLatestItem Map(RRStreetName source)
        {
            if (source == null)
                return null;

            return FindById(source.StreetNameId);
        }

        private StreetNameLatestItem FindById(int osloId)
        {
            return _latestQueries.FindLatestStreetNameById(osloId.ToString(CultureInfo.InvariantCulture));
        }
    }
}
