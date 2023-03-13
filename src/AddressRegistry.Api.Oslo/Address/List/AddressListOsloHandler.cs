namespace AddressRegistry.Api.Oslo.Address.List
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;

    public sealed record AddressListOsloRequest(
        FilteringHeader<AddressFilter> Filtering,
        SortingHeader Sorting,
        IPaginationRequest Pagination) : IRequest<AddressListOsloResponse>;

    public sealed class AddressListOsloHandler : IRequestHandler<AddressListOsloRequest, AddressListOsloResponse>
    {
        private readonly AddressQueryContext _addressQueryContext;
        private readonly IOptions<ResponseOptions> _responseOptions;

        public AddressListOsloHandler(
            AddressQueryContext addressQueryContext,
            IOptions<ResponseOptions> responseOptions)
        {
            _addressQueryContext = addressQueryContext;
            _responseOptions = responseOptions;
        }

        public async Task<AddressListOsloResponse> Handle(AddressListOsloRequest request, CancellationToken cancellationToken)
        {
            var pagedAddresses = new AddressListOsloQuery(_addressQueryContext)
                .Fetch(request.Filtering, request.Sorting, request.Pagination);

            var addresses = await pagedAddresses.Items
                .ToListAsync(cancellationToken);

            var addressListItemResponses = addresses
                .Select(address => new AddressListItemOsloResponse(
                    address.PersistentLocalId,
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
                Adressen = addressListItemResponses,
                Volgende = pagedAddresses.PaginationInfo.BuildNextUri(addressListItemResponses.Count, _responseOptions.Value.VolgendeUrl),
                Context = _responseOptions.Value.ContextUrlList,
                Sorting = pagedAddresses.Sorting,
                Pagination = pagedAddresses.PaginationInfo
            };
        }
    }
}
