namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;

    public sealed class SqsAddressRejectRequest : SqsRequest, IHasBackOfficeRequest<AddressBackOfficeRejectRequest>
    {
        public AddressBackOfficeRejectRequest Request { get; set; }
    }
}
