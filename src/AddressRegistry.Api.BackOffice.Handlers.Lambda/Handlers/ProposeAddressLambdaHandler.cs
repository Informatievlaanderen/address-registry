namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Abstractions.Validation;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo.Extensions;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using Consumer.Read.Municipality;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Projections.Syndication;
    using Requests;
    using StreetName;
    using StreetName.Exceptions;
    using TicketingService.Abstractions;
    using PostalCode = StreetName.PostalCode;

    public sealed class ProposeAddressLambdaHandler : SqsLambdaHandler<ProposeAddressLambdaRequest>
    {
        private readonly BackOfficeContext _backOfficeContext;
        private readonly SyndicationContext _syndicationContext;
        private readonly MunicipalityConsumerContext _municipalityConsumerContext;

        public ProposeAddressLambdaHandler(
            IConfiguration configuration,
            ICustomRetryPolicy retryPolicy,
            ITicketing ticketing,
            IStreetNames streetNames,
            IIdempotentCommandHandler idempotentCommandHandler,
            BackOfficeContext backOfficeContext,
            SyndicationContext syndicationContext,
            MunicipalityConsumerContext municipalityConsumerContext)
            : base(
                configuration,
                retryPolicy,
                streetNames,
                ticketing,
                idempotentCommandHandler)
        {
            _backOfficeContext = backOfficeContext;
            _syndicationContext = syndicationContext;
            _municipalityConsumerContext = municipalityConsumerContext;
        }

        protected override async Task<object> InnerHandle(ProposeAddressLambdaRequest request, CancellationToken cancellationToken)
        {
            var postInfoIdentifier = request.Request.PostInfoId.AsIdentifier().Map(x => x);
            var postalCode = new PostalCode(postInfoIdentifier.Value);

            var postalMunicipality = await _syndicationContext.PostalInfoLatestItems.FindAsync(new object[] { postalCode.ToString() }, cancellationToken);
            if (postalMunicipality is null)
            {
                throw new PostalCodeMunicipalityDoesNotMatchStreetNameMunicipalityException();
            }

            var municipality = await _municipalityConsumerContext
                .MunicipalityLatestItems
                .SingleAsync(x => x.NisCode == postalMunicipality.NisCode, cancellationToken);

            var cmd = request.ToCommand(postalCode, new MunicipalityId(municipality.MunicipalityId));

            try
            {
                await IdempotentCommandHandler.Dispatch(
                    cmd.CreateCommandId(),
                    cmd,
                    request.Metadata,
                    cancellationToken);
            }
            catch (IdempotencyException)
            {
                // Idempotent: Do Nothing return last etag
            }

            await _backOfficeContext.AddIdempotentAddressStreetNameIdRelation(request.AddressPersistentLocalId, request.StreetNamePersistentLocalId(), cancellationToken);
            await _backOfficeContext.SaveChangesAsync(cancellationToken);

            var lastHash = await GetHash(request.StreetNamePersistentLocalId(), request.AddressPersistentLocalId, cancellationToken);
            return new ETagResponse(string.Format(DetailUrlFormat, request.AddressPersistentLocalId), lastHash);
        }

        protected override TicketError? InnerMapDomainException(DomainException exception, ProposeAddressLambdaRequest request)
        {
            return exception switch
            {
                StreetNameIsRemovedException => new TicketError(
                    ValidationErrors.Common.StreetNameInvalid.Message(request.Request.StraatNaamId),
                    ValidationErrors.Common.StreetNameInvalid.Code),
                ParentAddressAlreadyExistsException => new TicketError(
                    ValidationErrors.Common.AddressAlreadyExists.Message,
                    ValidationErrors.Common.AddressAlreadyExists.Code),
                HouseNumberHasInvalidFormatException => new TicketError(
                    ValidationErrors.Common.HouseNumberInvalidFormat.Message,
                    ValidationErrors.Common.HouseNumberInvalidFormat.Code),
                AddressAlreadyExistsException => new TicketError(
                    ValidationErrors.Common.AddressAlreadyExists.Message,
                    ValidationErrors.Common.AddressAlreadyExists.Code),
                ParentAddressNotFoundException e => new TicketError(
                    ValidationErrors.Propose.AddressHouseNumberUnknown.Message(request.Request.StraatNaamId, e.HouseNumber),
                    ValidationErrors.Propose.AddressHouseNumberUnknown.Code),
                StreetNameHasInvalidStatusException => new TicketError(
                    ValidationErrors.Common.StreetNameIsNotActive.Message,
                    ValidationErrors.Common.StreetNameIsNotActive.Code),
                PostalCodeMunicipalityDoesNotMatchStreetNameMunicipalityException => new TicketError(
                    ValidationErrors.Common.PostalCode.PostalCodeNotInMunicipality.Message,
                    ValidationErrors.Common.PostalCode.PostalCodeNotInMunicipality.Code),
                AddressHasInvalidGeometryMethodException => new TicketError(
                    ValidationErrors.Common.PositionGeometryMethod.Invalid.Message,
                    ValidationErrors.Common.PositionGeometryMethod.Invalid.Code),
                AddressHasInvalidGeometrySpecificationException => new TicketError(
                    ValidationErrors.Common.PositionSpecification.Invalid.Message,
                    ValidationErrors.Common.PositionSpecification.Invalid.Code),
                BoxNumberPostalCodeDoesNotMatchHouseNumberPostalCodeException =>
                    ValidationErrors.Propose.BoxNumberPostalCodeDoesNotMatchHouseNumberPostalCode.ToTicketError(),
                _ => null
            };
        }
    }
}
