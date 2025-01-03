﻿namespace AddressRegistry.Api.Oslo.Address.List
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Infrastructure.Elastic;
    using Infrastructure.Elastic.List;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.Extensions.Options;

    public class AddressListOsloElasticHandler : IRequestHandler<AddressListOsloRequest, AddressListOsloResponse>
    {
        private readonly IAddressApiListElasticsearchClient _addressApiListElasticsearchClient;
        private readonly ResponseOptions _responseOptions;

        public AddressListOsloElasticHandler(
            IAddressApiListElasticsearchClient addressApiListElasticsearchClient,
            IOptions<ResponseOptions> responseOptions)
        {
            _addressApiListElasticsearchClient = addressApiListElasticsearchClient;
            _responseOptions = responseOptions.Value;
        }

        public async Task<AddressListOsloResponse> Handle(AddressListOsloRequest request, CancellationToken cancellationToken)
        {
            var pagination = (PaginationRequest)request.Pagination;
            var filtering = request.Filtering;

            var addressListResult = await _addressApiListElasticsearchClient.ListAddresses(
                filtering.Filter?.StreetNameId,
                filtering.Filter?.StreetName,
                filtering.Filter?.HomonymAddition,
                filtering.Filter?.HouseNumber,
                filtering.Filter?.BoxNumber,
                filtering.Filter?.PostalCode,
                filtering.Filter?.NisCode,
                filtering.Filter?.MunicipalityName,
                filtering.Filter?.Status,
                from: pagination.Offset,
                size: pagination.Limit);

            var addressListItemResponsesV2 = addressListResult.Addresses
                .Select(address => new AddressListItemOsloResponse(
                    address.AddressPersistentLocalId,
                    _responseOptions.Naamruimte,
                    _responseOptions.DetailUrl,
                    address.HouseNumber,
                    address.BoxNumber,
                    AddressMapper.GetVolledigAdres(address),
                    AddressMapper.ConvertFromAddressStatus(address.Status),
                    address.VersionTimestamp))
                .ToList();

            var paginationInfo = new PaginationInfo(pagination.Offset, pagination.Limit, pagination.Limit > 0);
            return new AddressListOsloResponse
            {
                Adressen = addressListItemResponsesV2,
                Volgende = paginationInfo.BuildNextUri(addressListItemResponsesV2.Count, _responseOptions.VolgendeUrl),
                Context = _responseOptions.ContextUrlList,
                Sorting = request.Sorting,
                Pagination = paginationInfo
            };
        }
    }
}
