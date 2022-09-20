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

    public sealed class SqsAddressRegularizeHandler : SqsLambdaHandler<SqsLambdaAddressRegularizeRequest>
    {
        public SqsAddressRegularizeHandler(
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

        protected override async Task<ETagResponse> InnerHandle(SqsLambdaAddressRegularizeRequest request, CancellationToken cancellationToken)
        {
            var streetNamePersistentLocalId = new AddressPersistentLocalId(request.Request.PersistentLocalId);
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

            var lastHash = await GetHash(request.StreetNamePersistentLocalId, streetNamePersistentLocalId, cancellationToken);
            return new ETagResponse(lastHash);
        }

        protected override TicketError? MapDomainException(DomainException exception, SqsLambdaAddressRegularizeRequest request)
        {
            return exception switch
            {
                StreetNameHasInvalidStatusException => new TicketError(
                    ValidationErrorMessages.Address.AddressCannotBeRegularized,
                    ValidationErrors.Address.AddressCannotBeRegularized),
                _ => null
            };
        }
    }
}
