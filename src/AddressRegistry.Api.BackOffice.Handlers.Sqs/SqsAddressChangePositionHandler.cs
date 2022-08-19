namespace AddressRegistry.Api.BackOffice.Handlers.Sqs
{
    using System.Linq;
    using Abstractions;
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
    using Microsoft.Extensions.Logging;
    using TicketingService.Abstractions;

    public class SqsAddressChangePositionHandler : SqsHandler<SqsAddressChangePositionRequest>
    {
        private readonly BackOfficeContext _backOfficeContext;

        public SqsAddressChangePositionHandler(
            ILogger<SqsAddressChangePositionHandler> logger,
            SqsOptions sqsOptions,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl,
            BackOfficeContext backOfficeContext)
            : base (logger, sqsOptions, ticketing, ticketingUrl)
        {
            _backOfficeContext = backOfficeContext;
        }

        protected override string WithGroupId(SqsAddressChangePositionRequest request)
        {
            var relation = _backOfficeContext.AddressPersistentIdStreetNamePersistentIds
                .Single(x => x.AddressPersistentLocalId == request.PersistentLocalId);

            return relation.StreetNamePersistentLocalId.ToString();
        }
    }
}
