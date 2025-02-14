namespace AddressRegistry.Api.BackOffice.Abstractions.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;

    public sealed class CorrectAddressBoxNumbersSqsRequest : SqsRequest, IHasBackOfficeRequest<CorrectAddressBoxNumbersRequest>
    {
        public CorrectAddressBoxNumbersRequest Request { get; init; }
    }
}
