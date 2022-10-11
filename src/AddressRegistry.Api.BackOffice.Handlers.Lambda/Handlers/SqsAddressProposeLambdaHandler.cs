namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Abstractions.Responses;
    using Abstractions.Validation;
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
    using TicketingService.Abstractions;
    using PostalCode = StreetName.PostalCode;

    public sealed class SqsAddressProposeLambdaHandler : SqsLambdaHandler<SqsLambdaAddressProposeRequest>
    {
        private readonly BackOfficeContext _backOfficeContext;
        private readonly SyndicationContext _syndicationContext;
        private readonly MunicipalityConsumerContext _municipalityConsumerContext;
        private readonly IPersistentLocalIdGenerator _persistentLocalIdGenerator;

        public SqsAddressProposeLambdaHandler(
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
                        request.StreetNamePersistentLocalId),
                    cancellationToken);
            await _backOfficeContext.SaveChangesAsync(cancellationToken);


            var lastHash = await GetHash(request.StreetNamePersistentLocalId, addressPersistentLocalId, cancellationToken);
            return new ETagResponse(string.Format(DetailUrlFormat, addressPersistentLocalId), lastHash);
        }

        protected override TicketError? MapDomainException(DomainException exception, SqsLambdaAddressProposeRequest request)
        {
            return exception switch
            {
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
                StreetNameIsRemovedException => new TicketError(
                    ValidationErrors.Common.StreetNameInvalid.Message(request.Request.StraatNaamId),
                    ValidationErrors.Common.StreetNameInvalid.Code),
                PostalCodeMunicipalityDoesNotMatchStreetNameMunicipalityException => new TicketError(
                    ValidationErrors.Common.PostalCode.PostalCodeNotInMunicipality.Message,
                    ValidationErrors.Common.PostalCode.PostalCodeNotInMunicipality.Code),
                _ => null
            };
        }
    }
}
