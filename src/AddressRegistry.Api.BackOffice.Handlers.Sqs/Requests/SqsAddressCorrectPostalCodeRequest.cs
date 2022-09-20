namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;

    public sealed class SqsAddressCorrectPostalCodeRequest : SqsRequest, IHasBackOfficeRequest<AddressBackOfficeCorrectPostalCodeRequest>
    {
        public AddressBackOfficeCorrectPostalCodeRequest Request { get; set; }
    }
}
