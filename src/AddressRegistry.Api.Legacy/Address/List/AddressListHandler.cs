namespace AddressRegistry.Api.Legacy.Address.List
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Address;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;

    public sealed record AddressListRequest(
        FilteringHeader<AddressFilter> Filtering,
        SortingHeader Sorting,
        IPaginationRequest Pagination) : IRequest<AddressListResponse>;

    public sealed class AddressListHandler : IRequestHandler<AddressListRequest, AddressListResponse>
    {
        private readonly AddressQueryContext _addressQueryContext;
        private readonly IOptions<ResponseOptions> _responseOptions;

        public AddressListHandler(
            AddressQueryContext addressQueryContext,
            IOptions<ResponseOptions> responseOptions)
        {
            _addressQueryContext = addressQueryContext;
            _responseOptions = responseOptions;
        }

        public async Task<AddressListResponse> Handle(AddressListRequest request, CancellationToken cancellationToken)
        {
            var pagedAddresses = new AddressListQuery(_addressQueryContext)
                .Fetch(request.Filtering, request.Sorting, request.Pagination);

            var addresses = await pagedAddresses.Items
                .ToListAsync(cancellationToken);

            var addressListItemResponses = addresses
                .Select(address => new AddressListItemResponse(
                    address.PersistentLocalId,
                    _responseOptions.Value.Naamruimte,
                    _responseOptions.Value.DetailUrl,
                    address.HouseNumber,
                    address.BoxNumber,
                    AddressMapper.GetVolledigAdres(address),
                    AddressMapper.ConvertFromAddressStatus(address.Status),
                    address.VersionTimestampAsInstant.ToBelgianDateTimeOffset()))
                .ToList();

            return new AddressListResponse
            {
                Adressen = addressListItemResponses,
                Volgende = pagedAddresses.PaginationInfo.BuildNextUri(addressListItemResponses.Count, _responseOptions.Value.VolgendeUrl),
                Sorting = pagedAddresses.Sorting,
                Pagination = pagedAddresses.PaginationInfo
            };
        }
    }
}
