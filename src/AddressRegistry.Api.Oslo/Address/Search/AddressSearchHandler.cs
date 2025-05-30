﻿namespace AddressRegistry.Api.Oslo.Address.Search
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
    using Infrastructure.Elastic.Search;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.Extensions.Options;
    using StreetName;
    using StreetNameStatus = Consumer.Read.StreetName.Projections.StreetNameStatus;

    public sealed class AddressSearchHandler : IRequestHandler<AddressSearchRequest, AddressSearchResponse>
    {
        private readonly IAddressApiSearchElasticsearchClient _addressApiSearchElasticsearchClient;
        private readonly IAddressApiStreetNameElasticsearchClient _addressApiStreetNameElasticsearchClient;
        private readonly IMunicipalityCache _municipalityCache;
        private readonly QueryParser _queryParser;
        private readonly ResponseOptions _responseOptions;

        public AddressSearchHandler(
            IAddressApiSearchElasticsearchClient addressApiSearchElasticsearchClient,
            IAddressApiStreetNameElasticsearchClient addressApiStreetNameElasticsearchClient,
            IOptions<ResponseOptions> responseOptions,
            IMunicipalityCache municipalityCache,
            QueryParser queryParser)
        {
            _addressApiSearchElasticsearchClient = addressApiSearchElasticsearchClient;
            _addressApiStreetNameElasticsearchClient = addressApiStreetNameElasticsearchClient;
            _municipalityCache = municipalityCache;
            _queryParser = queryParser;
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
                    return new AddressSearchResponse([]);
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
            return new AddressSearchResponse(
                streetNames.Results.Concat(addresses.Results).Take(pagination.Limit).ToList()
            );
        }

        private async Task<AddressSearchResponse> SearchAddresses(
            AddressSearchRequest request,
            string query,
            string? nisCode,
            PaginationRequest pagination)
        {
            if (!TryParseValidAddressStatus(request, out var addressStatus))
            {
                return new AddressSearchResponse([]);
            }

            var response = await _addressApiSearchElasticsearchClient.SearchAddresses(
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

        private async Task<AddressSearchResponse> SearchStreetNames(
            AddressSearchRequest request,
            string query,
            string? nisCode,
            PaginationRequest pagination)
        {
            if (!TryParseValidStreetNameStatus(request, out var streetNameStatus))
            {
                return new AddressSearchResponse([]);
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

            if (Enum.TryParse<AdresStatus>(request.Filtering.Filter.Status, true, out var status))
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

            if (Enum.TryParse<StraatnaamStatus>(request.Filtering.Filter.Status, true, out var straatNaamStatus))
            {
                streetNameStatus = Map(straatNaamStatus);
                return true;
            }

            return false;
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
