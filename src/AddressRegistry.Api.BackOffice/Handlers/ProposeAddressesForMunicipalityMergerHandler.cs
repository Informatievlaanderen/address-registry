namespace AddressRegistry.Api.BackOffice.Handlers
{
    using System.Collections.Generic;
    using Abstractions.SqsRequests;
    using Be.Vlaanderen.Basisregisters.Sqs;
    using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
    using TicketingService.Abstractions;

    public class ProposeAddressesForMunicipalityMergerHandler : SqsHandler<ProposeAddressesForMunicipalityMergerSqsRequest>
    {
        private const string Action = "ProposeAddressesForMunicipalityMerger";

        public ProposeAddressesForMunicipalityMergerHandler(
            ISqsQueue sqsQueue,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl)
            : base(sqsQueue, ticketing, ticketingUrl)
        {
        }

        protected override string? WithAggregateId(ProposeAddressesForMunicipalityMergerSqsRequest request)
        {
            return request.StreetNamePersistentLocalId.ToString();
        }

        protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, ProposeAddressesForMunicipalityMergerSqsRequest sqsRequest)
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
