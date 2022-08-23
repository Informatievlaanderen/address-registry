namespace AddressRegistry.Api.BackOffice
{
    using System;
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
    using Infrastructure;
    using Infrastructure.Options;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using StreetName;
    using StreetName.Exceptions;
    using Swashbuckle.AspNetCore.Filters;
    using Validators;

    public partial class AddressController
    {
        /// <summary>
        /// Corrigeer een adrespositie.
        /// </summary>
        /// <param name="backOfficeContext"></param>
        /// <param name="ifMatchHeaderValidator"></param>
        /// <param name="options"></param>
        /// <param name="persistentLocalId"></param>
        /// <param name="request"></param>
        /// <param name="validator"></param>
        /// <param name="ifMatchHeaderValue"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="202">De aanvraag wordt reeds verwerkt.</response>
        /// <response code="400">Als de adres status niet 'voorgesteld' of 'ingebruik' is.</response>
        /// <response code="412">Als de If-Match header niet overeenkomt met de laatste ETag.</response>
        /// <returns></returns>
        [HttpPost("{persistentLocalId}/acties/corrigeren/adrespositie")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status412PreconditionFailed)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> CorrectPosition(
            [FromServices] BackOfficeContext backOfficeContext,
            [FromServices] IValidator<AddressCorrectPositionRequest> validator,
            [FromServices] IIfMatchHeaderValidator ifMatchHeaderValidator,
            [FromServices] IOptions<ResponseOptions> options,
            [FromRoute] int persistentLocalId,
            [FromBody] AddressCorrectPositionRequest request,
            [FromHeader(Name = "If-Match")] string? ifMatchHeaderValue,
            CancellationToken cancellationToken = default)
        {
            request.PersistentLocalId = persistentLocalId;

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

                request.Metadata = GetMetadata();
                var response = await _mediator.Send(request, cancellationToken);

                return new AcceptedWithETagResult(
                    new Uri(string.Format(options.Value.DetailUrl, request.PersistentLocalId)),
                    response.LastEventHash);
            }
            catch (IdempotencyException)
            {
                return Accepted();
            }
            catch (AggregateNotFoundException)
            {
                throw new ApiException(ValidationErrorMessages.Address.AddressNotFound, StatusCodes.Status404NotFound);
            }
            catch (DomainException exception)
            {
                throw exception switch
                {
                    AddressIsNotFoundException => new ApiException(ValidationErrorMessages.Address.AddressNotFound, StatusCodes.Status404NotFound),
                    AddressIsRemovedException => new ApiException(ValidationErrorMessages.Address.AddressRemoved, StatusCodes.Status410Gone),
                    AddressHasInvalidStatusException => CreateValidationException(
                        ValidationErrors.Address.AddressPositionCannotBeChanged,
                        string.Empty,
                        ValidationErrorMessages.Address.AddressPositionCannotBeChanged),
                    AddressHasInvalidGeometryMethodException => CreateValidationException(
                        ValidationErrors.Address.GeometryMethodInvalid,
                        string.Empty,
                        ValidationErrorMessages.Address.GeometryMethodInvalid),
                    AddressHasMissingGeometrySpecificationException => CreateValidationException(
                        ValidationErrors.Address.PositionSpecificationRequired,
                        string.Empty,
                        ValidationErrorMessages.Address.PositionSpecificationRequired),
                    AddressHasInvalidGeometrySpecificationException => CreateValidationException(
                        ValidationErrors.Address.PositionSpecificationInvalid,
                        string.Empty,
                        ValidationErrorMessages.Address.PositionSpecificationInvalid),

                    _ => new ValidationException(new List<ValidationFailure>
                        { new ValidationFailure(string.Empty, exception.Message) })
                };
            }
        }
    }
}
