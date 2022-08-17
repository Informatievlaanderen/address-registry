namespace AddressRegistry.Api.BackOffice.Handlers.Sqs
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo.Extensions;
    using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
    using Microsoft.Extensions.Logging;
    using TicketingService.Abstractions;

    public class SqsAddressProposeHandler : SqsHandler<SqsAddressProposeRequest>
    {
        public SqsAddressProposeHandler(
            ILogger<SqsAddressProposeHandler> logger,
            SqsOptions sqsOptions,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl)
            : base (logger, sqsOptions, ticketing, ticketingUrl)
        { }

        protected override string WithGroupId(SqsAddressProposeRequest request)
        {
            return request.StraatNaamId
                .AsIdentifier()
                .Map(x => x);
        }
    }
}
