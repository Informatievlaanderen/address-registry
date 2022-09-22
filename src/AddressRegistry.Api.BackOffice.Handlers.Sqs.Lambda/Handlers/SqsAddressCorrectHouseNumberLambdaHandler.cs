namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Lambda.Handlers
{
    using Abstractions;
    using Abstractions.Exceptions;
    using Abstractions.Responses;
    using AddressRegistry.Infrastructure;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Microsoft.Extensions.Configuration;
    using Requests;
    using StreetName;
    using StreetName.Exceptions;
    using System.Threading;
    using System.Threading.Tasks;
    using TicketingService.Abstractions;

    public sealed class SqsAddressCorrectHouseNumberLambdaHandler : SqsLambdaHandler<SqsLambdaAddressCorrectHouseNumberRequest>
    {
        public SqsAddressCorrectHouseNumberLambdaHandler(
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

        protected override async Task<ETagResponse> InnerHandle(SqsLambdaAddressCorrectHouseNumberRequest request, CancellationToken cancellationToken)
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

        protected override TicketError? MapDomainException(DomainException exception, SqsLambdaAddressCorrectHouseNumberRequest request)
        {
            return exception switch
            {
                AddressHasInvalidStatusException => new TicketError(
                    ValidationErrorMessages.Address.AddressPostalCodeCannotBeChanged,
                    ValidationErrors.Address.AddressPostalCodeCannotBeChanged),
                ParentAddressAlreadyExistsException => new TicketError(
                    ValidationErrorMessages.Address.AddressAlreadyExists,
                    ValidationErrors.Address.AddressAlreadyExists),
                HouseNumberHasInvalidFormatException => new TicketError(
                    ValidationErrorMessages.Address.HouseNumberInvalid,
                    ValidationErrors.Address.HouseNumberInvalid),
                HouseNumberToCorrectHasBoxNumberException => new TicketError(
                    ValidationErrorMessages.Address.HouseNumberOfBoxNumberCannotBeChanged,
                    ValidationErrors.Address.HouseNumberOfBoxNumberCannotBeChanged),
                _ => null
            };
        }
    }
}
