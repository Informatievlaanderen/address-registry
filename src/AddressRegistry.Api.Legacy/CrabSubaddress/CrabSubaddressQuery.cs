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
                .Where(x => !x.IsRemoved && x.SubaddressId.HasValue && x.PersistentLocalId.HasValue);

            if (filtering.ShouldFilter && filtering.Filter.CrabSubaddressId.HasValue)
                query = query.Where(x => x.SubaddressId == filtering.Filter.CrabSubaddressId);

            return query;
        }

        protected override ISorting Sorting => new CrabSubaddressAddressSorting();
    }

    internal class CrabSubaddressAddressSorting : ISorting
    {
        public IEnumerable<string> SortableFields { get; } = new[]
        {
            nameof(CrabIdToPersistentLocalIdItem.SubaddressId),
        };

        public SortingHeader DefaultSortingHeader { get; } = new SortingHeader(nameof(CrabIdToPersistentLocalIdItem.SubaddressId), SortOrder.Ascending);
    }

    public class CrabSubaddressAddressFilter
    {
        public int? CrabSubaddressId { get; set; }
    }
}
