namespace AddressRegistry.Api.BackOffice
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Abstractions.Requests;
    using Abstractions.Validation;
    using Address;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Handlers.Sqs.Requests;
    using Infrastructure;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using StreetName;
    using Swashbuckle.AspNetCore.Filters;

    public partial class AddressController
    {
        /// <summary>
        /// Corrigeer de deregularisatie van een adres.
        /// </summary>
        /// <param name="backOfficeContext"></param>
        /// <param name="ifMatchHeaderValidator"></param>
        /// <param name="ifMatchHeaderValue"></param>
        /// <param name="persistentLocalId"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="202">Aanvraag tot corrigeren deregularisatie wordt reeds verwerkt.</response>
        [HttpPost("{persistentLocalId}/acties/corrigeren/deregularisatie")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status412PreconditionFailed)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> CorrectDeregularization(
            [FromServices] BackOfficeContext backOfficeContext,
            [FromServices] IIfMatchHeaderValidator ifMatchHeaderValidator,
            [FromHeader(Name = "If-Match")] string? ifMatchHeaderValue,
            [FromRoute] int persistentLocalId,
            CancellationToken cancellationToken = default)
        {
            var addressPersistentLocalId =
                new AddressPersistentLocalId(new PersistentLocalId(persistentLocalId));

            var relation = backOfficeContext.AddressPersistentIdStreetNamePersistentIds
                .FirstOrDefault(x => x.AddressPersistentLocalId == addressPersistentLocalId);

            if (relation is null)
            {
                return NotFound();
            }

            var streetNamePersistentLocalId = new StreetNamePersistentLocalId(relation.StreetNamePersistentLocalId);

            try
            {
                // Check if user provided ETag is equal to the current Entity Tag
                if (!await ifMatchHeaderValidator.IsValid(ifMatchHeaderValue, streetNamePersistentLocalId, addressPersistentLocalId, cancellationToken))
                {
                    return new PreconditionFailedResult();
                }

                var sqsRequest = new CorrectAddressDeregularizationSqsRequest
                {
                    Request = new CorrectAddressDeregularizationBackOfficeRequest {PersistentLocalId = addressPersistentLocalId},
                    IfMatchHeaderValue = ifMatchHeaderValue,
                    Metadata = GetMetadata(),
                    ProvenanceData = new ProvenanceData(CreateFakeProvenance())
                };

                var sqsResult = await _mediator.Send(sqsRequest, cancellationToken);

                return Accepted(sqsResult);
            }
            catch (AggregateIdIsNotFoundException)
            {
                throw new ApiException(ValidationErrors.Common.StreetNameInvalid.Message(streetNamePersistentLocalId), StatusCodes.Status404NotFound);
            }
            catch (AggregateNotFoundException)
            {
                throw new ApiException(ValidationErrors.Common.StreetNameInvalid.Message(streetNamePersistentLocalId), StatusCodes.Status404NotFound);
            }
        }
    }
}
