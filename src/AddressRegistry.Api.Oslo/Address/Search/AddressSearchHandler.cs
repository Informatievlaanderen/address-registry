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
                var addressExtraction = ExtractAddressComponents(query);
                var response = await _addressApiElasticsearchClient.SearchAddresses(addressExtraction.streetName,
                    addressExtraction.houseNumber,
                    addressExtraction.boxNumber,
                    addressExtraction.postalCode,
                    string.IsNullOrWhiteSpace(request.Filtering.Filter.MunicipalityOrPostalName)
                        ? addressExtraction.municipalityOrPostalName
                        : request.Filtering.Filter.MunicipalityOrPostalName,
                    !string.IsNullOrWhiteSpace(request.Filtering.Filter.MunicipalityOrPostalName),
                    pagination.Limit);

                return new AddressSearchResponse(response.Addresses
                    .AsEnumerable()
                    .Select(x => new AddressSearchItem(
                        $"{_responseOptions.Naamruimte.Trim('/')}/{x.AddressPersistentLocalId}",
                        x.AddressPersistentLocalId.ToString(),
                        new Uri(string.Format(_responseOptions.DetailUrl, x.AddressPersistentLocalId)),
                        x.FullAddress.FirstOrDefault(name => name.Language == Language.nl)?.Spelling ?? x.FullAddress.First().Spelling))
                    .Take(pagination.Limit)
                    .ToList());
            }

            return await SearchStreetNames(query, request.Filtering.Filter.MunicipalityOrPostalName, pagination.Limit);
        }

        // Define regex patterns
        private static readonly Regex StreetNameRegex = new Regex(@"^(.*?)(?=\d)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex HouseNumberRegex = new Regex(@"\d[\w\-/_]*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex BoxNumberRegex = new Regex(@"(?:bus|bte|boite|boîte|box)\s*([\w/_\-.]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex postalCodeRegex = new Regex(@"\b\d{4}\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex municipalityRegex = new Regex(@"\b([a-zA-Z\s\-]+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static (string streetName, string houseNumber, string? boxNumber, string? postalCode, string? municipalityOrPostalName) ExtractAddressComponents(string address)
        {
            // Extract street name
            var streetMatch = StreetNameRegex.Match(address);
            var streetName = streetMatch.Success ? streetMatch.Value.Trim() : address;

            // Extract house number
            var houseMatch = HouseNumberRegex.Match(address);
            var houseNumber = houseMatch.Success ? houseMatch.Value.Trim() : string.Empty;

            if (!string.IsNullOrEmpty(houseNumber))
                houseNumber = Regex.Replace(houseNumber, @"\s*(bus|bte|boite|boîte|box).*", "", RegexOptions.IgnoreCase);

            // Extract box number
            var boxMatch = BoxNumberRegex.Match(address);
            var boxNumber = boxMatch.Success ? boxMatch.Groups[1].Value.Trim() : null;

            var postalCodeMatch = postalCodeRegex.Match(address);
            var postalCode = postalCodeMatch.Success ? postalCodeMatch.Value.Trim() : null;

            string municipality = null;
            var cleanedAddress = address
                .Replace(streetName, "")
                .Replace(houseNumber, "");

            if (boxNumber is not null)
            {
                cleanedAddress = cleanedAddress
                    .Replace("bus", "", StringComparison.OrdinalIgnoreCase)
                    .Replace("bte", "", StringComparison.OrdinalIgnoreCase)
                    .Replace("boite", "", StringComparison.OrdinalIgnoreCase)
                    .Replace("boîte", "", StringComparison.OrdinalIgnoreCase)
                    .Replace("box", "", StringComparison.OrdinalIgnoreCase)
                    .Replace(boxNumber, "");
            }

            if(postalCode is not null)
                cleanedAddress = cleanedAddress.Replace(postalCode, "");

            cleanedAddress = cleanedAddress.Trim(", ".ToCharArray()); // Trim commas and spaces

            // Match remaining part as municipality
            var municipalityMatch = municipalityRegex.Match(cleanedAddress);
            municipality = municipalityMatch.Success ? municipalityMatch.Groups[1].Value.Trim() : null;

            return (streetName, houseNumber, boxNumber, postalCode, municipality);
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
