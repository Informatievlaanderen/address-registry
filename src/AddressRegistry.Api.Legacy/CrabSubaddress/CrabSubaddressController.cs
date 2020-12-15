namespace AddressRegistry.Api.Legacy.CrabSubaddress
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
    using Address.Responses;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Microsoft.AspNetCore.Http;
    using Swashbuckle.AspNetCore.Filters;
    using ProblemDetails = Be.Vlaanderen.Basisregisters.BasicApiProblem.ProblemDetails;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [ApiRoute("crabsubadressen")]
    [ApiExplorerSettings(GroupName = "CrabSubadressen")]
    public class CrabSubaddressController : ApiController
    {
        [HttpGet]
        [ProducesResponseType(typeof(CrabSubAddressListResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(CrabSubaddressListResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Get(
            [FromServices] LegacyContext context,
            [FromServices] SyndicationContext syndicationContext,
            [FromServices] IOptions<ResponseOptions> responseOptions,
            CancellationToken cancellationToken = default)
        {
            var sorting = Request.ExtractSortingRequest();
            var pagination = Request.ExtractPaginationRequest();
            var filtering = Request.ExtractFilteringRequest<CrabSubaddressAddressFilter>();

            var pagedAddresses = new CrabSubaddressQuery(context)
                .Fetch(filtering, sorting, pagination);

            Response.AddPagedQueryResultHeaders(pagedAddresses);

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
                    return new CrabSubAddressListItem(
                        a.SubaddressId.Value,
                        a.PersistentLocalId.Value,
                        responseOptions.Value.Naamruimte,
                        responseOptions.Value.DetailUrl,
                        a.HouseNumber,
                        a.BoxNumber,
                        AddressMapper.GetVolledigAdres(a.HouseNumber, a.BoxNumber, a.PostalCode, streetName, municipality),
                        a.VersionTimestamp.ToBelgianDateTimeOffset(),
                        a.IsComplete);
                })
                .ToList();

            return Ok(new CrabSubAddressListResponse
            {
                Addresses = addressListItemResponses,
                Volgende = pagedAddresses.PaginationInfo.BuildNextUri(responseOptions.Value.CrabSubadressenVolgendeUrl)
            });
        }

        /// <summary>
        /// Vraag het totaal aantal crab subadressen op.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als de opvraging van het totaal aantal gelukt is.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet("totaal-aantal")]
        [ProducesResponseType(typeof(TotaalAantalResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(TotalCountResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Count(
            [FromServices] LegacyContext context,
            CancellationToken cancellationToken = default)
        {
            return Ok(
                new TotaalAantalResponse
                {
                    Aantal = new CrabSubaddressQuery(context).Count()
                });
        }
    }
}
