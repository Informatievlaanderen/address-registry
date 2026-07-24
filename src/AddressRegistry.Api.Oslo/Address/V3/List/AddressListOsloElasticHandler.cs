namespace AddressRegistry.Api.Oslo.Address.V3.List
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Infrastructure.Elastic.List;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.Extensions.Options;

    public class AddressListOsloElasticHandler : IRequestHandler<AddressListOsloRequest, AddressListOsloV3Response>
    {
        private readonly IAddressApiListElasticsearchClient _addressApiListElasticsearchClient;
        private readonly ResponseOptionsV3 _responseOptions;

        public AddressListOsloElasticHandler(
            IAddressApiListElasticsearchClient addressApiListElasticsearchClient,
            IOptions<ResponseOptionsV3> responseOptions)
        {
            _addressApiListElasticsearchClient = addressApiListElasticsearchClient;
            _responseOptions = responseOptions.Value;
        }

        public async Task<AddressListOsloV3Response> Handle(AddressListOsloRequest request, CancellationToken cancellationToken)
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
                .Select(address => new AddressListItemOsloV3Response(
                    address.AddressPersistentLocalId,
                    _responseOptions.DetailUrl,
                    address.HouseNumber,
                    address.BoxNumber,
                    AddressMapper.GetVolledigAdres(address),
                    AddressMapper.ConvertFromAddressStatus(address.Status),
                    address.VersionTimestamp))
                .ToList();

            var paginationInfo = new PaginationInfo(pagination.Offset, pagination.Limit, pagination.Limit > 0);
            return new AddressListOsloV3Response
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
