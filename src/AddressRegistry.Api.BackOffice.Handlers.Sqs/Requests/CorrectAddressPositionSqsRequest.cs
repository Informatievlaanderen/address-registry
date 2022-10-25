namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class CorrectAddressPositionSqsRequest : SqsRequest, IHasBackOfficeRequest<CorrectAddressPositionBackOfficeRequest>
    {
        public int PersistentLocalId { get; set; }

        public CorrectAddressPositionBackOfficeRequest Request { get; init; }
    }
}
