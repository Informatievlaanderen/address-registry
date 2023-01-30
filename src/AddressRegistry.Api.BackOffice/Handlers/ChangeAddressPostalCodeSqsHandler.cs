namespace AddressRegistry.Api.BackOffice.Handlers
{
    using System.Collections.Generic;
    using AddressRegistry.Api.BackOffice.Abstractions;
    using AddressRegistry.Api.BackOffice.Abstractions.SqsRequests;
    using Be.Vlaanderen.Basisregisters.Sqs;
    using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
    using TicketingService.Abstractions;

    public sealed class ChangeAddressPostalCodeSqsHandler : SqsHandler<ChangeAddressPostalCodeSqsRequest>
    {
        public const string Action = "ChangeAddressPostalCode";

        private readonly BackOfficeContext _backOfficeContext;

        public ChangeAddressPostalCodeSqsHandler(
            ISqsQueue sqsQueue,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl,
            BackOfficeContext backOfficeContext)
            : base(sqsQueue, ticketing, ticketingUrl)
        {
            _backOfficeContext = backOfficeContext;
        }

        protected override string? WithAggregateId(ChangeAddressPostalCodeSqsRequest request)
        {
            var relation = _backOfficeContext
                .AddressPersistentIdStreetNamePersistentIds
                .Find(request.PersistentLocalId);

            return relation?.StreetNamePersistentLocalId.ToString();
        }

        protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, ChangeAddressPostalCodeSqsRequest sqsRequest)
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
