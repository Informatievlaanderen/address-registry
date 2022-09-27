namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Abstractions.Exceptions;
    using Abstractions.Responses;
    using AddressRegistry.Infrastructure;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Microsoft.Extensions.Configuration;
    using Requests;
    using StreetName;
    using StreetName.Exceptions;
    using TicketingService.Abstractions;

    public sealed class SqsAddressCorrectBoxNumberLambdaHandler : SqsLambdaHandler<SqsLambdaAddressCorrectBoxNumberRequest>
    {
        public SqsAddressCorrectBoxNumberLambdaHandler(
            IConfiguration configuration,
            ICustomRetryPolicy retryPolicy,
            ITicketing ticketing,
            IStreetNames streetNames,
            IIdempotentCommandHandler idempotentCommandHandler)
            : base(
                configuration,
                retryPolicy,
                streetNames,
                ticketing,
                idempotentCommandHandler)
        { }

        protected override async Task<ETagResponse> InnerHandle(SqsLambdaAddressCorrectBoxNumberRequest request, CancellationToken cancellationToken)
        {
            var addressPersistentLocalId = new AddressPersistentLocalId(request.AddressPersistentLocalId);
            var cmd = request.ToCommand();

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
            return new ETagResponse(string.Format(DetailUrlFormat, addressPersistentLocalId), lastHash);
        }

        protected override TicketError? MapDomainException(DomainException exception, SqsLambdaAddressCorrectBoxNumberRequest request)
        {
            return exception switch
            {
                AddressHasInvalidStatusException => new TicketError(
                    ValidationErrorMessages.Address.AddressPostalCodeCannotBeChanged,
                    ValidationErrors.Address.AddressPostalCodeCannotBeChanged),
                BoxNumberHasInvalidFormatException => new TicketError(
                    ValidationErrorMessages.Address.BoxNumberInvalid,
                    ValidationErrors.Address.BoxNumberInvalid),
                AddressHasNoBoxNumberException => new TicketError(
                    ValidationErrorMessages.Address.HasNoBoxNumber,
                    ValidationErrors.Address.HasNoBoxNumber),
                BoxNumberAlreadyExistsException => new TicketError(
                    ValidationErrorMessages.Address.AddressAlreadyExists,
                    ValidationErrors.Address.AddressAlreadyExists),
                _ => null
            };
        }
    }
}
