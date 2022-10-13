namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Handlers
{
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo.Extensions;
    using Requests;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Sqs;
    using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
    using TicketingService.Abstractions;

    public sealed class SqsAddressProposeHandler : SqsHandler<SqsAddressProposeRequest>
    {
        public const string Action = "ProposeAddress";

        public SqsAddressProposeHandler(
            ISqsQueue sqsQueue,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl)
            : base(sqsQueue, ticketing, ticketingUrl)
        {
        }

        protected override string WithAggregateId(SqsAddressProposeRequest request)
        {
            var identifier = request.Request.StraatNaamId
                .AsIdentifier()
                .Map(x => x);

            return identifier.Value;
        }

        protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, SqsAddressProposeRequest sqsRequest)
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
