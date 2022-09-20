namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;

    public sealed class SqsAddressApproveRequest : SqsRequest, IHasBackOfficeRequest<AddressBackOfficeApproveRequest>
    {
        public AddressBackOfficeApproveRequest Request { get; set; }
    }
}
