namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using Sqs.Requests;
    using StreetName;
    using StreetName.Commands;

    public sealed record CorrectAddressApprovalLambdaRequest :
        SqsLambdaRequest,
        IHasBackOfficeRequest<CorrectAddressApprovalRequest>,
        Abstractions.IHasAddressPersistentLocalId
    {
        public CorrectAddressApprovalLambdaRequest(string groupId, CorrectAddressApprovalSqsRequest sqsRequest)
            : base(
                groupId,
                sqsRequest.TicketId,
                sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
        }

        public CorrectAddressApprovalRequest Request { get; init; }

        public int AddressPersistentLocalId => Request.PersistentLocalId;

        /// <summary>
        /// Map to CorrectAddressApproval command.
        /// </summary>
        /// <returns>CorrectAddressApproval.</returns>
        public CorrectAddressApproval ToCommand()
        {
            return new CorrectAddressApproval(this.StreetNamePersistentLocalId(), new AddressPersistentLocalId(AddressPersistentLocalId), Provenance);
        }
    }
}
