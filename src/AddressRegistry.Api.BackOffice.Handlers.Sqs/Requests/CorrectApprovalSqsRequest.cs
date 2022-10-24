namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class CorrectApprovalSqsRequest : SqsRequest, IHasBackOfficeRequest<CorrectApprovalBackOfficeRequest>
    {
        public CorrectApprovalBackOfficeRequest Request { get; set; }
    }
}
