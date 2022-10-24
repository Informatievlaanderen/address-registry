namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class ApproveSqsRequest : SqsRequest, IHasBackOfficeRequest<BackOfficeApproveRequest>
    {
        public BackOfficeApproveRequest Request { get; set; }
    }
}
