namespace AddressRegistry.Api.BackOffice.Abstractions.SqsRequests
{
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class CorrectAddressApprovalSqsRequest : SqsRequest, IHasBackOfficeRequest<CorrectAddressApprovalRequest>
    {
        public CorrectAddressApprovalRequest Request { get; init; }
    }
}
