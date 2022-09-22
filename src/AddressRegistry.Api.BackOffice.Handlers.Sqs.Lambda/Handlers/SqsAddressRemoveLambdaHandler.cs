namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Lambda.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Exceptions;
    using Abstractions.Responses;
    using AddressRegistry.Infrastructure;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Microsoft.Extensions.Configuration;
    using Requests;
    using StreetName;
    using TicketingService.Abstractions;

    public sealed class SqsAddressRemoveLambdaHandler : SqsLambdaHandler<SqsLambdaAddressRemoveRequest>
    {
        public SqsAddressRemoveLambdaHandler(
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

        protected override async Task<ETagResponse> InnerHandle(SqsLambdaAddressRemoveRequest request, CancellationToken cancellationToken)
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

        protected override TicketError? MapDomainException(DomainException exception, SqsLambdaAddressRemoveRequest request)
        {
            return null;
        }
    }
}
