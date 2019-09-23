namespace AddressRegistry.Api.Legacy.CrabSubaddress
{
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Projections.Legacy;
    using System.Collections.Generic;
    using System.Linq;
    using Projections.Legacy.CrabIdToPersistentLocalId;

    public class CrabSubaddressQuery : Query<CrabIdToPersistentLocalIdItem, CrabSubaddressAddressFilter>
    {
        private readonly LegacyContext _context;

        public CrabSubaddressQuery(LegacyContext context) => _context = context;

        protected override IQueryable<CrabIdToPersistentLocalIdItem> Filter(FilteringHeader<CrabSubaddressAddressFilter> filtering)
        {
            var query = _context.CrabIdToPersistentLocalIds
                .Where(x => x.SubaddressId.HasValue && x.PersistentLocalId.HasValue);

            var parsed = int.TryParse(filtering.Filter.CrabSubaddressId, out var objectId);
            if (filtering.ShouldFilter && parsed)
                query = query.Where(x => x.SubaddressId == objectId);
            else if (!parsed && !string.IsNullOrEmpty(filtering.Filter.CrabSubaddressId))
                return new List<CrabIdToPersistentLocalIdItem>().AsQueryable();

            return query;
        }

        protected override ISorting Sorting => new CrabSubaddressAddressSorting();
    }

    public class CrabSubaddressAddressSorting : ISorting
    {
        public IEnumerable<string> SortableFields { get; } = new[]
        {
            nameof(CrabIdToPersistentLocalIdItem.SubaddressId),
        };

        public SortingHeader DefaultSortingHeader { get; } = new SortingHeader(nameof(CrabIdToPersistentLocalIdItem.SubaddressId), SortOrder.Ascending);
    }

    public class CrabSubaddressAddressFilter
    {
        public string CrabSubaddressId { get; set; }
    }
}
