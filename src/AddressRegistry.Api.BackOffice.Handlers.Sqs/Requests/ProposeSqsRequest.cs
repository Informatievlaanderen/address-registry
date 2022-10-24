namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class ProposeSqsRequest : SqsRequest, IHasBackOfficeRequest<BackOfficeProposeRequest>
    {
        public BackOfficeProposeRequest Request { get; set; }
    }
}
