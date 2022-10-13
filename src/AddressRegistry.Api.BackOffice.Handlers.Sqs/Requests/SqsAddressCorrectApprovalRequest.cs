namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class SqsAddressCorrectApprovalRequest : SqsRequest, IHasBackOfficeRequest<AddressBackOfficeCorrectApprovalRequest>
    {
        public AddressBackOfficeCorrectApprovalRequest Request { get; set; }
    }
}
