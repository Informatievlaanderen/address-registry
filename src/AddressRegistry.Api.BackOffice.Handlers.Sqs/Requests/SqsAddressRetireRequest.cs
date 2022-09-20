namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;

    public sealed class SqsAddressRetireRequest : SqsRequest, IHasBackOfficeRequest<AddressBackOfficeRetireRequest>
    {
        public AddressBackOfficeRetireRequest Request { get; set; }
    }
}
