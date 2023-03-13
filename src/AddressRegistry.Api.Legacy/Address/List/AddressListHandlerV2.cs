namespace AddressRegistry.Api.Legacy.Address.List
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Convertors;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;

    public sealed class AddressListHandlerV2 : IRequestHandler<AddressListRequest, AddressListResponse>
    {
        private readonly AddressQueryContext _addressQueryContext;
        private readonly IOptions<ResponseOptions> _responseOptions;

        public AddressListHandlerV2(
            AddressQueryContext addressQueryContext,
            IOptions<ResponseOptions> responseOptions)
        {
            _addressQueryContext = addressQueryContext;
            _responseOptions = responseOptions;
        }

        public async Task<AddressListResponse> Handle(AddressListRequest request, CancellationToken cancellationToken)
        {
            var pagedAddressesV2 = new AddressListQueryV2(_addressQueryContext)
                .Fetch(request.Filtering, request.Sorting, request.Pagination);

            var addressesV2 = await pagedAddressesV2.Items
                .ToListAsync(cancellationToken);

            var addressListItemResponsesV2 = addressesV2
                .Select(address => new AddressListItemResponse(
                    address.AddressPersistentLocalId,
                    _responseOptions.Value.Naamruimte,
                    _responseOptions.Value.DetailUrl,
                    address.HouseNumber,
                    address.BoxNumber,
                    AddressMapper.GetVolledigAdres(address),
                    address.Status.ConvertFromAddressStatus(),
                    address.VersionTimestampAsInstant.ToBelgianDateTimeOffset()))
                .ToList();

            return new AddressListResponse
            {
                Adressen = addressListItemResponsesV2,
                Volgende = pagedAddressesV2.PaginationInfo.BuildNextUri(addressListItemResponsesV2.Count, _responseOptions.Value.VolgendeUrl),
                Sorting = pagedAddressesV2.Sorting,
                Pagination = pagedAddressesV2.PaginationInfo
            };
        }
    }
}
