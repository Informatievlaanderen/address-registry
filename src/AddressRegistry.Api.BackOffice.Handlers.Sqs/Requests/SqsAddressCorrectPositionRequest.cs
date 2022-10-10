namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;

    public sealed class SqsAddressCorrectPositionRequest : SqsRequest, IHasBackOfficeRequest<AddressBackOfficeCorrectPositionRequest>
    {
        public int PersistentLocalId { get; set; }

        public AddressBackOfficeCorrectPositionRequest Request { get; set; }
    }
}