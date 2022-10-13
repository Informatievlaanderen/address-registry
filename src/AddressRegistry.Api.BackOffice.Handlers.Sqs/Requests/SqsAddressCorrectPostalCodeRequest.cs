namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class SqsAddressCorrectPostalCodeRequest : SqsRequest, IHasBackOfficeRequest<AddressBackOfficeCorrectPostalCodeRequest>
    {
        public int PersistentLocalId { get; set; }
        public AddressBackOfficeCorrectPostalCodeRequest Request { get; set; }
    }
}
