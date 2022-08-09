namespace AddressRegistry.Api.BackOffice.Handlers.Sqs
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo.Extensions;
    using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
    using TicketingService.Abstractions;

    public class SqsAddressProposeHandler : SqsHandler<SqsAddressProposeRequest>
    {
        public SqsAddressProposeHandler(
            SqsOptions sqsOptions,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl)
            : base (sqsOptions, ticketing, ticketingUrl)
        { }

        protected override string WithGroupId(SqsAddressProposeRequest request)
        {
            return request.StraatNaamId
                .AsIdentifier()
                .Map(x => x);
        }
    }
}
