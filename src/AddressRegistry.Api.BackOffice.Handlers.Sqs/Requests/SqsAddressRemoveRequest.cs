namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;

    public sealed class SqsAddressRemoveRequest : SqsRequest, IHasBackOfficeRequest<AddressBackOfficeRemoveRequest>
    {
        public AddressBackOfficeRemoveRequest? Request { get; set; }
    }
}
