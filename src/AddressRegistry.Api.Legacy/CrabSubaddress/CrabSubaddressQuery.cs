namespace AddressRegistry.Api.Legacy.CrabSubaddress
{
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Projections.Legacy;
    using Projections.Legacy.CrabIdToOsloId;
    using System.Collections.Generic;
    using System.Linq;


    public class CrabSubaddressQuery : Query<CrabIdToOsloIdItem, CrabSubaddressAddressFilter>
    {
        private readonly LegacyContext _context;

        public CrabSubaddressQuery(LegacyContext context) => _context = context;

        protected override IQueryable<CrabIdToOsloIdItem> Filter(FilteringHeader<CrabSubaddressAddressFilter> filtering)
        {
            var query = _context.CrabIdToOsloIds
                .Where(x => !x.IsRemoved && x.SubaddressId.HasValue && x.OsloId.HasValue);

            if (filtering.ShouldFilter)
                query = query.Where(x => x.SubaddressId == filtering.Filter.CrabSubaddressId);

            return query;
        }

        protected override ISorting Sorting => new CrabSubaddressAddressSorting();
    }

    internal class CrabSubaddressAddressSorting : ISorting
    {
        public IEnumerable<string> SortableFields { get; } = new[]
        {
            nameof(CrabIdToOsloIdItem.SubaddressId),
        };

        public SortingHeader DefaultSortingHeader { get; } = new SortingHeader(nameof(CrabIdToOsloIdItem.SubaddressId), SortOrder.Ascending);
    }

    public class CrabSubaddressAddressFilter
    {
        public int? CrabSubaddressId { get; set; }
    }
}
