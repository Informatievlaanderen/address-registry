namespace AddressRegistry.Api.BackOffice.Handlers
{
    using System;
    using System.Collections.Generic;
    using AddressRegistry.Api.BackOffice.Abstractions.SqsRequests;
    using AddressRegistry.Consumer;
    using Be.Vlaanderen.Basisregisters.Sqs;
    using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
    using TicketingService.Abstractions;

    public class ProposeAddressesForMunicipalityMergerHandler : SqsHandler<ProposeAddressesForMunicipalityMergerSqsRequest>
    {
        private const string Action = "ProposeAddressesForMunicipalityMerger";
        private readonly ConsumerContext _consumerContext;

        public ProposeAddressesForMunicipalityMergerHandler(
            ISqsQueue sqsQueue,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl,
            ConsumerContext consumerContext)
            : base(sqsQueue, ticketing, ticketingUrl)
        {
            _consumerContext = consumerContext;
        }

        protected override string? WithAggregateId(ProposeAddressesForMunicipalityMergerSqsRequest request)
        {
            //TODO-rik find aggregate id, streetname?
            // var municipality = _consumerContext.MunicipalityConsumerItems
            //     .AsNoTracking()
            //     .SingleOrDefault(item => item.NisCode == request.NisCode);
            //
            // return municipality?.MunicipalityId.ToString();
            throw new NotImplementedException();
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
