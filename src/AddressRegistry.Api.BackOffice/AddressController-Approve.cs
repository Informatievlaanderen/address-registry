namespace AddressRegistry.Api.BackOffice
{
    using System.Collections.Generic;
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
    using FluentValidation;
    using FluentValidation.Results;
    using Handlers.Sqs.Requests;
    using Infrastructure;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using StreetName;
    using StreetName.Exceptions;
    using Swashbuckle.AspNetCore.Filters;

    public partial class AddressController
    {
        /// <summary>
        /// Keur een adres goed.
        /// </summary>
        /// <param name="backOfficeContext"></param>
        /// <param name="request"></param>
        /// <param name="validator"></param>
        /// <param name="ifMatchHeaderValidator"></param>
        /// <param name="ifMatchHeaderValue"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="202">Aanvraag tot goedkeuring wordt reeds verwerkt.</response>
        /// <response code="204">Als het adres goedgekeurd is.</response>
        /// <response code="409">Als de adres status niet 'voorgesteld' is.</response>
        /// <response code="412">Als de If-Match header niet overeenkomt met de laatste ETag.</response>
        /// <returns></returns>
        [HttpPost("{persistentLocalId}/acties/goedkeuren")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status412PreconditionFailed)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Approve(
            [FromServices] BackOfficeContext backOfficeContext,
            [FromServices] IValidator<AddressApproveRequest> validator,
            [FromServices] IIfMatchHeaderValidator ifMatchHeaderValidator,
            [FromRoute] AddressApproveRequest request,
            [FromHeader(Name = "If-Match")] string? ifMatchHeaderValue,
            CancellationToken cancellationToken = default)
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken);

            var addressPersistentLocalId =
                new AddressPersistentLocalId(new PersistentLocalId(request.PersistentLocalId));

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

                if (_useSqsToggle.FeatureEnabled)
                {
                    var sqsRequest = new ApproveAddressSqsRequest
                    {
                        Request = request,
                        IfMatchHeaderValue = ifMatchHeaderValue,
                        Metadata = GetMetadata(),
                        ProvenanceData = new ProvenanceData(CreateFakeProvenance())
                    };
                    var sqsResult = await _mediator.Send(sqsRequest, cancellationToken);

                    return Accepted(sqsResult);
                }

                request.Metadata = GetMetadata();
                var response = await _mediator.Send(request, cancellationToken);

                return new NoContentWithETagResult(response.ETag);
            }
            catch (AggregateIdIsNotFoundException)
            {
                throw new ApiException(ValidationErrors.Common.AddressNotFound.Message, StatusCodes.Status404NotFound);
            }
            catch (IdempotencyException)
            {
                return Accepted();
            }
            catch (AggregateNotFoundException)
            {
                throw new ApiException(ValidationErrors.Common.AddressNotFound.Message, StatusCodes.Status404NotFound);
            }
            catch (DomainException exception)
            {
                throw exception switch
                {
                    StreetNameHasInvalidStatusException _ =>
                        CreateValidationException(
                            ValidationErrors.Common.StreetNameIsNotActive.Code,
                            string.Empty,
                            ValidationErrors.Common.StreetNameIsNotActive.Message),

                    StreetNameIsRemovedException _ =>
                        CreateValidationException(
                            ValidationErrors.Common.StreetNameInvalid.Code,
                            string.Empty,
                            ValidationErrors.Common.StreetNameInvalid.Message(streetNamePersistentLocalId)),

                    AddressIsNotFoundException => new ApiException(ValidationErrors.Common.AddressNotFound.Message, StatusCodes.Status404NotFound),
                    AddressIsRemovedException => new ApiException(ValidationErrors.Common.AddressRemoved.Message, StatusCodes.Status410Gone),
                    AddressHasInvalidStatusException => CreateValidationException(
                        ValidationErrors.ApproveAddress.AddressInvalidStatus.Code,
                        string.Empty,
                        ValidationErrors.ApproveAddress.AddressInvalidStatus.Message),
                    ParentAddressHasInvalidStatusException => CreateValidationException(
                        ValidationErrors.ApproveAddress.ParentInvalidStatus.Code,
                        string.Empty,
                        ValidationErrors.ApproveAddress.ParentInvalidStatus.Message),

                    _ => new ValidationException(new List<ValidationFailure>
                        { new ValidationFailure(string.Empty, exception.Message) })
                };
            }
        }
    }
}
