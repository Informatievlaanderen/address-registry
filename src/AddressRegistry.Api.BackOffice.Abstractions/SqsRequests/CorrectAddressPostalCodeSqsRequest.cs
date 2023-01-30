namespace AddressRegistry.Api.BackOffice.Abstractions.SqsRequests
{
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class CorrectAddressPostalCodeSqsRequest : SqsRequest, IHasBackOfficeRequest<CorrectAddressPostalCodeRequest>
    {
        public int PersistentLocalId { get; set; }
        public CorrectAddressPostalCodeRequest Request { get; init; }
    }
}
