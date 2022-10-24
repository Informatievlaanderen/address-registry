namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Handlers
{
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo.Extensions;
    using Requests;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Sqs;
    using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
    using TicketingService.Abstractions;

    public sealed class ProposeSqsHandler : SqsHandler<ProposeSqsRequest>
    {
        public const string Action = "ProposeAddress";

        public ProposeSqsHandler(
            ISqsQueue sqsQueue,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl)
            : base(sqsQueue, ticketing, ticketingUrl)
        {
        }

        protected override string WithAggregateId(ProposeSqsRequest request)
        {
            var identifier = request.Request.StraatNaamId
                .AsIdentifier()
                .Map(x => x);

            return identifier.Value;
        }

        protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, ProposeSqsRequest sqsRequest)
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
