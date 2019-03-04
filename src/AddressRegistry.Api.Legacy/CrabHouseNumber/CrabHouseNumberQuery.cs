namespace AddressRegistry.Api.Legacy.CrabHouseNumber
{
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Projections.Legacy;
    using Projections.Legacy.CrabIdToOsloId;
    using System.Collections.Generic;
    using System.Linq;


    public class CrabHouseNumberQuery : Query<CrabIdToOsloIdItem, CrabHouseNumberAddressFilter>
    {
        private readonly LegacyContext _context;

        public CrabHouseNumberQuery(LegacyContext context) => _context = context;

        protected override IQueryable<CrabIdToOsloIdItem> Filter(FilteringHeader<CrabHouseNumberAddressFilter> filtering)
        {
            var query = _context.CrabIdToOsloIds
                .Where(x => !x.IsRemoved && x.HouseNumberId.HasValue && x.OsloId.HasValue);

            if (filtering.ShouldFilter)
                query = query.Where(x => x.HouseNumberId == filtering.Filter.CrabHouseNumberId);

            return query;
        }

        protected override ISorting Sorting => new CrabHouseNumberAddressSorting();
    }

    internal class CrabHouseNumberAddressSorting : ISorting
    {
        public IEnumerable<string> SortableFields { get; } = new[]
        {
            nameof(CrabIdToOsloIdItem.HouseNumberId),
        };

        public SortingHeader DefaultSortingHeader { get; } = new SortingHeader(nameof(CrabIdToOsloIdItem.HouseNumberId), SortOrder.Ascending);
    }

    public class CrabHouseNumberAddressFilter
    {
        public int? CrabHouseNumberId { get; set; }
    }
}
