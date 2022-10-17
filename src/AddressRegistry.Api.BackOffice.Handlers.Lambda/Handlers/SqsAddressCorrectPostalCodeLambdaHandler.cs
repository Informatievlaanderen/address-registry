namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
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

    public sealed class SqsAddressCorrectPostalCodeLambdaHandler : SqsLambdaHandler<SqsLambdaAddressCorrectPostalCodeRequest>
    {
        private readonly SyndicationContext _syndicationContext;
        private readonly MunicipalityConsumerContext _municipalityConsumerContext;

        public SqsAddressCorrectPostalCodeLambdaHandler(
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

            var lastHash = await GetHash(request.StreetNamePersistentLocalId(), addressPersistentLocalId, cancellationToken);
            return new ETagResponse(string.Format(DetailUrlFormat, addressPersistentLocalId), lastHash);
        }

        protected override TicketError? MapDomainException(DomainException exception, SqsLambdaAddressCorrectPostalCodeRequest request)
        {
            return exception switch
            {
                StreetNameHasInvalidStatusException => new TicketError(
                    ValidationErrors.Common.StreetNameStatusInvalidForCorrection.Message,
                    ValidationErrors.Common.StreetNameStatusInvalidForCorrection.Code),
                AddressHasInvalidStatusException => new TicketError(
                    ValidationErrors.Common.PostalCode.CannotBeChanged.Message,
                    ValidationErrors.Common.PostalCode.CannotBeChanged.Code),
                PostalCodeMunicipalityDoesNotMatchStreetNameMunicipalityException => new TicketError(
                    ValidationErrors.Common.PostalCode.PostalCodeNotInMunicipality.Message,
                    ValidationErrors.Common.PostalCode.PostalCodeNotInMunicipality.Code),
                _ => null
            };
        }
    }
}
