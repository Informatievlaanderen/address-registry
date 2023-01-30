namespace AddressRegistry.Api.BackOffice.Handlers
{
    using System.Collections.Generic;
    using Abstractions.SqsRequests;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo.Extensions;
    using Be.Vlaanderen.Basisregisters.Sqs;
    using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
    using TicketingService.Abstractions;

    public sealed class ProposeAddressHandler : SqsHandler<ProposeAddressSqsRequest>
    {
        public const string Action = "ProposeAddress";

        public ProposeAddressHandler(
            ISqsQueue sqsQueue,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl)
            : base(sqsQueue, ticketing, ticketingUrl)
        {
        }

        protected override string WithAggregateId(ProposeAddressSqsRequest request)
        {
            var identifier = request.Request.StraatNaamId
                .AsIdentifier()
                .Map(x => x);

            return identifier.Value;
        }

        protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, ProposeAddressSqsRequest sqsRequest)
        {
            return new Dictionary<string, string>
            {
                { RegistryKey, nameof(AddressRegistry) },
                { ActionKey, Action },
                { AggregateIdKey, aggregateId },
            };
        }
    }
}
