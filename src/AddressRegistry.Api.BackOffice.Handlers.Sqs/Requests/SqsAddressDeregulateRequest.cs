namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;

    public sealed class SqsAddressDeregulateRequest : SqsRequest, IHasBackOfficeRequest<AddressBackOfficeDeregulateRequest>
    {
        public AddressBackOfficeDeregulateRequest Request { get; set; }
    }
}
