namespace AddressRegistry.Api.BackOffice
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Abstractions.Requests;
    using Abstractions.Validation;
    using Address;
    using Be.Vlaanderen.Basisregisters.AcmIdm;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using FluentValidation;
    using FluentValidation.Results;
    using Handlers.Sqs.Requests;
    using Infrastructure;
    using Infrastructure.Options;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using StreetName;
    using StreetName.Exceptions;
    using Swashbuckle.AspNetCore.Filters;

    public partial class AddressController
    {
        /// <summary>
        /// Corrigeer een adres afkeuring.
        /// </summary>
        /// <param name="backOfficeContext"></param>
        /// <param name="ifMatchHeaderValidator"></param>
        /// <param name="options"></param>
        /// <param name="persistentLocalId"></param>
        /// <param name="request"></param>
        /// <param name="validator"></param>
        /// <param name="ifMatchHeaderValue"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="202">Aanvraag tot correctie adres afkeuring wordt reeds verwerkt.</response>
        /// <response code="400">Als de adres status niet 'afgekeurd' of 'voorgesteld' is.</response>
        /// <response code="412">Als de If-Match header niet overeenkomt met de laatste ETag.</response>
        /// <returns></returns>
        [HttpPost("{persistentLocalId}/acties/corrigeren/afkeuring")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status412PreconditionFailed)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyNames.Adres.DecentraleBijwerker)]
        public async Task<IActionResult> CorrectRejection(
            [FromServices] BackOfficeContext backOfficeContext,
            [FromServices] IValidator<CorrectAddressRejectionRequest> validator,
            [FromServices] IIfMatchHeaderValidator ifMatchHeaderValidator,
            [FromServices] IOptions<ResponseOptions> options,
            [FromRoute] CorrectAddressRejectionRequest request,
            [FromHeader(Name = "If-Match")] string? ifMatchHeaderValue,
            CancellationToken cancellationToken = default)
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken);

            var addressPersistentLocalId = new AddressPersistentLocalId(new PersistentLocalId(request.PersistentLocalId));

            var relation = backOfficeContext.AddressPersistentIdStreetNamePersistentIds
                .FirstOrDefault(x => x.AddressPersistentLocalId == addressPersistentLocalId);

            if (relation is null)
            {
                throw new ApiException(ValidationErrors.Common.AddressNotFound.Message, StatusCodes.Status404NotFound);
            }

            var streetNamePersistentLocalId = new StreetNamePersistentLocalId(relation.StreetNamePersistentLocalId);

            try
            {
                // Check if user provided ETag is equal to the current Entity Tag
                if (!await ifMatchHeaderValidator.IsValid(ifMatchHeaderValue, streetNamePersistentLocalId, addressPersistentLocalId, cancellationToken))
                {
                    return new PreconditionFailedResult();
                }

                var sqsRequest = new CorrectAddressRejectionSqsRequest
                {
                    Request = request,
                    IfMatchHeaderValue = ifMatchHeaderValue,
                    Metadata = GetMetadata(),
                    ProvenanceData = new ProvenanceData(CreateFakeProvenance())
                };
                var sqsResult = await _mediator.Send(sqsRequest, cancellationToken);

                return Accepted(sqsResult);
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
                    StreetNameHasInvalidStatusException => CreateValidationException(
                        ValidationErrors.Common.StreetNameStatusInvalidForCorrection.Code,
                        string.Empty,
                        ValidationErrors.Common.StreetNameStatusInvalidForCorrection.Message),
                    AddressIsNotFoundException => new ApiException(ValidationErrors.Common.AddressNotFound.Message, StatusCodes.Status404NotFound),
                    AddressIsRemovedException => new ApiException(ValidationErrors.Common.AddressRemoved.Message, StatusCodes.Status410Gone),
                    AddressHasInvalidStatusException => CreateValidationException(
                        ValidationErrors.CorrectRejection.AddressInvalidStatus.Code,
                        string.Empty,
                        ValidationErrors.CorrectRejection.AddressInvalidStatus.Message),

                    AddressAlreadyExistsException => CreateValidationException(
                        ValidationErrors.Common.AddressAlreadyExists.Code,
                        string.Empty,
                        ValidationErrors.Common.AddressAlreadyExists.Message),

                    AddressBoxNumberHasInconsistentHouseNumberException => CreateValidationException(
                        ValidationErrors.CorrectRejection.InconsistentHouseNumber.Code,
                        string.Empty,
                        ValidationErrors.CorrectRejection.InconsistentHouseNumber.Message),

                    AddressBoxNumberHasInconsistentPostalCodeException => CreateValidationException(
                        ValidationErrors.CorrectRejection.InconsistentPostalCode.Code,
                        string.Empty,
                        ValidationErrors.CorrectRejection.InconsistentPostalCode.Message),

                    ParentAddressHasInvalidStatusException => CreateValidationException(
                        ValidationErrors.CorrectRejection.ParentInvalidStatus.Code,
                        string.Empty,
                        ValidationErrors.CorrectRejection.ParentInvalidStatus.Message),

                    _ => new ValidationException(new List<ValidationFailure>
                        { new(string.Empty, exception.Message) })
                };
            }
        }
    }
}
