namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;

    public sealed class SqsAddressCorrectApprovalRequest : SqsRequest, IHasBackOfficeRequest<AddressBackOfficeCorrectApprovalRequest>
    {
        public AddressBackOfficeCorrectApprovalRequest Request { get; set; }
    }
}
