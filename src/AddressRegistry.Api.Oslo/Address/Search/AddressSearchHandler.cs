namespace AddressRegistry.Api.Oslo.Address.Search
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Infrastructure.Elastic;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.Extensions.Options;
    using Projections.Elastic.AddressSearch;

    public sealed class AddressSearchHandler : IRequestHandler<AddressSearchRequest, AddressSearchResponse>
    {
        private readonly IAddressApiElasticsearchClient _addressApiElasticsearchClient;
        private readonly ResponseOptions _responseOptions;

        public AddressSearchHandler(
            IAddressApiElasticsearchClient addressApiElasticsearchClient,
            IOptions<ResponseOptions> responseOptions)
        {
            _addressApiElasticsearchClient = addressApiElasticsearchClient;
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

            return await SearchStreetNames(query, request.Filtering.Filter.MunicipalityOrPostalName, pagination.Limit);
        }

        private async Task<AddressSearchResponse> SearchStreetNames(string query, string? municipalityOrPostalNameQuery, int limit)
        {
            var streetNames = query.Split(' ');
            streetNames = streetNames.Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

            List<StreetNameSearchResult> streetNameResult;
            if (streetNames.Length > 1 || !string.IsNullOrWhiteSpace(municipalityOrPostalNameQuery))
            {
                var municipalityOrPostalName = string.IsNullOrWhiteSpace(municipalityOrPostalNameQuery)
                    ? streetNames.Last()
                    : municipalityOrPostalNameQuery;

                var namesToSearch = new List<string>();
                var previousStreetName = string.Empty;
                foreach (var streetName in streetNames)
                {
                    if (!string.IsNullOrWhiteSpace(previousStreetName))
                    {
                        namesToSearch.Add(previousStreetName + ' ' + streetName);
                        previousStreetName = previousStreetName + ' ' + streetName;
                    }
                    else
                    {
                        namesToSearch.Add(streetName);
                        previousStreetName = streetName;
                    }
                }
                streetNameResult = (await _addressApiElasticsearchClient
                    .SearchStreetNames(
                        namesToSearch.ToArray(),
                        municipalityOrPostalName,
                        !string.IsNullOrWhiteSpace(municipalityOrPostalNameQuery),
                        limit))
                    .ToList();
            }
            else
            {
                streetNameResult = (await _addressApiElasticsearchClient.SearchStreetNames(query, limit)).ToList();
            }

            return new AddressSearchResponse(streetNameResult
                .Select(x => new AddressSearchItem(
                    $"{_responseOptions.StraatNaamNaamruimte.Trim('/')}/{x.StreetNamePersistentLocalId}",
                    x.StreetNamePersistentLocalId.ToString(),
                    new Uri(string.Format(_responseOptions.StraatnaamDetailUrl, x.StreetNamePersistentLocalId)),
                    x.GetFormattedStreetName()))
                .Take(limit)
                .ToList());
        }

        private static bool ContainsNumberAfterSpace(string input)
        {
            return Regex.IsMatch(input, @"\s\d\w*");
        }
    }
}
