namespace AddressRegistry.Api.Legacy.CrabHouseNumber
{
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Projections.Legacy;
    using System.Collections.Generic;
    using System.Linq;
    using Projections.Legacy.CrabIdToPersistentLocalId;


    public class CrabHouseNumberQuery : Query<CrabIdToPersistentLocalIdItem, CrabHouseNumberAddressFilter>
    {
        private readonly LegacyContext _context;

        public CrabHouseNumberQuery(LegacyContext context) => _context = context;

        protected override IQueryable<CrabIdToPersistentLocalIdItem> Filter(FilteringHeader<CrabHouseNumberAddressFilter> filtering)
        {
            var query = _context.CrabIdToPersistentLocalIds
                .Where(x => !x.IsRemoved && x.HouseNumberId.HasValue && x.PersistentLocalId.HasValue);

            if (filtering.ShouldFilter && filtering.Filter.CrabHouseNumberId.HasValue)
                query = query.Where(x => x.HouseNumberId == filtering.Filter.CrabHouseNumberId);

            return query;
        }

        protected override ISorting Sorting => new CrabHouseNumberAddressSorting();
    }

    internal class CrabHouseNumberAddressSorting : ISorting
    {
        public IEnumerable<string> SortableFields { get; } = new[]
        {
            nameof(CrabIdToPersistentLocalIdItem.HouseNumberId),
        };

        public SortingHeader DefaultSortingHeader { get; } = new SortingHeader(nameof(CrabIdToPersistentLocalIdItem.HouseNumberId), SortOrder.Ascending);
    }

    public class CrabHouseNumberAddressFilter
    {
        public int? CrabHouseNumberId { get; set; }
    }
}
