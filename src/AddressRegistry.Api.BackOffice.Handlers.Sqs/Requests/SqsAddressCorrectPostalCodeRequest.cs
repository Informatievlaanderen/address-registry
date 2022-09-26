namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;

    public sealed class SqsAddressCorrectPostalCodeRequest : SqsRequest, IHasBackOfficeRequest<AddressBackOfficeCorrectPostalCodeRequest>
    {
        public int PersistentLocalId { get; set; }
        public AddressBackOfficeCorrectPostalCodeRequest Request { get; set; }
    }
}
