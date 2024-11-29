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
    using StreetNameStatus = Consumer.Read.StreetName.Projections.StreetNameStatus;

    public sealed class AddressSearchHandler : IRequestHandler<AddressSearchRequest, AddressSearchResponse>
    {
        private readonly IAddressApiElasticsearchClient _addressApiElasticsearchClient;
        private readonly IAddressApiStreetNameElasticsearchClient _addressApiStreetNameElasticsearchClient;
        private readonly IMunicipalityCache _municipalityCache;
        private readonly ResponseOptions _responseOptions;

        public AddressSearchHandler(
            IAddressApiElasticsearchClient addressApiElasticsearchClient,
            IAddressApiStreetNameElasticsearchClient addressApiStreetNameElasticsearchClient,
            IOptions<ResponseOptions> responseOptions,
            IMunicipalityCache municipalityCache)
        {
            _addressApiElasticsearchClient = addressApiElasticsearchClient;
            _addressApiStreetNameElasticsearchClient = addressApiStreetNameElasticsearchClient;
            _municipalityCache = municipalityCache;
            _responseOptions = responseOptions.Value;
        }

        public async Task<AddressSearchResponse> Handle(AddressSearchRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Filtering.Filter.Query))
            {
                return new AddressSearchResponse([]);
            }

            if (!TryGetValidNisCode(request.Filtering.Filter, out var nisCode))
            {
                return new AddressSearchResponse([]);
            }

            var pagination = (PaginationRequest)request.Pagination;

            var query = request.Filtering.Filter.Query;

            if (ContainsNumber(query))
            {
                AddressStatus? addressStatus = null;
                if (!string.IsNullOrWhiteSpace(request.Filtering.Filter.Status) &&
                    !Enum.TryParse<AdresStatus>(request.Filtering.Filter.Status, true, out _))
                {
                    return new AddressSearchResponse([]);
                }

                if (!string.IsNullOrWhiteSpace(request.Filtering.Filter.Status) &&
                    Enum.TryParse<AdresStatus>(request.Filtering.Filter.Status, true, out var status))
                {
                    addressStatus = status.ConvertFromAdresStatus();
                }

                var response = await _addressApiElasticsearchClient.SearchAddresses(
                    query,
                    nisCode,
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

            StreetNameStatus? streetNameStatus = null;
            if (!string.IsNullOrWhiteSpace(request.Filtering.Filter.Status) &&
                !Enum.TryParse<StraatnaamStatus>(request.Filtering.Filter.Status, true, out _))
            {
                return new AddressSearchResponse([]);
            }

            if (!string.IsNullOrWhiteSpace(request.Filtering.Filter.Status) &&
                Enum.TryParse<StraatnaamStatus>(request.Filtering.Filter.Status, true, out var straatNaamStatus))
            {
                streetNameStatus = Map(straatNaamStatus);
            }

            var streetNameResponse = await _addressApiStreetNameElasticsearchClient
                .SearchStreetNames(query, nisCode, streetNameStatus, pagination.Limit);

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

        private bool TryGetValidNisCode(AddressSearchFilter filter, out string? nisCode)
        {
            nisCode = filter.NisCode;

            if (!string.IsNullOrWhiteSpace(nisCode))
            {
                return _municipalityCache.IsNisCodeValid(nisCode);
            }

            if (!string.IsNullOrWhiteSpace(filter.MunicipalityName))
            {
                nisCode = _municipalityCache.GetNisCodeByName(filter.MunicipalityName);

                if (string.IsNullOrWhiteSpace(nisCode))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool ContainsNumber(string input)
        {
            return Regex.IsMatch(input, @"\d");
        }

        private StreetNameStatus Map(StraatnaamStatus straatnaamStatus)
        {
            return straatnaamStatus switch
            {
                StraatnaamStatus.Voorgesteld => StreetNameStatus.Proposed,
                StraatnaamStatus.InGebruik => StreetNameStatus.Current,
                StraatnaamStatus.Gehistoreerd => StreetNameStatus.Retired,
                StraatnaamStatus.Afgekeurd => StreetNameStatus.Rejected,
                _ => throw new ArgumentOutOfRangeException(nameof(straatnaamStatus), straatnaamStatus, null)
            };
        }
    }
}
