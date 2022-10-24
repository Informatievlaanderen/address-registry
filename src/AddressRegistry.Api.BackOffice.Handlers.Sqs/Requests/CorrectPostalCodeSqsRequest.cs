namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class CorrectPostalCodeSqsRequest : SqsRequest, IHasBackOfficeRequest<BackOfficeCorrectPostalCodeRequest>
    {
        public int PersistentLocalId { get; set; }
        public BackOfficeCorrectPostalCodeRequest Request { get; set; }
    }
}
