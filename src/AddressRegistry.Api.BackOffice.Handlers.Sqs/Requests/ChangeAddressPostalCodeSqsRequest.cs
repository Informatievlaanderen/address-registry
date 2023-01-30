namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class ChangeAddressPostalCodeSqsRequest : SqsRequest, IHasBackOfficeRequest<ChangeAddressPostalCodeRequest>
    {
        public int PersistentLocalId { get; set; }

        public ChangeAddressPostalCodeRequest Request { get; init; }
    }
}
