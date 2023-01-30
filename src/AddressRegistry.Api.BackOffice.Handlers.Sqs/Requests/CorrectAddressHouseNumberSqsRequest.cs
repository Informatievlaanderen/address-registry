namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class CorrectAddressHouseNumberSqsRequest : SqsRequest, IHasBackOfficeRequest<CorrectAddressHouseNumberRequest>
    {
        public int PersistentLocalId { get; set; }

        public CorrectAddressHouseNumberRequest Request { get; init; }
    }
}
