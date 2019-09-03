namespace AddressRegistry.Api.Legacy.Address
{
    using System;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;

    public static class PaginationNextUriExtension
    {
        public static Uri BuildNextUri(
            this PaginationInfo paginationInfo,
            string volgendeUrlBase)
        {
            var offset = paginationInfo.Offset;
            var limit = paginationInfo.Limit;

            return offset + limit < paginationInfo.TotalItems
                ? new Uri(string.Format(volgendeUrlBase, offset + limit, limit))
                : null;
        }
    }
}
