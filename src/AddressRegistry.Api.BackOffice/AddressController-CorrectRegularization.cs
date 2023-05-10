namespace AddressRegistry.Api.BackOffice
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Requests;
    using Abstractions.SqsRequests;
    using Abstractions.Validation;
    using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Infrastructure;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using StreetName;
    using Swashbuckle.AspNetCore.Filters;

    public partial class AddressController
    {
        /// <summary>
        /// Corrigeer de regularisatie van een adres.
        /// </summary>
        /// <param name="ifMatchHeaderValidator"></param>
        /// <param name="ifMatchHeaderValue"></param>
        /// <param name="persistentLocalId"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="202">Aanvraag tot corrigeren regularisatie wordt reeds verwerkt.</response>
        [HttpPost("{persistentLocalId}/acties/corrigeren/regularisatie")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status412PreconditionFailed)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyNames.Adres.DecentraleBijwerker)]
        public async Task<IActionResult> CorrectRegularization(
            [FromServices] IIfMatchHeaderValidator ifMatchHeaderValidator,
            [FromHeader(Name = "If-Match")] string? ifMatchHeaderValue,
            [FromRoute] int persistentLocalId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Check if user provided ETag is equal to the current Entity Tag
                if (!await ifMatchHeaderValidator.IsValid(ifMatchHeaderValue, new AddressPersistentLocalId(persistentLocalId), cancellationToken))
                {
                    return new PreconditionFailedResult();
                }

                var sqsRequest = new CorrectAddressRegularizationSqsRequest
                {
                    Request = new CorrectAddressRegularizationRequest {PersistentLocalId = persistentLocalId},
                    IfMatchHeaderValue = ifMatchHeaderValue,
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
            catch (AggregateNotFoundException)
            {
                throw new ApiException(ValidationErrors.Common.AddressNotFound.Message, StatusCodes.Status404NotFound);
            }
        }
    }
}
