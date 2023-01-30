namespace AddressRegistry.Api.BackOffice.Abstractions.SqsRequests
{
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class CorrectAddressHouseNumberSqsRequest : SqsRequest, IHasBackOfficeRequest<CorrectAddressHouseNumberRequest>
    {
        public int PersistentLocalId { get; set; }

        public CorrectAddressHouseNumberRequest Request { get; init; }
    }
}
