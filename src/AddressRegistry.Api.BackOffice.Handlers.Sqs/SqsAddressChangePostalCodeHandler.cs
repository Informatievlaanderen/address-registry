namespace AddressRegistry.Api.BackOffice.Handlers.Sqs
{
    using System.Linq;
    using Abstractions;
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
    using Microsoft.Extensions.Logging;
    using TicketingService.Abstractions;

    public class SqsAddressChangePostalCodeHandler : SqsHandler<SqsAddressChangePostalCodeRequest>
    {
        private readonly BackOfficeContext _backOfficeContext;

        public SqsAddressChangePostalCodeHandler(
            ILogger<SqsAddressChangePostalCodeHandler> logger,
            SqsOptions sqsOptions,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl,
            BackOfficeContext backOfficeContext)
            : base (logger, sqsOptions, ticketing, ticketingUrl)
        {
            _backOfficeContext = backOfficeContext;
        }

        protected override string WithGroupId(SqsAddressChangePostalCodeRequest request)
        {
            var relation = _backOfficeContext.AddressPersistentIdStreetNamePersistentIds
                .Single(x => x.AddressPersistentLocalId == request.PersistentLocalId);

            return relation.StreetNamePersistentLocalId.ToString();
        }
    }
}
