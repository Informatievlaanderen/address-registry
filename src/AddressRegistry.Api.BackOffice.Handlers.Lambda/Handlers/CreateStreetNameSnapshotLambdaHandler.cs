namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Handlers
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
    using Microsoft.Extensions.Configuration;
    using Requests;
    using StreetName;
    using TicketingService.Abstractions;

    public sealed class CreateStreetNameSnapshotLambdaHandler : SqsLambdaHandler<CreateStreetNameSnapshotLambdaRequest>
    {
        public CreateStreetNameSnapshotLambdaHandler(
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

        protected override async Task<object> InnerHandle(CreateStreetNameSnapshotLambdaRequest request, CancellationToken cancellationToken)
        {
            var cmd = request.ToCommand();

            try
            {
                await IdempotentCommandHandler.Dispatch(
                    cmd.CreateCommandId(),
                    cmd,
                    request.Metadata!,
                    cancellationToken);
            }
            catch (IdempotencyException)
            {
                // Idempotent: Do Nothing return last etag
            }

            return "snapshot created";
        }

        protected override TicketError? InnerMapDomainException(DomainException exception, CreateStreetNameSnapshotLambdaRequest request)
        {
            return null;
        }
    }
}
