namespace AddressRegistry.Api.BackOffice.Handlers
{
    using System.Collections.Generic;
    using Abstractions;
    using Abstractions.SqsRequests;
    using Be.Vlaanderen.Basisregisters.Sqs;
    using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
    using TicketingService.Abstractions;

    public sealed class RejectAddressHandler : SqsHandler<RejectAddressSqsRequest>
    {
        public const string Action = "RejectAddress";

        private readonly BackOfficeContext _backOfficeContext;

        public RejectAddressHandler(
            ISqsQueue sqsQueue,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl,
            BackOfficeContext backOfficeContext)
            : base(sqsQueue, ticketing, ticketingUrl)
        {
            _backOfficeContext = backOfficeContext;
        }

        protected override string? WithAggregateId(RejectAddressSqsRequest request)
        {
            var relation = _backOfficeContext
                .AddressPersistentIdStreetNamePersistentIds
                .Find(request.Request.PersistentLocalId);

            return relation?.StreetNamePersistentLocalId.ToString();
        }

        protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, RejectAddressSqsRequest sqsRequest)
        {
            return new Dictionary<string, string>
            {
                { RegistryKey, nameof(AddressRegistry) },
                { ActionKey, Action },
                { AggregateIdKey, aggregateId },
                { ObjectIdKey, sqsRequest.Request.PersistentLocalId.ToString() }
            };
        }
    }
}
