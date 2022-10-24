namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class ChangePostalCodeSqsRequest : SqsRequest, IHasBackOfficeRequest<BackOfficeChangePostalCodeRequest>
    {
        public int PersistentLocalId { get; set; }

        public BackOfficeChangePostalCodeRequest Request { get; set; }
    }
}
