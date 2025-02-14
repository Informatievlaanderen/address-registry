namespace AddressRegistry.Api.BackOffice.Abstractions.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;

    public sealed class CorrectAddressBoxNumberSqsRequest : SqsRequest, IHasBackOfficeRequest<CorrectAddressBoxNumberRequest>
    {
        public int PersistentLocalId { get; set; }

        public CorrectAddressBoxNumberRequest Request { get; init; }
    }
}
