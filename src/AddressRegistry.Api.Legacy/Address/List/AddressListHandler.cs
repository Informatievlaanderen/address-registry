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
    using Projections.Syndication.Municipality;

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
                .Select(a =>
                new
                {
                    a.PersistentLocalId,
                    a.StreetNameId,
                    a.HouseNumber,
                    a.BoxNumber,
                    a.PostalCode,
                    a.Status,
                    a.VersionTimestamp
                })
                .ToListAsync(cancellationToken);

            var streetNameIds = addresses
                .Select(x => x.StreetNameId)
                .Distinct()
                .ToList();

            var streetNames = await _addressQueryContext
                .StreetNameLatestItems
                .Where(x => streetNameIds.Contains(x.StreetNameId))
                .ToListAsync(cancellationToken);

            var nisCodes = streetNames
                .Select(x => x.NisCode)
                .Distinct()
                .ToList();

            var municipalities = await _addressQueryContext
                .MunicipalityLatestItems
                .Where(x => nisCodes.Contains(x.NisCode))
                .ToListAsync(cancellationToken);

            var addressListItemResponses = addresses
                .Select(a =>
                {
                    var streetName = streetNames.SingleOrDefault(x => x.StreetNameId == a.StreetNameId);
                    MunicipalityLatestItem? municipality = null;
                    if (streetName != null)
                    {
                        municipality = municipalities.SingleOrDefault(x => x.NisCode == streetName.NisCode);
                    }

                    return new AddressListItemResponse(
                        a.PersistentLocalId,
                        _responseOptions.Value.Naamruimte,
                        _responseOptions.Value.DetailUrl,
                        a.HouseNumber,
                        a.BoxNumber,
                        AddressMapper.GetVolledigAdres(a.HouseNumber, a.BoxNumber, a.PostalCode, streetName, municipality),
                        AddressMapper.ConvertFromAddressStatus(a.Status),
                        a.VersionTimestamp.ToBelgianDateTimeOffset());
                })
                .ToList();

            return new AddressListResponse
            {
                Adressen = addressListItemResponses,
                Volgende = pagedAddresses.PaginationInfo.BuildNextUri(_responseOptions.Value.VolgendeUrl),
                Sorting = pagedAddresses.Sorting,
                Pagination = pagedAddresses.PaginationInfo
            };
        }
    }
}
