namespace AddressRegistry.Api.BackOffice.Handlers
{
    using System.Collections.Generic;
    using AddressRegistry.Api.BackOffice.Abstractions;
    using AddressRegistry.Api.BackOffice.Abstractions.SqsRequests;
    using Be.Vlaanderen.Basisregisters.Sqs;
    using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
    using TicketingService.Abstractions;

    public sealed class CorrectAddressRetirementSqsHandler : SqsHandler<CorrectAddressRetirementSqsRequest>
    {
        public const string Action = "CorrectAddressRetirement";
        private readonly BackOfficeContext _backOfficeContext;

        public CorrectAddressRetirementSqsHandler(
            ISqsQueue sqsQueue,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl,
            BackOfficeContext backOfficeContext)
            : base(sqsQueue, ticketing, ticketingUrl)
        {
            _backOfficeContext = backOfficeContext;
        }

        protected override string? WithAggregateId(CorrectAddressRetirementSqsRequest request)
        {
            var relation = _backOfficeContext
                .AddressPersistentIdStreetNamePersistentIds
                .Find(request.Request.PersistentLocalId);

            return relation?.StreetNamePersistentLocalId.ToString();
        }

        protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, CorrectAddressRetirementSqsRequest sqsRequest)
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
