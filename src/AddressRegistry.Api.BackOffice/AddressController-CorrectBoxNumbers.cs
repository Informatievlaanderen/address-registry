namespace AddressRegistry.Api.BackOffice
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Requests;
    using Abstractions.SqsRequests;
    using Abstractions.Validation;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using FluentValidation;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Filters;

    public partial class AddressController
    {
        /// <summary>
        /// Correctie van meerdere busnummers van een adres.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="validator"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="202">Aanvraag tot correctie adres busnummer wordt reeds verwerkt.</response>
        /// <response code="400">Als de adres status niet 'voorgesteld' of 'ingebruik' is.</response>
        /// <returns></returns>
        [HttpPost("/acties/corrigeren/busnummers")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyNames.Adres.DecentraleBijwerker)]
        public async Task<IActionResult> CorrectBoxNumbers(
            [FromServices] IValidator<CorrectAddressBoxNumbersRequest> validator,
            [FromBody] CorrectAddressBoxNumbersRequest request,
            CancellationToken cancellationToken = default)
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken);

            try
            {
                var sqsRequest = new CorrectAddressBoxNumbersSqsRequest
                {
                    Request = request,
                    Metadata = GetMetadata(),
                    ProvenanceData = new ProvenanceData(CreateProvenance(Modification.Update))
                };
                var sqsResult = await _mediator.Send(sqsRequest, cancellationToken);

                return Accepted(sqsResult);
            }
            catch (AggregateIdIsNotFoundException)
            {
                throw new ApiException(ValidationErrors.Common.AddressNotFound.Message, StatusCodes.Status404NotFound);
            }
        }
    }
}
