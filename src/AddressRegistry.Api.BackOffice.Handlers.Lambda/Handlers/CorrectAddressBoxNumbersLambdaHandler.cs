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
                AddressHasInvalidStatusException => ValidationErrors.Common.PostalCode.CannotBeChanged.ToTicketError(),
                StreetNameHasInvalidStatusException => ValidationErrors.Common.StreetNameStatusInvalidForAction.ToTicketError(),
                BoxNumberHasInvalidFormatException => ValidationErrors.Common.BoxNumberInvalidFormat.ToTicketError(),
                AddressHasNoBoxNumberException => ValidationErrors.CorrectBoxNumber.HasNoBoxNumber.ToTicketError(),
                AddressAlreadyExistsException => ValidationErrors.Common.AddressAlreadyExists.ToTicketError(),
                _ => null
            };
        }
    }
}
