namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Handlers
{
    using Abstractions;
    using Requests;
    using System.Collections.Generic;
    using TicketingService.Abstractions;

    public sealed class SqsAddressChangePositionHandler : SqsHandler<SqsAddressChangePositionRequest>
    {
        public const string Action = "ChangeAddressPosition";

        private readonly BackOfficeContext _backOfficeContext;

        public SqsAddressChangePositionHandler(
            ISqsQueue sqsQueue,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl,
            BackOfficeContext backOfficeContext)
            : base(sqsQueue, ticketing, ticketingUrl)
        {
            _backOfficeContext = backOfficeContext;
        }

        protected override string? WithAggregateId(SqsAddressChangePositionRequest request)
        {
            var relation = _backOfficeContext
                .AddressPersistentIdStreetNamePersistentIds
                .Find(request.PersistentLocalId);

            return relation?.StreetNamePersistentLocalId.ToString();
        }

        protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, SqsAddressChangePositionRequest sqsRequest)
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
