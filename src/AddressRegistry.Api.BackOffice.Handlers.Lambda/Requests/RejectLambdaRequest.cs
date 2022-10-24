namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using Sqs.Requests;
    using StreetName;
    using StreetName.Commands;

    public record RejectLambdaRequest :
        SqsLambdaRequest,
        IHasBackOfficeRequest<BackOfficeRejectRequest>,
        Abstractions.IHasAddressPersistentLocalId
    {
        public RejectLambdaRequest(string groupId, RejectSqsRequest sqsRequest)
            : base(
                groupId,
                sqsRequest.TicketId,
                sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
        }

        public BackOfficeRejectRequest Request { get; set; }

        public int AddressPersistentLocalId => Request.PersistentLocalId;

        /// <summary>
        /// Map to RejectAddress command
        /// </summary>
        /// <returns>RejectAddress.</returns>
        public RejectAddress ToCommand()
        {
            return new RejectAddress(
                this.StreetNamePersistentLocalId(),
                new AddressPersistentLocalId(AddressPersistentLocalId),
                Provenance);
        }
    }
}
