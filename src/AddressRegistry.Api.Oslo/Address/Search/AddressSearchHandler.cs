namespace AddressRegistry.Api.Oslo.Address.Search
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Infrastructure.Elastic;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Infrastructure.Elastic;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.Extensions.Options;

    public sealed class AddressSearchHandler : IRequestHandler<AddressSearchRequest, AddressSearchResponse>
    {
        private readonly IAddressApiElasticsearchClient _addressApiElasticsearchClient;
        private readonly IAddressApiStreetNameElasticsearchClient _addressApiStreetNameElasticsearchClient;
        private readonly ResponseOptions _responseOptions;

        public AddressSearchHandler(
            IAddressApiElasticsearchClient addressApiElasticsearchClient,
            IAddressApiStreetNameElasticsearchClient addressApiStreetNameElasticsearchClient,
            IOptions<ResponseOptions> responseOptions)
        {
            _addressApiElasticsearchClient = addressApiElasticsearchClient;
            _addressApiStreetNameElasticsearchClient = addressApiStreetNameElasticsearchClient;
            _responseOptions = responseOptions.Value;
        }

        public async Task<AddressSearchResponse> Handle(AddressSearchRequest request, CancellationToken cancellationToken)
        {
            var pagination = (PaginationRequest)request.Pagination;
            if (string.IsNullOrWhiteSpace(request.Filtering.Filter.Query))
                return new AddressSearchResponse([]);

            var query = request.Filtering.Filter.Query;

            if (ContainsNumberAfterSpace(query))
            {
                var response = await _addressApiElasticsearchClient.SearchAddresses(query, request.Filtering.Filter.MunicipalityOrPostalName, pagination.Limit);
                var language = response.Language ?? Language.nl;
                return new AddressSearchResponse(response.Addresses
                    .AsEnumerable()
                    .Select(x => new AddressSearchItem(
                        $"{_responseOptions.Naamruimte.Trim('/')}/{x.AddressPersistentLocalId}",
                        x.AddressPersistentLocalId.ToString(),
                        new Uri(string.Format(_responseOptions.DetailUrl, x.AddressPersistentLocalId)),
                        x.FullAddress.FirstOrDefault(name => name.Language == language)?.Spelling ?? x.FullAddress.First().Spelling))
                    .Take(pagination.Limit)
                    .ToList());
            }

            var streetNameResponse = await _addressApiStreetNameElasticsearchClient.SearchStreetNames(query, request.Filtering.Filter.MunicipalityOrPostalName, pagination.Limit);
            var streetNameLanguage = streetNameResponse.Language ?? Language.nl;
            return new AddressSearchResponse(streetNameResponse.StreetNames
                .AsEnumerable()
                .Select(x => new AddressSearchItem(
                    $"{_responseOptions.StraatNaamNaamruimte.Trim('/')}/{x.StreetNamePersistentLocalId}",
                    x.StreetNamePersistentLocalId.ToString(),
                    new Uri(string.Format(_responseOptions.StraatnaamDetailUrl, x.StreetNamePersistentLocalId)),
                    x.FullStreetNames.FirstOrDefault(name => name.Language == streetNameLanguage)?.Spelling ?? x.FullStreetNames.First().Spelling))
                .Take(pagination.Limit)
                .ToList());
        }

        private static bool ContainsNumberAfterSpace(string input)
        {
            return Regex.IsMatch(input, @"\s\d\w*");
        }
    }
}
