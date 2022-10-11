namespace AddressRegistry.Api.BackOffice
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Abstractions.Exceptions;
    using Abstractions.Requests;
    using Abstractions.Validation;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using FluentValidation;
    using FluentValidation.Results;
    using Handlers.Sqs.Requests;
    using Infrastructure.Options;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using StreetName.Exceptions;
    using Swashbuckle.AspNetCore.Filters;

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
                if (_useSqsToggle.FeatureEnabled)
                {
                    var sqsRequest = new SqsAddressProposeRequest
                    {
                        Request = request,
                        Metadata = GetMetadata(),
                        ProvenanceData = new ProvenanceData(CreateFakeProvenance())
                    };
                    var sqsResult = await _mediator.Send(sqsRequest, cancellationToken);

                    return Accepted(sqsResult);
                }

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
                    ValidationErrors.Common.StreetNameInvalid.Code,
                    nameof(request.StraatNaamId),
                    ValidationErrors.Common.StreetNameInvalid.Message(request.StraatNaamId));
            }
            catch (DomainException exception)
            {
                throw exception switch
                {
                    ParentAddressAlreadyExistsException _ =>
                        CreateValidationException(
                            ValidationErrors.Common.AddressAlreadyExists.Code,
                            nameof(request.Huisnummer),
                            ValidationErrors.Common.AddressAlreadyExists.Message),

                    HouseNumberHasInvalidFormatException _ =>
                        CreateValidationException(
                            ValidationErrors.Common.HouseNumberInvalidFormat.Code,
                            nameof(request.Huisnummer),
                            ValidationErrors.Common.HouseNumberInvalidFormat.Message),

                    AddressAlreadyExistsException _ =>
                        CreateValidationException(
                            ValidationErrors.Common.AddressAlreadyExists.Code,
                            nameof(request.Busnummer),
                            ValidationErrors.Common.AddressAlreadyExists.Message),

                    ParentAddressNotFoundException e =>
                        CreateValidationException(
                            Deprecated.Address.AddressHouseNumberUnknown,
                            nameof(request.Huisnummer),
                            ValidationErrorMessages.Address.AddressHouseNumberUnknown(
                                request.StraatNaamId,
                                e.HouseNumber)),

                    StreetNameHasInvalidStatusException _ =>
                        CreateValidationException(
                                ValidationErrors.Common.StreetNameIsNotActive.Code,
                                nameof(request.StraatNaamId),
                                ValidationErrors.Common.StreetNameIsNotActive.Message),

                    StreetNameIsRemovedException _ =>
                        CreateValidationException(
                            ValidationErrors.Common.StreetNameInvalid.Code,
                            nameof(request.StraatNaamId),
                            ValidationErrors.Common.StreetNameInvalid.Message(request.StraatNaamId)),

                    PostalCodeMunicipalityDoesNotMatchStreetNameMunicipalityException _ =>
                        CreateValidationException(
                            Deprecated.Address.PostalCodeNotInMunicipality,
                            nameof(request.PostInfoId),
                            ValidationErrorMessages.Address.PostalCodeNotInMunicipality),

                    _ => new ValidationException(new List<ValidationFailure>
                        { new ValidationFailure(string.Empty, exception.Message) })
                };
            }
        }
    }
}
