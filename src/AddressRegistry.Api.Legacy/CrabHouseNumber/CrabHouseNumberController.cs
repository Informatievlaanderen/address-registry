namespace AddressRegistry.Api.Legacy.CrabHouseNumber
{
    using System.Threading;
    using System.Threading.Tasks;
    using Address.Count;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Filters;
    using ProblemDetails = Be.Vlaanderen.Basisregisters.BasicApiProblem.ProblemDetails;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [ApiRoute("crabhuisnummers")]
    [ApiExplorerSettings(GroupName = "CrabHuisnummers")]
    public class CrabHouseNumberController : ApiController
    {
        private readonly IMediator _mediator;

        public CrabHouseNumberController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(CrabHouseNumberAddressListResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(CrabHouseNumberListResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Get(CancellationToken cancellationToken = default)
        {
            var sorting = Request.ExtractSortingRequest();
            var pagination = Request.ExtractPaginationRequest();
            var filtering = Request.ExtractFilteringRequest<CrabHouseNumberAddressFilter>();

            var result = await _mediator.Send(new CrabHouseNumberAddressRequest(filtering, sorting, pagination), cancellationToken);

            Response.AddPaginationResponse(result.Pagination);
            Response.AddSortingResponse(result.Sorting);

            return Ok(result);
        }

        /// <summary>
        /// Vraag het totaal aantal crab huisnummers op.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als de opvraging van het totaal aantal gelukt is.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet("totaal-aantal")]
        [ProducesResponseType(typeof(TotaalAantalResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(TotalCountResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Count(CancellationToken cancellationToken = default)
        {
            return Ok(await _mediator.Send(new CrabHouseNumberCountRequest(), cancellationToken));
        }
    }
}
