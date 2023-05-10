namespace AddressRegistry.Api.BackOffice
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Requests;
    using Abstractions.SqsRequests;
    using Abstractions.Validation;
    using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using FluentValidation;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Filters;

    public partial class AddressController
    {
        /// <summary>
        /// Stel een adres voor.
        /// </summary>
        /// <param name="validator"></param>
        /// <param name="proposeAddressSqsRequestFactory"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="202">Als het adres reeds voorgesteld is.</response>
        /// <returns></returns>
        [HttpPost("acties/voorstellen")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerRequestExample(typeof(ProposeAddressRequest), typeof(ProposeAddressRequestExamples))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyNames.Adres.DecentraleBijwerker)]
        public async Task<IActionResult> Propose(
            [FromServices] IValidator<ProposeAddressRequest> validator,
            [FromServices] ProposeAddressSqsRequestFactory proposeAddressSqsRequestFactory,
            [FromBody] ProposeAddressRequest request,
            CancellationToken cancellationToken = default)
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken);

            try
            {
                var sqsRequest = proposeAddressSqsRequestFactory.Create(request, GetMetadata(), new ProvenanceData(CreateProvenance(Modification.Insert)));
                var sqsResult = await _mediator.Send(sqsRequest, cancellationToken);

                return Accepted(sqsResult);
            }
            catch (AggregateNotFoundException)
            {
                throw CreateValidationException(
                    ValidationErrors.Common.StreetNameInvalid.Code,
                    nameof(request.StraatNaamId),
                    ValidationErrors.Common.StreetNameInvalid.Message(request.StraatNaamId));
            }
        }
    }
}
