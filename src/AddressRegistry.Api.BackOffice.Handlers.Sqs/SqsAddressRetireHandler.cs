namespace AddressRegistry.Api.BackOffice.Handlers.Sqs
{
    using System.Linq;
    using Abstractions;
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
    using TicketingService.Abstractions;

    public class SqsAddressRetireHandler : SqsHandler<SqsAddressRetireRequest>
    {
        private readonly BackOfficeContext _backOfficeContext;

        public SqsAddressRetireHandler(
            SqsOptions sqsOptions,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl,
            BackOfficeContext backOfficeContext)
            : base (sqsOptions, ticketing, ticketingUrl)
        {
            _backOfficeContext = backOfficeContext;
        }

        protected override string WithGroupId(SqsAddressRetireRequest request)
        {
            var relation = _backOfficeContext.AddressPersistentIdStreetNamePersistentIds
                .Single(x => x.AddressPersistentLocalId == request.PersistentLocalId);

            return relation.StreetNamePersistentLocalId.ToString();
        }
    }
}
