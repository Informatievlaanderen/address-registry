namespace AddressRegistry.Api.Oslo.Address.Search
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Infrastructure.Elastic;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Straatnaam;
    using Convertors;
    using Infrastructure.Elastic;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.Extensions.Options;
    using StreetName;

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
            string? nisCode;
            if (!string.IsNullOrWhiteSpace(request.Filtering.Filter.MunicipalityName))
            {

            }

            var pagination = (PaginationRequest)request.Pagination;
            if (string.IsNullOrWhiteSpace(request.Filtering.Filter.Query))
                return new AddressSearchResponse([]);

            var query = request.Filtering.Filter.Query;

            if (ContainsNumber(query))
            {
                AddressStatus? addressStatus = null;
                if(!string.IsNullOrWhiteSpace(request.Filtering.Filter.Status) && !Enum.TryParse<AdresStatus>(request.Filtering.Filter.Status, true, out _))
                    return new AddressSearchResponse([]);

                if(!string.IsNullOrWhiteSpace(request.Filtering.Filter.Status) && Enum.TryParse<AdresStatus>(request.Filtering.Filter.Status, true, out var status))
                    addressStatus = status.ConvertFromAdresStatus();

                var response = await _addressApiElasticsearchClient.SearchAddresses(
                    query,
                    request.Filtering.Filter.MunicipalityName,
                    addressStatus,
                    pagination.Limit);

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

            Consumer.Read.StreetName.Projections.StreetNameStatus? streetNameStatus = null;
            if(!string.IsNullOrWhiteSpace(request.Filtering.Filter.Status) && !Enum.TryParse<StraatnaamStatus>(request.Filtering.Filter.Status, true, out _))
                return new AddressSearchResponse([]);

            if (!string.IsNullOrWhiteSpace(request.Filtering.Filter.Status) &&
                Enum.TryParse<StraatnaamStatus>(request.Filtering.Filter.Status, true, out var straatNaamStatus))
                streetNameStatus = Map(straatNaamStatus);

            var streetNameResponse = await _addressApiStreetNameElasticsearchClient.SearchStreetNames(query, request.Filtering.Filter.MunicipalityName, streetNameStatus, pagination.Limit);
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

        private static bool ContainsNumber(string input)
        {
            return Regex.IsMatch(input, @"\d");
        }

        private Consumer.Read.StreetName.Projections.StreetNameStatus Map(StraatnaamStatus straatnaamStatus)
        {
            switch (straatnaamStatus)
            {
                case StraatnaamStatus.Voorgesteld:
                    return Consumer.Read.StreetName.Projections.StreetNameStatus.Proposed;
                case StraatnaamStatus.InGebruik:
                    return Consumer.Read.StreetName.Projections.StreetNameStatus.Current;
                case StraatnaamStatus.Gehistoreerd:
                    return Consumer.Read.StreetName.Projections.StreetNameStatus.Retired;
                case StraatnaamStatus.Afgekeurd:
                    return Consumer.Read.StreetName.Projections.StreetNameStatus.Rejected;
                default:
                    throw new ArgumentOutOfRangeException(nameof(straatnaamStatus), straatnaamStatus, null);
            }
        }
    }
}
