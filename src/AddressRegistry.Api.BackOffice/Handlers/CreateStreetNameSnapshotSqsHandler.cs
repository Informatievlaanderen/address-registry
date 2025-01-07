namespace AddressRegistry.Api.BackOffice.Handlers
{
    using System.Collections.Generic;
    using System.Linq;
    using Abstractions;
    using Abstractions.SqsRequests;
    using Be.Vlaanderen.Basisregisters.Sqs;
    using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
    using TicketingService.Abstractions;

    public sealed class CreateStreetNameSnapshotSqsHandler : SqsHandler<CreateStreetNameSnapshotSqsRequest>
    {
        public const string Action = "CreateStreetNameSnapshot";

        private readonly BackOfficeContext _backOfficeContext;

        public CreateStreetNameSnapshotSqsHandler(
            ISqsQueue sqsQueue,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl,
            BackOfficeContext backOfficeContext)
            : base(sqsQueue, ticketing, ticketingUrl)
        {
            _backOfficeContext = backOfficeContext;
        }

        protected override string? WithAggregateId(CreateStreetNameSnapshotSqsRequest request)
        {
            return _backOfficeContext
                .AddressPersistentIdStreetNamePersistentIds
                .FirstOrDefault(x =>
                    x.StreetNamePersistentLocalId == request.Request.StreetNamePersistentLocalId)
                ?.StreetNamePersistentLocalId.ToString();
        }

        protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, CreateStreetNameSnapshotSqsRequest sqsRequest)
        {
            return new Dictionary<string, string>
            {
                { RegistryKey, nameof(AddressRegistry) },
                { ActionKey, Action },
                { AggregateIdKey, aggregateId }
            };
        }
    }
}
