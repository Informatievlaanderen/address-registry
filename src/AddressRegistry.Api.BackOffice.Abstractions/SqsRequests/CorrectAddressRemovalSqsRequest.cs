namespace AddressRegistry.Api.BackOffice.Abstractions.SqsRequests
{
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class CorrectAddressRemovalSqsRequest : SqsRequest, IHasBackOfficeRequest<CorrectAddressRemovalRequest>
    {
        public CorrectAddressRemovalRequest Request { get; init; }
    }
}
