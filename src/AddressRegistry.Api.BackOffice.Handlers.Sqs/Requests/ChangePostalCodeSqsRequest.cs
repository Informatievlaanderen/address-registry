namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class ChangePostalCodeSqsRequest : SqsRequest, IHasBackOfficeRequest<ChangePostalCodeBackOfficeRequest>
    {
        public int PersistentLocalId { get; set; }

        public ChangePostalCodeBackOfficeRequest Request { get; set; }
    }
}
