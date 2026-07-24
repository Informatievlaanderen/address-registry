namespace AddressRegistry.Api.Oslo.Address.V3.Search
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.Oslo.Address.Search;
    using AddressRegistry.Infrastructure.Elastic;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.Adres;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.Straatnaam;
    using Convertors;
    using Infrastructure.Elastic.Search;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.Extensions.Options;
    using StreetName;
    using StreetNameStatus = Consumer.Read.StreetName.Projections.StreetNameStatus;

    public sealed class AddressSearchHandler : IRequestHandler<AddressSearchRequest, AddressSearchV3Response>
    {
        private readonly IAddressApiSearchElasticsearchClient _addressApiSearchElasticsearchClient;
        private readonly IAddressApiStreetNameElasticsearchClient _addressApiStreetNameElasticsearchClient;
        private readonly IMunicipalityCache _municipalityCache;
        private readonly QueryParser _queryParser;
        private readonly ResponseOptionsV3 _responseOptions;

        public AddressSearchHandler(
            IAddressApiSearchElasticsearchClient addressApiSearchElasticsearchClient,
            IAddressApiStreetNameElasticsearchClient addressApiStreetNameElasticsearchClient,
            IOptions<ResponseOptionsV3> responseOptions,
            IMunicipalityCache municipalityCache,
            QueryParser queryParser)
        {
            _addressApiSearchElasticsearchClient = addressApiSearchElasticsearchClient;
            _addressApiStreetNameElasticsearchClient = addressApiStreetNameElasticsearchClient;
            _municipalityCache = municipalityCache;
            _queryParser = queryParser;
            _responseOptions = responseOptions.Value;
        }

        public async Task<AddressSearchV3Response> Handle(AddressSearchRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Filtering.Filter.Query))
            {
                return new AddressSearchV3Response([]);
            }

            if (!TryGetValidNisCode(request.Filtering.Filter, out var nisCode))
            {
                return new AddressSearchV3Response([]);
            }

            var pagination = (PaginationRequest)request.Pagination;
            if (pagination.Limit > 50)
                pagination = new PaginationRequest(pagination.Offset, 50);

            var query = request.Filtering.Filter.Query!;

            if (request.Filtering.Filter.ResultType == ResultType.Address)
            {
                return await SearchAddresses(request, query, nisCode, pagination);
            }

            var streetNameNisCode = nisCode;
            var streetNameQuery = query;
            if (_queryParser.TryExtractNisCodeViaPostalCode(ref streetNameQuery, out var queryNisCode))
            {
                if (request.Filtering.Filter.ResultType == ResultType.StreetName && nisCode is not null && nisCode != queryNisCode)
                {
                    return new AddressSearchV3Response([]);
                }

                if (nisCode is null || nisCode == queryNisCode)
                {
                    streetNameNisCode = queryNisCode;
                }
            }

            var streetNames = await SearchStreetNames(request, streetNameQuery, streetNameNisCode, pagination);
            if (request.Filtering.Filter.ResultType == ResultType.StreetName || streetNames.Results.Count >= pagination.Limit)
            {
                return streetNames;
            }

            var addresses = await SearchAddresses(request, query, nisCode, pagination);
            return new AddressSearchV3Response(
                streetNames.Results.Concat(addresses.Results).Take(pagination.Limit).ToList()
            );
        }

        private async Task<AddressSearchV3Response> SearchAddresses(
            AddressSearchRequest request,
            string query,
            string? nisCode,
            PaginationRequest pagination)
        {
            if (!TryParseValidAddressStatus(request, out var addressStatus))
            {
                return new AddressSearchV3Response([]);
            }

            var response = await _addressApiSearchElasticsearchClient.SearchAddresses(
                query,
                nisCode,
                addressStatus,
                pagination.Limit);

            var language = response.Language ?? Language.nl;

            return new AddressSearchV3Response(response.Addresses
                .AsEnumerable()
                .Select(x => new AddressSearchItemV3(
                    OsloNamespaces.Adres.ToPuri(x.AddressPersistentLocalId.ToString()),
                    x.AddressPersistentLocalId.ToString(),
                    new Uri(string.Format(_responseOptions.DetailUrl, x.AddressPersistentLocalId)),
                    x.FullAddress.FirstOrDefault(name => name.Language == language)?.Spelling ?? x.FullAddress.First().Spelling))
                .Take(pagination.Limit)
                .ToList());
        }

        private async Task<AddressSearchV3Response> SearchStreetNames(
            AddressSearchRequest request,
            string query,
            string? nisCode,
            PaginationRequest pagination)
        {
            if (!TryParseValidStreetNameStatus(request, out var streetNameStatus))
            {
                return new AddressSearchV3Response([]);
            }

            var streetNameResponse = await _addressApiStreetNameElasticsearchClient
                .SearchStreetNames(query, nisCode, streetNameStatus, pagination.Limit);

            var streetNameLanguage = streetNameResponse.Language ?? Language.nl;

            return new AddressSearchV3Response(streetNameResponse.StreetNames
                .AsEnumerable()
                .Select(x => new AddressSearchItemV3(
                    OsloNamespaces.StraatNaam.ToPuri(x.StreetNamePersistentLocalId.ToString()),
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
                return _municipalityCache.NisCodeExists(nisCode);
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

        private static bool TryParseValidAddressStatus(AddressSearchRequest request, out AddressStatus? addressStatus)
        {
            addressStatus = null;

            if (string.IsNullOrWhiteSpace(request.Filtering.Filter.Status))
            {
                return true;
            }

            if (Enum.TryParse<AdresStatusValue>(request.Filtering.Filter.Status, true, out var status))
            {
                addressStatus = status.ConvertFromAdresStatus();
                return true;
            }

            return false;
        }

        private bool TryParseValidStreetNameStatus(AddressSearchRequest request, out StreetNameStatus? streetNameStatus)
        {
            streetNameStatus = null;

            if (string.IsNullOrWhiteSpace(request.Filtering.Filter.Status))
            {
                return true;
            }

            if (Enum.TryParse<StraatnaamStatusValue>(request.Filtering.Filter.Status, true, out var straatNaamStatus))
            {
                streetNameStatus = Map(straatNaamStatus);
                return true;
            }

            return false;
        }

        private StreetNameStatus Map(StraatnaamStatusValue straatnaamStatus)
        {
            return straatnaamStatus switch
            {
                StraatnaamStatusValue.Voorgesteld => StreetNameStatus.Proposed,
                StraatnaamStatusValue.InGebruik => StreetNameStatus.Current,
                StraatnaamStatusValue.Gehistoreerd => StreetNameStatus.Retired,
                StraatnaamStatusValue.Afgekeurd => StreetNameStatus.Rejected,
                _ => throw new ArgumentOutOfRangeException(nameof(straatnaamStatus), straatnaamStatus, null)
            };
        }
    }
}
