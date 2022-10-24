namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class CorrectApprovalSqsRequest : SqsRequest, IHasBackOfficeRequest<BackOfficeCorrectApprovalRequest>
    {
        public BackOfficeCorrectApprovalRequest Request { get; set; }
    }
}
