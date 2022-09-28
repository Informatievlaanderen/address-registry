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
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using FluentValidation;
    using FluentValidation.Results;
    using Handlers.Sqs.Requests;
    using Infrastructure;
    using Infrastructure.Options;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using StreetName;
    using StreetName.Exceptions;
    using Swashbuckle.AspNetCore.Filters;

    public partial class AddressController
    {
        /// <summary>
        /// Verwijder een adres.
        /// </summary>
        /// <param name="backOfficeContext"></param>
        /// <param name="validator"></param>
        /// <param name="ifMatchHeaderValidator"></param>
        /// <param name="ifMatchHeaderValue"></param>
        /// <param name="options"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="202">Aanvraag tot verwijdering wordt reeds verwerkt.</response>
        /// <response code="412">Als de If-Match header niet overeenkomt met de laatste ETag.</response>
        /// <returns></returns>
        [HttpPost("{persistentLocalId}/acties/verwijderen")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Remove(
            [FromServices] BackOfficeContext backOfficeContext,
            [FromServices] IValidator<AddressRemoveRequest> validator,
            [FromServices] IIfMatchHeaderValidator ifMatchHeaderValidator,
            [FromHeader(Name = "If-Match")] string? ifMatchHeaderValue,
            [FromServices] IOptions<ResponseOptions> options,
            [FromRoute] AddressRemoveRequest request,
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
                if (!await ifMatchHeaderValidator.IsValid(
                        ifMatchHeaderValue,
                        streetNamePersistentLocalId,
                        addressPersistentLocalId,
                        cancellationToken))
                {
                    return new PreconditionFailedResult();
                }

                if (_useSqsToggle.FeatureEnabled)
                {
                    var sqsRequest = new SqsAddressRemoveRequest
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
                var _ = await _mediator.Send(request, cancellationToken);

                return Accepted(new Uri(string.Format(options.Value.DetailUrl, request.PersistentLocalId)));
            }
            catch (AggregateIdIsNotFoundException)
            {
                throw new ApiException(ValidationErrorMessages.Address.AddressNotFound, StatusCodes.Status404NotFound);
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
                    AddressIsNotFoundException => new ApiException(ValidationErrorMessages.Address.AddressNotFound,
                        StatusCodes.Status404NotFound),

                    var _ => new ValidationException(new List<ValidationFailure> { new ValidationFailure(string.Empty, exception.Message) })
                };
            }
        }
    }
}
