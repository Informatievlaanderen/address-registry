namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Lambda.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Abstractions.Exceptions;
    using Abstractions.Responses;
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

    public sealed class SqsAddressCorrectPostalCodeHandler : SqsLambdaHandler<SqsLambdaAddressCorrectPostalCodeRequest>
    {
        private readonly SyndicationContext _syndicationContext;
        private readonly MunicipalityConsumerContext _municipalityConsumerContext;

        public SqsAddressCorrectPostalCodeHandler(
             IConfiguration configuration,
             ICustomRetryPolicy retryPolicy,
             ITicketing ticketing,
             IStreetNames streetNames,
             IIdempotentCommandHandler idempotentCommandHandler,
             SyndicationContext syndicationContext,
             MunicipalityConsumerContext municipalityConsumerContext)
             : base(
                 configuration,
                 retryPolicy,
                 streetNames,
                 ticketing,
                 idempotentCommandHandler)
        {
            _syndicationContext = syndicationContext;
            _municipalityConsumerContext = municipalityConsumerContext;
        }

        protected override async Task<ETagResponse> InnerHandle(SqsLambdaAddressCorrectPostalCodeRequest request, CancellationToken cancellationToken)
        {
            var streetNamePersistentLocalId = new StreetNamePersistentLocalId(int.Parse(request.MessageGroupId));
            var addressPersistentLocalId = new AddressPersistentLocalId(request.AddressPersistentLocalId);

            var postInfoIdentifier = request.Request.PostInfoId
                .AsIdentifier()
                .Map(x => x);
            var postalCode = new PostalCode(postInfoIdentifier.Value);

            var postalMunicipality = await _syndicationContext.PostalInfoLatestItems.FindAsync(new object[] { postalCode.ToString() }, cancellationToken);
            if (postalMunicipality is null)
            {
                throw new PostalCodeMunicipalityDoesNotMatchStreetNameMunicipalityException();
            }

            var municipality = await _municipalityConsumerContext
                .MunicipalityLatestItems
                .SingleAsync(x => x.NisCode == postalMunicipality.NisCode, cancellationToken);
            
            var cmd = request.ToCommand(new MunicipalityId(municipality.MunicipalityId));

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

            var lastHash = await GetHash(request.StreetNamePersistentLocalId, addressPersistentLocalId, cancellationToken);
            return new ETagResponse(lastHash);
        }

        protected override TicketError? MapDomainException(DomainException exception, SqsLambdaAddressCorrectPostalCodeRequest request)
        {
            return exception switch
            {
                AddressHasInvalidStatusException => new TicketError(
                    ValidationErrorMessages.Address.AddressPostalCodeCannotBeChanged,
                    ValidationErrors.Address.AddressPostalCodeCannotBeChanged),
                PostalCodeMunicipalityDoesNotMatchStreetNameMunicipalityException => new TicketError(
                    ValidationErrorMessages.Address.PostalCodeNotInMunicipality,
                    ValidationErrors.Address.PostalCodeNotInMunicipality),
                _ => null
            };
        }
    }
}
