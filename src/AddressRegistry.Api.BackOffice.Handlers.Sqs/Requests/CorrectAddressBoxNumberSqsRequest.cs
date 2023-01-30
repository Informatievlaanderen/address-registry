namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class CorrectAddressBoxNumberSqsRequest : SqsRequest, IHasBackOfficeRequest<CorrectAddressBoxNumberRequest>
    {
        public int PersistentLocalId { get; set; }

        public CorrectAddressBoxNumberRequest Request { get; init; }
    }
}
