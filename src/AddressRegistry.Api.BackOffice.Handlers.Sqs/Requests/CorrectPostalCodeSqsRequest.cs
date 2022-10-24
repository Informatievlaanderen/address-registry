namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class CorrectPostalCodeSqsRequest : SqsRequest, IHasBackOfficeRequest<CorrectPostalCodeBackOfficeRequest>
    {
        public int PersistentLocalId { get; set; }
        public CorrectPostalCodeBackOfficeRequest Request { get; set; }
    }
}
