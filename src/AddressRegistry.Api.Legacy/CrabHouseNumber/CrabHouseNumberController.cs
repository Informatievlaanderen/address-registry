namespace AddressRegistry.Api.Legacy.CrabHouseNumber
{
    using Address;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Infrastructure.Options;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Projections.Legacy;
    using Projections.Syndication;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [ApiRoute("crabhuisnummers")]
    [ApiExplorerSettings(GroupName = "CrabHuisnummers")]
    public class CrabHouseNumberController : ApiController
    {
        public async Task<IActionResult> Get(
            [FromServices] LegacyContext context,
            [FromServices] SyndicationContext syndicationContext,
            [FromServices] IOptions<ResponseOptions> responseOptions,
            CancellationToken cancellationToken = default)
        {
            var sorting = Request.ExtractSortingRequest();
            var pagination = Request.ExtractPaginationRequest();
            var filtering = Request.ExtractFilteringRequest<CrabHouseNumberAddressFilter>();

            var pagedAddresses = new CrabHouseNumberQuery(context)
                .Fetch(filtering, sorting, pagination);

            Response.AddPaginationResponse(pagedAddresses.PaginationInfo);
            Response.AddSortingResponse(sorting.SortBy, sorting.SortOrder);

            var addresses = await pagedAddresses
                .Items
                .ToListAsync(cancellationToken);

            var streetNameIds = addresses
                .Select(x => x.StreetNameId)
                .ToList();

            var streetNames = await syndicationContext
                .StreetNameLatestItems
                .AsNoTracking()
                .Where(x => streetNameIds.Contains(x.StreetNameId))
                .ToListAsync(cancellationToken);

            var nisCodes = streetNames
                .Select(x => x.NisCode)
                .ToList();

            var municipalities = await syndicationContext
                .MunicipalityLatestItems
                .AsNoTracking()
                .Where(x => nisCodes.Contains(x.NisCode))
                .ToListAsync(cancellationToken);

            var addressListItemResponses = addresses
                .Select(a =>
                {
                    var streetName = streetNames.SingleOrDefault(x => x.StreetNameId == a.StreetNameId);
                    var municipality = municipalities.SingleOrDefault(x => x.NisCode == streetName.NisCode);
                    return new CrabAddressListItem(
                        a.HouseNumberId.Value,
                        a.OsloId.Value,
                        responseOptions.Value.Naamruimte,
                        responseOptions.Value.DetailUrl,
                        a.HouseNumber,
                        AddressMapper.GetVolledigAdres(a.HouseNumber, "", a.PostalCode, streetName, municipality),
                        a.VersionTimestamp.ToBelgianDateTimeOffset(),
                        a.IsComplete);
                })
                .ToList();

            return Ok(new CrabAddressListResponse
            {
                Addresses = addressListItemResponses,
                TotaalAantal = pagedAddresses.PaginationInfo.TotalItems,
                Volgende = AddressController.BuildVolgendeUri(pagedAddresses.PaginationInfo, responseOptions.Value.CrabHuisnummersVolgendeUrl)
            });
        }
    }
}
