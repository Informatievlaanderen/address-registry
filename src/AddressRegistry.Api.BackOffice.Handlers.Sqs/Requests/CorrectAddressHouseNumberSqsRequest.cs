namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class CorrectAddressHouseNumberSqsRequest : SqsRequest, IHasBackOfficeRequest<CorrectAddressHouseNumberBackOfficeRequest>
    {
        public int PersistentLocalId { get; set; }

        public CorrectAddressHouseNumberBackOfficeRequest Request { get; init; }
    }
}
