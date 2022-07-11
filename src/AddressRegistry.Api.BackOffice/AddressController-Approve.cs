namespace AddressRegistry.Api.BackOffice
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Abstractions.Exceptions;
    using Abstractions.Requests;
    using Address;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using FluentValidation;
    using FluentValidation.Results;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using StreetName;
    using StreetName.Exceptions;
    using Swashbuckle.AspNetCore.Filters;
    using Validators;

    public partial class AddressController
    {
        /// <summary>
        /// Keur een adres goed.
        /// </summary>
        /// <param name="backOfficeContext"></param>
        /// <param name="streetNameRepository"></param>
        /// <param name="request"></param>
        /// <param name="validator"></param>
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
            [FromServices] IStreetNames streetNameRepository,
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
                if (ifMatchHeaderValue is not null)
                {
                    var ifMatchTag = ifMatchHeaderValue.Trim();
                    var lastHash = await GetHash(
                        streetNameRepository,
                        streetNamePersistentLocalId,
                        addressPersistentLocalId,
                        cancellationToken);
                    var lastHashTag = new ETag(ETagType.Strong, lastHash);
                    if (ifMatchTag != lastHashTag.ToString())
                    {
                        return new PreconditionFailedResult();
                    }
                }

                request.Metadata = GetMetadata();
                var response = await _mediator.Send(request, cancellationToken);

                return new NoContentWithETagResult(response.LastEventHash);
            }
            catch (IdempotencyException)
            {
                return Accepted();
            }
            catch (AggregateNotFoundException)
            {
                throw new ApiException(ValidationErrorMessages.AddressNotFound, StatusCodes.Status404NotFound);
            }
            catch (DomainException exception)
            {
                throw exception switch
                {
                    StreetNameNotActiveException _ =>
                        CreateValidationException(
                            ValidationErrorCodes.StreetNameIsNotActive,
                            string.Empty,
                            ValidationErrorMessages.StreetNameIsNotActive),

                    StreetNameIsRemovedException _ =>
                        CreateValidationException(
                            ValidationErrorCodes.StreetNameInvalid,
                            string.Empty,
                            ValidationErrorMessages.StreetNameInvalid(streetNamePersistentLocalId)),

                    AddressNotFoundException => new ApiException(ValidationErrorMessages.AddressNotFound, StatusCodes.Status404NotFound),
                    AddressIsRemovedException => new ApiException(ValidationErrorMessages.AddressRemoved, StatusCodes.Status410Gone),
                    AddressCannotBeApprovedException => CreateValidationException(
                        ValidationErrorCodes.AddressCannotBeApproved,
                        string.Empty,
                        ValidationErrorMessages.AddressCannotBeApproved),

                    _ => new ValidationException(new List<ValidationFailure>
                        { new ValidationFailure(string.Empty, exception.Message) })
                };
            }
        }
    }
}
