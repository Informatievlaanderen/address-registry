namespace AddressRegistry.Api.Oslo.Address.Search
{
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using MediatR;

    public sealed record AddressSearchRequest(
        FilteringHeader<AddressSearchFilter> Filtering,
        IPaginationRequest Pagination) : IRequest<AddressSearchResponse>;

    public sealed class AddressSearchFilter
    {
        public string? Query { get; init; }
        public string? NisCode { get; init; }
        public string? MunicipalityName { get; init; }
        public string? Status { get; init; }
        public ResultType? ResultType { get; init; }
    }

    public enum ResultType
    {
        Address,
        StreetName
    }
}
