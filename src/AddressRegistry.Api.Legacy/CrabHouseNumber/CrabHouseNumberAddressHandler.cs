namespace AddressRegistry.Api.Legacy.CrabHouseNumber
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

    public sealed record CrabHouseNumberAddressRequest(
        FilteringHeader<CrabHouseNumberAddressFilter> Filtering,
        SortingHeader Sorting,
        IPaginationRequest Pagination) : IRequest<CrabHouseNumberAddressListResponse>;

    public sealed class CrabHouseNumberAddressHandler : IRequestHandler<CrabHouseNumberAddressRequest, CrabHouseNumberAddressListResponse>
    {
        private readonly LegacyContext _legacyContext;
        private readonly SyndicationContext _syndicationContext;
        private readonly IOptions<ResponseOptions> _responseOptions;

        public CrabHouseNumberAddressHandler(
            LegacyContext legacyContext,
            SyndicationContext syndicationContext,
            IOptions<ResponseOptions> responseOptions)
        {
            _legacyContext = legacyContext;
            _syndicationContext = syndicationContext;
            _responseOptions = responseOptions;
        }

        public async Task<CrabHouseNumberAddressListResponse> Handle(CrabHouseNumberAddressRequest request, CancellationToken cancellationToken)
        {
            var pagedAddresses = new CrabHouseNumberQuery(_legacyContext)
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
                    return new CrabHouseNumberAddressListItem(
                        a.HouseNumberId.Value,
                        a.PersistentLocalId.Value,
                        _responseOptions.Value.Naamruimte,
                        _responseOptions.Value.DetailUrl,
                        a.HouseNumber,
                        AddressMapper.GetVolledigAdres(a.HouseNumber, "", a.PostalCode, streetName, municipality),
                        a.VersionTimestamp.ToBelgianDateTimeOffset(),
                        a.IsComplete);
                })
                .ToList();

            return new CrabHouseNumberAddressListResponse
            {
                Addresses = addressListItemResponses,
                Volgende = pagedAddresses.PaginationInfo.BuildNextUri(addressListItemResponses.Count, _responseOptions.Value.CrabHuisnummersVolgendeUrl),
                Sorting = pagedAddresses.Sorting,
                Pagination = pagedAddresses.PaginationInfo
            };
        }
    }
}
