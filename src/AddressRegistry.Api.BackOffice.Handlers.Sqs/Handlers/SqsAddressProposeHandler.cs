namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Handlers
{
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo.Extensions;
    using Consumer.Read.Municipality;
    using Microsoft.EntityFrameworkCore;
    using Requests;
    using System.Collections.Generic;
    using System.Linq;
    using TicketingService.Abstractions;

    public sealed class SqsAddressProposeHandler : SqsHandler<SqsAddressProposeRequest>
    {
        public const string Action = "ProposeAddress";

        private readonly MunicipalityConsumerContext _municipalityConsumerContext;

        public SqsAddressProposeHandler(
            ISqsQueue sqsQueue,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl,
            MunicipalityConsumerContext municipalityConsumerContext)
            : base(sqsQueue, ticketing, ticketingUrl)
        {
            _municipalityConsumerContext = municipalityConsumerContext;
        }

        protected override string? WithAggregateId(SqsAddressProposeRequest request)
        {
            var identifier = request
                .Request
                .PostInfoId
                .AsIdentifier()
                .Map(x => x);

            var municipality = _municipalityConsumerContext.MunicipalityLatestItems
                .AsNoTracking()
                .SingleOrDefault(item => item.NisCode == identifier.Value);

            return municipality?.MunicipalityId.ToString();
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
