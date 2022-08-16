namespace AddressRegistry.Api.BackOffice
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Exceptions;
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using FluentValidation;
    using FluentValidation.Results;
    using Infrastructure.Options;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using StreetName.Exceptions;
    using Swashbuckle.AspNetCore.Filters;
    using Validators;

    public partial class AddressController
    {
        /// <summary>
        /// Stel een address voor.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="request"></param>
        /// <param name="validator"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="201">Als de adres voorgesteld is.</response>
        /// <response code="202">Als de adres reeds voorgesteld is.</response>
        /// <returns></returns>
        [HttpPost("acties/voorstellen")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseHeader(StatusCodes.Status201Created, "location", "string", "De url van het voorgestelde adres.")]
        [SwaggerRequestExample(typeof(AddressProposeRequest), typeof(AddressProposeRequestExamples))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Propose(
            [FromServices] IOptions<ResponseOptions> options,
            [FromServices] IValidator<AddressProposeRequest> validator,
            [FromBody] AddressProposeRequest request,
            CancellationToken cancellationToken = default)
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken);

            try
            {
                request.Metadata = GetMetadata();
                var response = await _mediator.Send(request, cancellationToken);

                return new CreatedWithLastObservedPositionAsETagResult(
                    new Uri(string.Format(options.Value.DetailUrl, response.PersistentLocalId)), response.LastEventHash);
            }
            catch (IdempotencyException)
            {
                return Accepted();
            }
            catch (AggregateNotFoundException)
            {
                throw CreateValidationException(
                    ValidationErrorCodes.StreetNameInvalid,
                    nameof(request.StraatNaamId),
                    ValidationErrorMessages.StreetNameInvalid(request.StraatNaamId));
            }
            catch (DomainException exception)
            {
                throw exception switch
                {
                    ParentAddressAlreadyExistsException _ =>
                        CreateValidationException(
                            ValidationErrorCodes.AddressAlreadyExists,
                            nameof(request.Huisnummer),
                            ValidationErrorMessages.AddressAlreadyExists),

                    BoxNumberAlreadyExistsException _ =>
                        CreateValidationException(
                            ValidationErrorCodes.AddressAlreadyExists,
                            nameof(request.Busnummer),
                            ValidationErrorMessages.AddressAlreadyExists),

                    ParentAddressNotFoundException e =>
                        CreateValidationException(
                            ValidationErrorCodes.AddressHouseNumberUnknown,
                            nameof(request.Huisnummer),
                            ValidationErrorMessages.AddressHouseNumberUnknown(
                                request.StraatNaamId,
                                e.HouseNumber)),

                    StreetNameHasInvalidStatusException _ =>
                        CreateValidationException(
                            ValidationErrorCodes.StreetNameIsNotActive,
                            nameof(request.StraatNaamId),
                            ValidationErrorMessages.StreetNameIsNotActive),

                    StreetNameIsRemovedException _ =>
                        CreateValidationException(
                            ValidationErrorCodes.StreetNameInvalid,
                            nameof(request.StraatNaamId),
                            ValidationErrorMessages.StreetNameInvalid(request.StraatNaamId)),

                    PostalCodeMunicipalityDoesNotMatchStreetNameMunicipalityException _ =>
                        CreateValidationException(
                            ValidationErrorCodes.PostalCodeNotInMunicipality,
                            nameof(request.PostInfoId),
                            ValidationErrorMessages.PostalCodeNotInMunicipality),

                    _ => new ValidationException(new List<ValidationFailure>
                        { new ValidationFailure(string.Empty, exception.Message) })
                };
            }
        }
    }
}
