namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;

    public sealed class SqsAddressCorrectBoxNumberRequest : SqsRequest, IHasBackOfficeRequest<AddressBackOfficeCorrectBoxNumberRequest>
    {
        public int PersistentLocalId { get; set; }

        public AddressBackOfficeCorrectBoxNumberRequest Request { get; set; }
    }
}
