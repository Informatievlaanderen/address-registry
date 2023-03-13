namespace AddressRegistry.Api.Oslo.Address.List
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;

    public sealed class AddressListOsloHandlerV2 : IRequestHandler<AddressListOsloRequest, AddressListOsloResponse>
    {
        private readonly AddressQueryContext _addressQueryContext;
        private readonly IOptions<ResponseOptions> _responseOptions;

        public AddressListOsloHandlerV2(
            AddressQueryContext addressQueryContext,
            IOptions<ResponseOptions> responseOptions)
        {
            _addressQueryContext = addressQueryContext;
            _responseOptions = responseOptions;
        }

        public async Task<AddressListOsloResponse> Handle(AddressListOsloRequest request, CancellationToken cancellationToken)
        {
            var pagedAddressesV2 = new AddressListOsloQueryV2(_addressQueryContext)
                .Fetch(request.Filtering, request.Sorting, request.Pagination);

            var addressesV2 = await pagedAddressesV2.Items
                .ToListAsync(cancellationToken);

            var addressListItemResponsesV2 = addressesV2
                .Select(address => new AddressListItemOsloResponse(
                    address.AddressPersistentLocalId,
                    _responseOptions.Value.Naamruimte,
                    _responseOptions.Value.DetailUrl,
                    address.HouseNumber,
                    address.BoxNumber,
                    AddressMapper.GetVolledigAdres(address),
                    AddressMapper.ConvertFromAddressStatus(address.Status),
                    address.VersionTimestampAsInstant.ToBelgianDateTimeOffset()))
                .ToList();

            return new AddressListOsloResponse
            {
                Adressen = addressListItemResponsesV2,
                Volgende = pagedAddressesV2.PaginationInfo.BuildNextUri(addressListItemResponsesV2.Count, _responseOptions.Value.VolgendeUrl),
                Context = _responseOptions.Value.ContextUrlList,
                Sorting = pagedAddressesV2.Sorting,
                Pagination = pagedAddressesV2.PaginationInfo
            };
        }
    }
}
