namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using Sqs.Requests;
    using StreetName;
    using StreetName.Commands;

    public record CorrectRejectionLambdaRequest :
        SqsLambdaRequest,
        IHasBackOfficeRequest<CorrectRejectionBackOfficeRequest>,
        Abstractions.IHasAddressPersistentLocalId
    {
        public CorrectRejectionLambdaRequest(string groupId, CorrectRejectionSqsRequest sqsRequest)
            : base(
                groupId,
                sqsRequest.TicketId,
                sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
        }

        public CorrectRejectionBackOfficeRequest Request { get; set; }

        public int AddressPersistentLocalId => Request.PersistentLocalId;

        /// <summary>
        /// Map to CorrectAddressRejection command
        /// </summary>
        /// <returns>CorrectAddressRejection.</returns>
        public CorrectAddressRejection ToCommand()
        {
            return new CorrectAddressRejection(
                this.StreetNamePersistentLocalId(),
                new AddressPersistentLocalId(AddressPersistentLocalId),
                Provenance);
        }
    }
}
