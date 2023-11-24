namespace AddressRegistry.Api.Oslo.AddressMatch.V2.Matching
{
    using System.Collections.Generic;
    using System.Linq;
    using Consumer.Read.StreetName.Projections;

    public sealed class StreetNameCache
    {
        public StreetNameCache(IList<StreetNameLatestItem> streetNameLatestItems)
        {
            ByPersistentLocalId = streetNameLatestItems.ToDictionary(x => x.PersistentLocalId);
            ByMunicipalityNisCode = streetNameLatestItems
                .GroupBy(x => x.NisCode)
                .ToDictionary(x => x.Key!, x => x.AsEnumerable());
        }

        public Dictionary<int,StreetNameLatestItem> ByPersistentLocalId { get; private set; }
        public Dictionary<string, IEnumerable<StreetNameLatestItem>> ByMunicipalityNisCode { get; private set; }
    }
}
