namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class SqsAddressChangePostalCodeRequest : SqsRequest, IHasBackOfficeRequest<AddressBackOfficeChangePostalCodeRequest>
    {
        public int PersistentLocalId { get; set; }

        public AddressBackOfficeChangePostalCodeRequest Request { get; set; }
    }
}
