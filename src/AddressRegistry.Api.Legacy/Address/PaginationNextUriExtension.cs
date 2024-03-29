namespace AddressRegistry.Api.Legacy.Address
{
    using System;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;

    public static class PaginationNextUriExtension
    {
        public static Uri? BuildNextUri(
            this PaginationInfo paginationInfo,
            int itemCountInCollection,
            string nextUrlBase)
        {
            var offset = paginationInfo.Offset;
            var limit = paginationInfo.Limit;

            return paginationInfo.HasNextPage(itemCountInCollection)
                ? new Uri(string.Format(nextUrlBase, offset + limit, limit))
                : null;
        }
    }
}
