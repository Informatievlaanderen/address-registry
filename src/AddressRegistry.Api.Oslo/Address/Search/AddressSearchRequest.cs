namespace AddressRegistry.Api.Oslo.Address.Search
{
    using System;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using MediatR;
    using Newtonsoft.Json;

    public sealed record AddressSearchRequest(
        FilteringHeader<AddressSearchFilter> Filtering,
        SortingHeader Sorting,
        IPaginationRequest Pagination) : IRequest<AddressSearchResponse>;

    public sealed class AddressSearchFilter
    {
        public string? Query { get; init; }
        public string? MunicipalityOrPostalName { get; init; }
        public string? Status { get; init; }
    }
}
