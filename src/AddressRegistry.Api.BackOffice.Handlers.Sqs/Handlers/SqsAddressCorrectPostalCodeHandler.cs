namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Handlers
{
    using Abstractions;
    using Requests;
    using System.Collections.Generic;
    using TicketingService.Abstractions;

    public sealed class SqsAddressCorrectPostalCodeHandler : SqsHandler<SqsAddressCorrectPostalCodeRequest>
    {
        public const string Action = "CorrectAddressPostalCode";

        private readonly BackOfficeContext _backOfficeContext;

        public SqsAddressCorrectPostalCodeHandler(
            ISqsQueue sqsQueue,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl,
            BackOfficeContext backOfficeContext)
            : base(sqsQueue, ticketing, ticketingUrl)
        {
            _backOfficeContext = backOfficeContext;
        }

        protected override string? WithAggregateId(SqsAddressCorrectPostalCodeRequest request)
        {
            var relation = _backOfficeContext
                .AddressPersistentIdStreetNamePersistentIds
                .Find(request.PersistentLocalId);

            return relation?.StreetNamePersistentLocalId.ToString();
        }

        protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, SqsAddressCorrectPostalCodeRequest sqsRequest)
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
