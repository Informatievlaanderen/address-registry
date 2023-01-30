namespace AddressRegistry.Api.BackOffice.Abstractions.SqsRequests
{
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class CorrectAddressRejectionSqsRequest : SqsRequest, IHasBackOfficeRequest<CorrectAddressRejectionRequest>
    {
        public CorrectAddressRejectionRequest Request { get; init; }
    }
}
