namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class SqsAddressCorrectPositionRequest : SqsRequest, IHasBackOfficeRequest<AddressBackOfficeCorrectPositionRequest>
    {
        public int PersistentLocalId { get; set; }

        public AddressBackOfficeCorrectPositionRequest Request { get; set; }
    }
}
