namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Lambda.Handlers
{
    using Abstractions;
    using Abstractions.Responses;
    using Address;
    using AddressRegistry.Infrastructure;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo.Extensions;
    using Consumer.Read.Municipality;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Projections.Syndication;
    using Requests;
    using StreetName;
    using StreetName.Exceptions;
    using System.Threading;
    using System.Threading.Tasks;
    using TicketingService.Abstractions;
    using PostalCode = StreetName.PostalCode;

    public sealed class SqsAddressProposeHandler : SqsLambdaHandler<SqsLambdaAddressProposeRequest>
    {
        private readonly BackOfficeContext _backOfficeContext;
        private readonly SyndicationContext _syndicationContext;
        private readonly MunicipalityConsumerContext _municipalityConsumerContext;
        private readonly IPersistentLocalIdGenerator _persistentLocalIdGenerator;

        public SqsAddressProposeHandler(
            IConfiguration configuration,
            ICustomRetryPolicy retryPolicy,
            ITicketing ticketing,
            IStreetNames streetNames,
            IIdempotentCommandHandler idempotentCommandHandler,
            BackOfficeContext backOfficeContext,
            SyndicationContext syndicationContext,
            MunicipalityConsumerContext municipalityConsumerContext,
            IPersistentLocalIdGenerator persistentLocalIdGenerator)
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
            _persistentLocalIdGenerator = persistentLocalIdGenerator;
        }

        protected override async Task<ETagResponse> InnerHandle(SqsLambdaAddressProposeRequest request, CancellationToken cancellationToken)
        {
            var streetNamePersistentLocalId = new StreetNamePersistentLocalId(int.Parse(request.MessageGroupId));

            var postInfoIdentifier = request.Request.PostInfoId
                .AsIdentifier()
                .Map(x => x);

            var postalCode = new PostalCode(postInfoIdentifier.Value);
            var addressPersistentLocalId =
                new AddressPersistentLocalId(_persistentLocalIdGenerator.GenerateNextPersistentLocalId());

            var postalMunicipality =
                await _syndicationContext.PostalInfoLatestItems.FindAsync(new object[] { postalCode.ToString() },
                    cancellationToken);
            if (postalMunicipality is null)
            {
                throw new PostalCodeMunicipalityDoesNotMatchStreetNameMunicipalityException();
            }

            var municipality = await _municipalityConsumerContext
                .MunicipalityLatestItems
                .SingleAsync(x => x.NisCode == postalMunicipality.NisCode, cancellationToken);

            var cmd = request.ToCommand(addressPersistentLocalId, postalCode,
                new MunicipalityId(municipality.MunicipalityId));

            await IdempotentCommandHandler.Dispatch(
                cmd.CreateCommandId(),
                cmd,
                request.Metadata,
                cancellationToken);

            // Insert PersistentLocalId with MunicipalityId
            await _backOfficeContext
                .AddressPersistentIdStreetNamePersistentIds
                .AddAsync(
                    new AddressPersistentIdStreetNamePersistentId(
                        addressPersistentLocalId,
                        streetNamePersistentLocalId),
                    cancellationToken);
            await _backOfficeContext.SaveChangesAsync(cancellationToken);


            var lastHash = await GetHash(request.StreetNamePersistentLocalId, addressPersistentLocalId,
                cancellationToken);
            return new ETagResponse(lastHash);
        }

        protected override TicketError? MapDomainException(DomainException exception, SqsLambdaAddressProposeRequest request)
        {
            return exception switch
            {
                ParentAddressAlreadyExistsException => new TicketError(
                    ValidationErrorMessages.Address.AddressAlreadyExists,
                    ValidationErrors.Address.AddressAlreadyExists),
                HouseNumberHasInvalidFormatException => new TicketError(
                    ValidationErrorMessages.Address.HouseNumberInvalid,
                    ValidationErrors.Address.HouseNumberInvalid),
                BoxNumberAlreadyExistsException => new TicketError(
                    ValidationErrorMessages.Address.AddressAlreadyExists,
                    ValidationErrors.Address.AddressAlreadyExists),
                ParentAddressNotFoundException e => new TicketError(
                    ValidationErrorMessages.Address.AddressHouseNumberUnknown(
                        request.Request.StraatNaamId,
                        e.HouseNumber),
                    ValidationErrors.Address.AddressHouseNumberUnknown),
                StreetNameHasInvalidStatusException => new TicketError(
                    ValidationErrorMessages.StreetName.StreetNameIsNotActive,
                    ValidationErrors.StreetName.StreetNameIsNotActive),
                StreetNameIsRemovedException e => new TicketError(
                    ValidationErrorMessages.StreetName.StreetNameInvalid(request.Request.StraatNaamId),
                    ValidationErrors.StreetName.StreetNameInvalid),
                PostalCodeMunicipalityDoesNotMatchStreetNameMunicipalityException => new TicketError(
                    ValidationErrorMessages.Address.PostalCodeNotInMunicipality,
                    ValidationErrors.Address.PostalCodeNotInMunicipality),
                _ => null
            };
        }
    }
}
