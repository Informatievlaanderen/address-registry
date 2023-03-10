namespace AddressRegistry.Api.Oslo.AddressMatch
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Requests;
    using Responses;
    using Swashbuckle.AspNetCore.Filters;
    using ProblemDetails = Be.Vlaanderen.Basisregisters.BasicApiProblem.ProblemDetails;

    [ApiVersion("2.0")]
    [AdvertiseApiVersions("2.0")]
    [ApiRoute("adresmatch")]
    [ApiExplorerSettings(GroupName = "AdresMatch")]
    public class AddressMatchController : ApiController
    {
        private readonly IMediator _mediator;

        public AddressMatchController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(AddressMatchCollection), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(AddressMatchResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(ValidationErrorResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Get(
            [FromQuery] AddressMatchRequest addressMatchRequest,
            CancellationToken cancellationToken = default)
        {
            await new AddressMatchRequestValidator()
                .ValidateAndThrowAsync(addressMatchRequest, cancellationToken: cancellationToken);

            var result = await _mediator.Send(addressMatchRequest, cancellationToken);

            return Ok(result);
        }
    }
}
