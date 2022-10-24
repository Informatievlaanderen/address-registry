namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using Sqs.Requests;
    using StreetName;
    using StreetName.Commands;

    public record CorrectApprovalLambdaRequest :
        SqsLambdaRequest,
        IHasBackOfficeRequest<BackOfficeCorrectApprovalRequest>,
        Abstractions.IHasAddressPersistentLocalId
    {
        public CorrectApprovalLambdaRequest(string groupId, CorrectApprovalSqsRequest sqsRequest)
            : base(
                groupId,
                sqsRequest.TicketId,
                sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
        }

        public BackOfficeCorrectApprovalRequest Request { get; set; }

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
