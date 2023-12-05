namespace AddressRegistry.Api.Oslo.Address.List
{
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using MediatR;

    public sealed record AddressListOsloRequest(
        FilteringHeader<AddressFilter> Filtering,
        SortingHeader Sorting,
        IPaginationRequest Pagination) : IRequest<AddressListOsloResponse>;
}
