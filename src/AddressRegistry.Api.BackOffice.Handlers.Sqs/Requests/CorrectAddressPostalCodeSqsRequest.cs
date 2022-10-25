namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class CorrectAddressPostalCodeSqsRequest : SqsRequest, IHasBackOfficeRequest<CorrectAddressPostalCodeBackOfficeRequest>
    {
        public int PersistentLocalId { get; set; }
        public CorrectAddressPostalCodeBackOfficeRequest Request { get; init; }
    }
}
