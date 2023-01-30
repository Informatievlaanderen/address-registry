namespace AddressRegistry.Api.BackOffice.Abstractions.SqsRequests
{
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class ChangeAddressPositionSqsRequest : SqsRequest, IHasBackOfficeRequest<ChangeAddressPositionRequest>
    {
        public int PersistentLocalId { get; set; }

        public ChangeAddressPositionRequest Request { get; init; }
    }
}
