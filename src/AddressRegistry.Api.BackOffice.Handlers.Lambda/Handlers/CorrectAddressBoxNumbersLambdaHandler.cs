namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Validation;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using Microsoft.Extensions.Configuration;
    using Requests;
    using StreetName;
    using StreetName.Exceptions;
    using TicketingService.Abstractions;

    public sealed class CorrectAddressBoxNumbersLambdaHandler : SqsLambdaHandler<CorrectAddressBoxNumbersLambdaRequest>
    {
        public CorrectAddressBoxNumbersLambdaHandler(
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

        protected override async Task<object> InnerHandle(CorrectAddressBoxNumbersLambdaRequest request, CancellationToken cancellationToken)
        {
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

            var etagResponses = new List<ETagResponse>();

            foreach (var (addressPersistentLocalId, _) in cmd.AddressBoxNumbers)
            {
                var lastHash = await GetHash(request.StreetNamePersistentLocalId(), addressPersistentLocalId, cancellationToken);
                etagResponses.Add(new ETagResponse(string.Format(DetailUrlFormat, addressPersistentLocalId), lastHash));
            }

            return etagResponses;
        }

        protected override TicketError? InnerMapDomainException(DomainException exception, CorrectAddressBoxNumbersLambdaRequest request)
        {
            return exception switch
            {
                //TODO-rik add unit lambda tests
                BoxNumberHasInvalidFormatException => ValidationErrors.Common.BoxNumberInvalidFormat.ToTicketError(),
                StreetNameHasInvalidStatusException => ValidationErrors.Common.StreetNameStatusInvalidForAction.ToTicketError(),
                AddressIsNotFoundException ex => ValidationErrors.Common.AddressNotFoundWithId.ToTicketError((int)ex.AddressPersistentLocalId!),
                AddressIsRemovedException ex => ValidationErrors.Common.AddressRemovedWithId.ToTicketError((int)ex.AddressPersistentLocalId!),
                AddressHasNoBoxNumberException ex => ValidationErrors.CorrectBoxNumbers.HasNoBoxNumber.ToTicketError((int)ex.AddressPersistentLocalId!),
                AddressHasInvalidStatusException ex => ValidationErrors.CorrectBoxNumbers.AddressInvalidStatus.ToTicketError((int)ex.AddressPersistentLocalId!),
                BoxNumberHouseNumberDoesNotMatchParentHouseNumberException => ValidationErrors.CorrectBoxNumbers.MultipleHouseNumberAddresses.ToTicketError(),
                AddressAlreadyExistsException ex => ValidationErrors.CorrectBoxNumbers.AddressAlreadyExists.ToTicketError(ex.HouseNumber!, ex.BoxNumber!),
                _ => null
            };
        }
    }
}
