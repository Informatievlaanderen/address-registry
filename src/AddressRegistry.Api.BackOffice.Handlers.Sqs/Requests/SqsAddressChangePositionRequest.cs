namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;

    public sealed class SqsAddressChangePositionRequest : SqsRequest, IHasBackOfficeRequest<AddressBackOfficeChangePositionRequest>
    {
        public int PersistentLocalId { get; set; }

        public AddressBackOfficeChangePositionRequest Request { get; set; }
    }
}
