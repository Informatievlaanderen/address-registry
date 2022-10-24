namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class ProposeSqsRequest : SqsRequest, IHasBackOfficeRequest<ProposeBackOfficeRequest>
    {
        public ProposeBackOfficeRequest Request { get; set; }
    }
}
