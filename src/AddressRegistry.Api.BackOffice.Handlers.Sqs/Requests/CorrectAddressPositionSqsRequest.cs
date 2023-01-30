namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class CorrectAddressPositionSqsRequest : SqsRequest, IHasBackOfficeRequest<CorrectAddressPositionRequest>
    {
        public int PersistentLocalId { get; set; }

        public CorrectAddressPositionRequest Request { get; init; }
    }
}
