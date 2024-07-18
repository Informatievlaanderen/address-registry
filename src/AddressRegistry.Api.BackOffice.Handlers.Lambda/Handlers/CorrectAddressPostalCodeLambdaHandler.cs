namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Validation;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo.Extensions;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using Consumer.Read.Municipality;
    using Consumer.Read.Postal;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Requests;
    using StreetName;
    using StreetName.Exceptions;
    using TicketingService.Abstractions;

    public sealed class CorrectAddressPostalCodeLambdaHandler : SqsLambdaHandler<CorrectAddressPostalCodeLambdaRequest>
    {
        private readonly PostalConsumerContext _postalConsumerContext;
        private readonly MunicipalityConsumerContext _municipalityConsumerContext;

        public CorrectAddressPostalCodeLambdaHandler(
             IConfiguration configuration,
             ICustomRetryPolicy retryPolicy,
             ITicketing ticketing,
             IStreetNames streetNames,
             IIdempotentCommandHandler idempotentCommandHandler,
             PostalConsumerContext postalConsumerContext,
             MunicipalityConsumerContext municipalityConsumerContext)
             : base(
                 configuration,
                 retryPolicy,
                 streetNames,
                 ticketing,
                 idempotentCommandHandler)
        {
            _postalConsumerContext = postalConsumerContext;
            _municipalityConsumerContext = municipalityConsumerContext;
        }

        protected override async Task<object> InnerHandle(CorrectAddressPostalCodeLambdaRequest request, CancellationToken cancellationToken)
        {
            var addressPersistentLocalId = new AddressPersistentLocalId(request.AddressPersistentLocalId);
            var postInfoIdentifier = request.Request.PostInfoId
                .AsIdentifier()
                .Map(x => x);
            var postalCode = new PostalCode(postInfoIdentifier.Value);

            var postalMunicipality = await _postalConsumerContext.PostalLatestItems.FindAsync(new object[] { postalCode.ToString() }, cancellationToken);
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

        protected override TicketError? InnerMapDomainException(DomainException exception, CorrectAddressPostalCodeLambdaRequest request)
        {
            return exception switch
            {
                StreetNameHasInvalidStatusException => ValidationErrors.Common.StreetNameStatusInvalidForAction.ToTicketError(),
                AddressHasInvalidStatusException => ValidationErrors.Common.PostalCode.CannotBeChanged.ToTicketError(),
                PostalCodeMunicipalityDoesNotMatchStreetNameMunicipalityException => ValidationErrors.Common.PostalCode.PostalCodeNotInMunicipality.ToTicketError(),
                AddressHasBoxNumberException => ValidationErrors.CorrectAddressPostalCode.AddressHasBoxNumber.ToTicketError(),
                _ => null
            };
        }
    }
}
