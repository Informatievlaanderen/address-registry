namespace AddressRegistry.Api.BackOffice.Address
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Address;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo.Extensions;
    using FluentValidation;
    using FluentValidation.Results;
    using Infrastructure.Options;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Projections.Syndication;
    using Requests;
    using StreetName;
    using StreetName.Exceptions;
    using Swashbuckle.AspNetCore.Filters;
    using Validators;
    using PostalCode = StreetName.PostalCode;

    public partial class AddressController
    {
        /// <summary>
        /// Stel een address voor.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="idempotencyContext"></param>
        /// <param name="backOfficeContext"></param>
        /// <param name="persistentLocalIdGenerator"></param>
        /// <param name="streetNameRepository"></param>
        /// <param name="addressProposeRequest"></param>
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
            [FromServices] IdempotencyContext idempotencyContext,
            [FromServices] BackOfficeContext backOfficeContext,
            [FromServices] SyndicationContext syndicationContext,
            [FromServices] IPersistentLocalIdGenerator persistentLocalIdGenerator,
            [FromServices] IValidator<AddressProposeRequest> validator,
            [FromServices] IStreetNames streetNameRepository,
            [FromBody] AddressProposeRequest addressProposeRequest,
            CancellationToken cancellationToken = default)
        {
            await validator.ValidateAndThrowAsync(addressProposeRequest, cancellationToken);

            try
            {
                var identifier = addressProposeRequest.StraatNaamId
                    .AsIdentifier()
                    .Map(x => x);

                var postInfoIdentifier = addressProposeRequest.PostInfoId
                    .AsIdentifier()
                    .Map(x => x);

                var streetNamePersistentLocalId = new StreetNamePersistentLocalId(int.Parse(identifier.Value));
                var postalCodeId = new PostalCode(postInfoIdentifier.Value);
                var addressPersistentLocalId =
                    new AddressPersistentLocalId(persistentLocalIdGenerator.GenerateNextPersistentLocalId());

                var postalMunicipality = await syndicationContext.PostalInfoLatestItems.FindAsync(new object[] { postalCodeId.ToString() }, cancellationToken);
                if (postalMunicipality is null)
                {
                    throw new PostalCodeMunicipalityDoesNotMatchStreetNameMunicipalityException();
                }

                var municipality = await syndicationContext.MunicipalityLatestItems.SingleAsync(x => x.NisCode == postalMunicipality.NisCode, cancellationToken);
                var cmd = addressProposeRequest.ToCommand(
                    streetNamePersistentLocalId,
                    postalCodeId,
                    new MunicipalityId(municipality.MunicipalityId),
                    addressPersistentLocalId,
                    CreateFakeProvenance());

                await IdempotentCommandHandlerDispatch(idempotencyContext, cmd.CreateCommandId(), cmd, cancellationToken);

                // Insert PersistentLocalId with MunicipalityId
                await backOfficeContext
                    .AddressPersistentIdStreetNamePersistentIds
                    .AddAsync(
                        new AddressPersistentIdStreetNamePersistentId(
                            addressPersistentLocalId,
                            streetNamePersistentLocalId),
                        cancellationToken);
                await backOfficeContext.SaveChangesAsync(cancellationToken);

                var addressHash = await GetHash(
                    streetNameRepository,
                    streetNamePersistentLocalId,
                    addressPersistentLocalId,
                    cancellationToken);

                return new CreatedWithLastObservedPositionAsETagResult(
                    new Uri(string.Format(options.Value.DetailUrl, addressPersistentLocalId)), addressHash);
            }
            catch (IdempotencyException)
            {
                return Accepted();
            }
            catch (AggregateNotFoundException)
            {
                throw CreateValidationException(
                    ValidationErrorCodes.StreetNameInvalid,
                    nameof(addressProposeRequest.StraatNaamId),
                    ValidationErrorMessages.StreetNameInvalid(addressProposeRequest.StraatNaamId));
            }
            catch (DomainException exception)
            {
                throw exception switch
                {
                    ParentAddressAlreadyExistsException _ =>
                        CreateValidationException(
                            ValidationErrorCodes.AddressAlreadyExists,
                            nameof(addressProposeRequest.Huisnummer),
                            ValidationErrorMessages.AddressAlreadyExists),

                    DuplicateBoxNumberException _ =>
                        CreateValidationException(
                            ValidationErrorCodes.AddressAlreadyExists,
                            nameof(addressProposeRequest.Busnummer),
                            ValidationErrorMessages.AddressAlreadyExists),

                    ParentAddressNotFoundException e =>
                        CreateValidationException(
                            ValidationErrorCodes.AddressHouseNumberUnknown,
                            nameof(addressProposeRequest.Huisnummer),
                            ValidationErrorMessages.AddressHouseNumberUnknown(
                                addressProposeRequest.StraatNaamId,
                                e.HouseNumber)),

                    StreetNameNotActiveException _ =>
                        CreateValidationException(
                            ValidationErrorCodes.StreetNameIsNotActive,
                            nameof(addressProposeRequest.StraatNaamId),
                            ValidationErrorMessages.StreetNameIsNotActive),

                    StreetNameIsRemovedException _ =>
                        CreateValidationException(
                            ValidationErrorCodes.StreetNameInvalid,
                            nameof(addressProposeRequest.StraatNaamId),
                            ValidationErrorMessages.StreetNameInvalid(addressProposeRequest.StraatNaamId)),

                    PostalCodeMunicipalityDoesNotMatchStreetNameMunicipalityException _ =>
                        CreateValidationException(
                            ValidationErrorCodes.PostalCodeNotInMunicipality,
                            nameof(addressProposeRequest.PostInfoId),
                            ValidationErrorMessages.PostalCodeNotInMunicipality),

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
