namespace AddressRegistry.Api.Oslo.Address
{
    using System.Net.Mime;
    using System.Threading;
    using System.Threading.Tasks;
    using Asp.Versioning;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Count;
    using Detail;
    using List;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Search;
    using Swashbuckle.AspNetCore.Filters;
    using Sync;
    using ProblemDetails = Be.Vlaanderen.Basisregisters.BasicApiProblem.ProblemDetails;

    [ApiVersion("2.0")]
    [AdvertiseApiVersions("2.0")]
    [ApiRoute("adressen")]
    [ApiExplorerSettings(GroupName = "Adressen")]
    public class AddressController : ApiController
    {
        private readonly IMediator _mediator;

        public AddressController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Vraag een adres op.
        /// </summary>
        /// <param name="persistentLocalId">Identificator van het adres.</param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als het adres gevonden is.</response>
        /// <response code="404">Als het adres niet gevonden kan worden.</response>
        /// <response code="410">Als het adres verwijderd is.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet("{persistentLocalId}")]
        [Produces(AcceptTypes.JsonLd)]
        [ProducesResponseType(typeof(AddressDetailOsloResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status410Gone)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(AddressDetailOsloResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(AddressNotFoundResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status410Gone, typeof(AddressGoneResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Get(
            [FromRoute] int persistentLocalId,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new AddressDetailOsloRequest(persistentLocalId), cancellationToken);

            return string.IsNullOrWhiteSpace(result.LastEventHash)
                ? Ok(result)
                : new OkWithLastObservedPositionAsETagResult(result, result.LastEventHash);
        }

        /// <summary>
        /// Vraag een lijst met adressen op.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als de opvraging van een lijst met adressen gelukt is.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet]
        [Produces(AcceptTypes.JsonLd)]
        [ProducesResponseType(typeof(AddressListOsloResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(AddressListOsloResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> List(CancellationToken cancellationToken = default)
        {
            var filtering = Request.ExtractFilteringRequest<AddressFilter>();
            var sorting = Request.ExtractSortingRequest();
            var pagination = Request.ExtractPaginationRequest();

            var result = await _mediator.Send(new AddressListOsloRequest(filtering, sorting, pagination), cancellationToken);

            Response.AddPaginationResponse(result.Pagination);
            Response.AddSortingResponse(result.Sorting);

            return Ok(result);
        }

        /// <summary>
        /// Zoek straatnamen / adressen.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als de opvraging van de zoekopdracht gelukt is.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet("zoeken")]
        [Produces(AcceptTypes.Json)]
        [ProducesResponseType(typeof(AddressSearchResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(AddressSearchResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Search(CancellationToken cancellationToken = default)
        {
            var filtering = Request.ExtractFilteringRequest<AddressSearchFilter>();
            var pagination = Request.ExtractPaginationRequest();

            if(!filtering.ShouldFilter)
                return Ok(new AddressSearchResponse([]));

            var result = await _mediator.Send(new AddressSearchRequest(filtering, pagination), cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Vraag het totaal aantal adressen op.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als de opvraging van het totaal aantal gelukt is.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet("totaal-aantal")]
        [Produces(AcceptTypes.JsonLd)]
        [ProducesResponseType(typeof(TotaalAantalResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(TotalCountOsloResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Count(CancellationToken cancellationToken = default)
        {
            var filtering = Request.ExtractFilteringRequest<AddressFilter>();
            var sorting = Request.ExtractSortingRequest();
            var pagination = new NoPaginationRequest();

            var result = await _mediator.Send(new AddressCountRequest(filtering, sorting, pagination), cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Vraag een lijst met wijzigingen van adressen op.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("sync")]
        [Produces("text/xml")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(AddressSyndicationResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Sync(CancellationToken cancellationToken = default)
        {
            var filtering = Request.ExtractFilteringRequest<AddressSyndicationFilter>();
            var sorting = Request.ExtractSortingRequest();
            var pagination = Request.ExtractPaginationRequest();

            var result =
                await _mediator.Send(new SyndicationRequest(filtering, sorting, pagination), cancellationToken);

            return new ContentResult
            {
                Content = result.Content,
                ContentType = MediaTypeNames.Text.Xml,
                StatusCode = StatusCodes.Status200OK
            };
        }
    }
}
