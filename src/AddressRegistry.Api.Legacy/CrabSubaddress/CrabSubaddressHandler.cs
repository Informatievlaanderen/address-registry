namespace AddressRegistry.Api.Legacy.CrabSubaddress
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
    using Projections.Legacy;
    using Projections.Syndication;

    public sealed record CrabSubaddressRequest(
        FilteringHeader<CrabSubaddressAddressFilter> Filtering,
        SortingHeader Sorting,
        IPaginationRequest Pagination) : IRequest<CrabSubAddressListResponse>;

    public sealed class CrabSubaddressHandler : IRequestHandler<CrabSubaddressRequest, CrabSubAddressListResponse>
    {
        private readonly LegacyContext _legacyContext;
        private readonly SyndicationContext _syndicationContext;
        private readonly IOptions<ResponseOptions> _responseOptions;

        public CrabSubaddressHandler(
            LegacyContext legacyContext,
            SyndicationContext syndicationContext,
            IOptions<ResponseOptions> responseOptions)
        {
            _legacyContext = legacyContext;
            _syndicationContext = syndicationContext;
            _responseOptions = responseOptions;
        }

        public async Task<CrabSubAddressListResponse> Handle(CrabSubaddressRequest request, CancellationToken cancellationToken)
        {
            var pagedAddresses = new CrabSubaddressQuery(_legacyContext)
                .Fetch(request.Filtering, request.Sorting, request.Pagination);

            var addresses = await pagedAddresses
               .Items
               .ToListAsync(cancellationToken);

            var streetNameIds = addresses
                .Select(x => x.StreetNameId)
                .ToList();

            var streetNames = await _syndicationContext
                .StreetNameLatestItems
                .AsNoTracking()
                .Where(x => streetNameIds.Contains(x.StreetNameId))
                .ToListAsync(cancellationToken);

            var nisCodes = streetNames
                .Select(x => x.NisCode)
                .ToList();

            var municipalities = await _syndicationContext
                .MunicipalityLatestItems
                .AsNoTracking()
                .Where(x => nisCodes.Contains(x.NisCode))
                .ToListAsync(cancellationToken);

            var addressListItemResponses = addresses
                .Select(a =>
                {
                    var streetName = streetNames.SingleOrDefault(x => x.StreetNameId == a.StreetNameId);
                    var municipality = municipalities.SingleOrDefault(x => x.NisCode == streetName.NisCode);
                    return new CrabSubAddressListItem(
                        a.SubaddressId.Value,
                        a.PersistentLocalId.Value,
                        _responseOptions.Value.Naamruimte,
                        _responseOptions.Value.DetailUrl,
                        a.HouseNumber,
                        a.BoxNumber,
                        AddressMapper.GetVolledigAdres(a.HouseNumber, a.BoxNumber, a.PostalCode, streetName, municipality),
                        a.VersionTimestamp.ToBelgianDateTimeOffset(),
                        a.IsComplete);
                })
                .ToList();

            return new CrabSubAddressListResponse
            {
                Addresses = addressListItemResponses,
                Volgende = pagedAddresses.PaginationInfo.BuildNextUri(addressListItemResponses.Count, _responseOptions.Value.CrabSubadressenVolgendeUrl),
                Sorting = pagedAddresses.Sorting,
                Pagination = pagedAddresses.PaginationInfo
            };
        }
    }
}
