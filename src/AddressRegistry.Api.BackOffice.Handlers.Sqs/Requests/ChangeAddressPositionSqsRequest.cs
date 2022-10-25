namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class ChangeAddressPositionSqsRequest : SqsRequest, IHasBackOfficeRequest<ChangeAddressPositionBackOfficeRequest>
    {
        public int PersistentLocalId { get; set; }

        public ChangeAddressPositionBackOfficeRequest Request { get; init; }
    }
}
