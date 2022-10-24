namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class RejectSqsRequest : SqsRequest, IHasBackOfficeRequest<RejectBackOfficeRequest>
    {
        public RejectBackOfficeRequest Request { get; set; }
    }
}
