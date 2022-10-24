namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class CorrectAddressBoxNumberSqsRequest : SqsRequest, IHasBackOfficeRequest<CorrectAddressBoxNumberBackOfficeRequest>
    {
        public int PersistentLocalId { get; set; }

        public CorrectAddressBoxNumberBackOfficeRequest Request { get; init; }
    }
}
