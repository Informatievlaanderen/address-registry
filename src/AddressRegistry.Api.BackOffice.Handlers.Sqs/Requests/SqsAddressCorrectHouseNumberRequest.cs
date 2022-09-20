namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;

    public sealed class SqsAddressCorrectHouseNumberRequest : SqsRequest, IHasBackOfficeRequest<AddressBackOfficeCorrectHouseNumberRequest>
    {
        public int PersistentLocalId { get; set; }

        public AddressBackOfficeCorrectHouseNumberRequest Request { get; set; }
    }
}
