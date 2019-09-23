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
                .Where(x => x.HouseNumberId.HasValue && x.PersistentLocalId.HasValue);

            var parsed = int.TryParse(filtering.Filter.CrabHouseNumberId, out var objectId);
            if (filtering.ShouldFilter && parsed)
                query = query.Where(x => x.HouseNumberId == objectId);
            else if (!parsed && !string.IsNullOrEmpty(filtering.Filter.CrabHouseNumberId))
                return new List<CrabIdToPersistentLocalIdItem>().AsQueryable();

            return query;
        }

        protected override ISorting Sorting => new CrabHouseNumberAddressSorting();
    }

    public class CrabHouseNumberAddressSorting : ISorting
    {
        public IEnumerable<string> SortableFields { get; } = new[]
        {
            nameof(CrabIdToPersistentLocalIdItem.HouseNumberId),
        };

        public SortingHeader DefaultSortingHeader { get; } = new SortingHeader(nameof(CrabIdToPersistentLocalIdItem.HouseNumberId), SortOrder.Ascending);
    }

    public class CrabHouseNumberAddressFilter
    {
        public string CrabHouseNumberId { get; set; }
    }
}
