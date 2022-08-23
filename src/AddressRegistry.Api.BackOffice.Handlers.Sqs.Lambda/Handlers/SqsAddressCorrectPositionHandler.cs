namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Lambda.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using StreetName;
    using TicketingService.Abstractions;

    public class SqsAddressCorrectPositionHandler : SqsLambdaHandler<SqsAddressCorrectPositionRequest>
    {
        private readonly IStreetNames _streetNames;
        private readonly IdempotencyContext _idempotencyContext;

        public SqsAddressCorrectPositionHandler(
            ITicketing ticketing,
            ITicketingUrl ticketingUrl,
            ICommandHandlerResolver bus,
            IStreetNames streetNames,
            IdempotencyContext idempotencyContext)
            : base(ticketing, ticketingUrl, bus)
        {
            _streetNames = streetNames;
            _idempotencyContext = idempotencyContext;
        }

        protected override async Task<string> Handle2(SqsAddressCorrectPositionRequest request, CancellationToken cancellationToken)
        {
            var streetNamePersistentLocalId = new StreetNamePersistentLocalId(int.Parse(request.MessageGroupId));
            var addressPersistentLocalId = new AddressPersistentLocalId(request.PersistentLocalId);

            var cmd = request.ToCommand(
                streetNamePersistentLocalId,
                request.Positie?.ToExtendedWkbGeometry(),
                CreateFakeProvenance());

            await IdempotentCommandHandlerDispatch(
                _idempotencyContext,
                cmd.CreateCommandId(),
                cmd,
                request.Metadata,
                cancellationToken);

            var etag = await GetHash(
                _streetNames,
                streetNamePersistentLocalId,
                addressPersistentLocalId,
                cancellationToken);

            return etag;
        }
    }
}
