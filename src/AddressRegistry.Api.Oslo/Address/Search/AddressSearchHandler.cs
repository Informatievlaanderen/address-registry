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
            if (string.IsNullOrEmpty(request.Filtering.Filter.Query))
                return new AddressSearchResponse([]);

            var query = request.Filtering.Filter.Query;

            if (ContainsNumberAfterSpace(query))
            {
                return new AddressSearchResponse([]);
            }

            return await SearchStreetNames(query, request.Filtering.Filter.MunicipalityOrPostalName, pagination.Limit);
        }

        private async Task<AddressSearchResponse> SearchStreetNames(string query, string? municipalityOrPostalNameQuery, int limit)
        {
            var streetNames = query.Split(' ');
            streetNames = streetNames.Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToArray();

            List<StreetNameSearchResult> streetNameResult;
            if (streetNames.Length > 1 || !string.IsNullOrEmpty(municipalityOrPostalNameQuery))
            {
                var municipalityOrPostalName = string.IsNullOrEmpty(municipalityOrPostalNameQuery)
                    ? streetNames.Last()
                    : municipalityOrPostalNameQuery;

                var namesToSearch = new List<string>();
                var previousStreetName = string.Empty;
                foreach (var streetName in streetNames)
                {
                    if (!string.IsNullOrEmpty(previousStreetName))
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
                streetNameResult = (await _addressApiElasticsearchClient.SearchStreetNames(namesToSearch.ToArray(), municipalityOrPostalName)).ToList();
            }
            else
            {
                streetNameResult = (await _addressApiElasticsearchClient.SearchStreetNames(query)).ToList();
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
