namespace AddressRegistry.Api.BackOffice.Handlers
{
    using System.Collections.Generic;
    using AddressRegistry.Api.BackOffice.Abstractions;
    using AddressRegistry.Api.BackOffice.Abstractions.SqsRequests;
    using Be.Vlaanderen.Basisregisters.Sqs;
    using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
    using TicketingService.Abstractions;

    public sealed class ChangeAddressPositionSqsHandler : SqsHandler<ChangeAddressPositionSqsRequest>
    {
        public const string Action = "ChangeAddressPosition";

        private readonly BackOfficeContext _backOfficeContext;

        public ChangeAddressPositionSqsHandler(
            ISqsQueue sqsQueue,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl,
            BackOfficeContext backOfficeContext)
            : base(sqsQueue, ticketing, ticketingUrl)
        {
            _backOfficeContext = backOfficeContext;
        }

        protected override string? WithAggregateId(ChangeAddressPositionSqsRequest request)
        {
            var relation = _backOfficeContext
                .AddressPersistentIdStreetNamePersistentIds
                .Find(request.PersistentLocalId);

            return relation?.StreetNamePersistentLocalId.ToString();
        }

        protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, ChangeAddressPositionSqsRequest sqsRequest)
        {
            return new Dictionary<string, string>
            {
                { RegistryKey, nameof(AddressRegistry) },
                { ActionKey, Action },
                { AggregateIdKey, aggregateId },
                { ObjectIdKey, sqsRequest.PersistentLocalId.ToString() }
            };
        }
    }
}
