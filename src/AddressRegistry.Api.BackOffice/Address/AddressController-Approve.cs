namespace AddressRegistry.Api.BackOffice.Address
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Address;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using FluentValidation;
    using FluentValidation.Results;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Requests;
    using StreetName;
    using StreetName.Commands;
    using Swashbuckle.AspNetCore.Filters;

    public partial class AddressController
    {
        /// <summary>
        /// Keur een adres goed.
        /// </summary>
        /// <param name="idempotencyContext"></param>
        /// <param name="backOfficeContext"></param>
        /// <param name="streetNameRepository"></param>
        /// <param name="addressApproveRequest"></param>
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
            [FromServices] IdempotencyContext idempotencyContext,
            [FromServices] BackOfficeContext backOfficeContext,
            [FromServices] IValidator<AddressApproveRequest> validator,
            [FromServices] IStreetNames streetNameRepository,
            [FromRoute] AddressApproveRequest addressApproveRequest,
            [FromHeader(Name = "If-Match")] string? ifMatchHeaderValue,
            CancellationToken cancellationToken = default)
        {
            await validator.ValidateAndThrowAsync(addressApproveRequest, cancellationToken);

            try
            {
                var addressPersistentLocalId = new AddressPersistentLocalId(new PersistentLocalId(addressApproveRequest.PersistentLocalId));

                var relation = backOfficeContext.AddressPersistentIdStreetNamePersistentIds
                    .FirstOrDefault(x => x.AddressPersistentLocalId == addressPersistentLocalId);

                if (relation is null)
                {
                    return NotFound();
                }

                var streetNamePersistentLocalId = new StreetNamePersistentLocalId(relation.StreetNamePersistentLocalId);

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

                var cmd = new ApproveAddress(
                    streetNamePersistentLocalId,
                    addressPersistentLocalId,
                    CreateFakeProvenance());

                await IdempotentCommandHandlerDispatch(idempotencyContext, cmd.CreateCommandId(), cmd, cancellationToken);
                
                var etag = await GetHash(
                    streetNameRepository,
                    streetNamePersistentLocalId,
                    addressPersistentLocalId,
                    cancellationToken);

                return new NoContentWithETagResult(etag);
            }
            catch (IdempotencyException)
            {
                return Accepted();
            }
            catch (DomainException exception)
            {
                throw exception switch
                {
                    // TODO: catch validation exceptions

                    _ => new ValidationException(new List<ValidationFailure>
                        { new ValidationFailure(string.Empty, exception.Message) })
                };
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
