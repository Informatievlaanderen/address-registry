namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class CorrectAddressApprovalSqsRequest : SqsRequest, IHasBackOfficeRequest<CorrectAddressApprovalRequest>
    {
        public CorrectAddressApprovalRequest Request { get; init; }
    }
}
