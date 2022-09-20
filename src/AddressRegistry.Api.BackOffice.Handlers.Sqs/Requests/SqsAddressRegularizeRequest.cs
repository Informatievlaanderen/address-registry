namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;

    public sealed class SqsAddressRegularizeRequest : SqsRequest, IHasBackOfficeRequest<AddressBackOfficeRegularizeRequest>
    {
        public AddressBackOfficeRegularizeRequest Request { get; set; }
    }
}
