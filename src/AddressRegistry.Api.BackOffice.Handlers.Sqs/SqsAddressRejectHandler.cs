namespace AddressRegistry.Api.BackOffice.Handlers.Sqs
{
    using System.Linq;
    using Abstractions;
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
    using TicketingService.Abstractions;

    public class SqsAddressRejectHandler : SqsHandler<SqsAddressRejectRequest>
    {
        private readonly BackOfficeContext _backOfficeContext;

        public SqsAddressRejectHandler(
            SqsOptions sqsOptions,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl,
            BackOfficeContext backOfficeContext)
            : base (sqsOptions, ticketing, ticketingUrl)
        {
            _backOfficeContext = backOfficeContext;
        }

        protected override string WithGroupId(SqsAddressRejectRequest request)
        {
            var relation = _backOfficeContext.AddressPersistentIdStreetNamePersistentIds
                .Single(x => x.AddressPersistentLocalId == request.PersistentLocalId);

            return relation.StreetNamePersistentLocalId.ToString();
        }
    }
}
