namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using Sqs.Requests;
    using StreetName;
    using StreetName.Commands;

    public record RetireLambdaRequest :
        SqsLambdaRequest,
        IHasBackOfficeRequest<BackOfficeRetireRequest>,
        Abstractions.IHasAddressPersistentLocalId
    {
        public RetireLambdaRequest(string groupId, RetireSqsRequest sqsRequest)
            : base(
                groupId,
                sqsRequest.TicketId,
                sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
        }

        public BackOfficeRetireRequest Request { get; set; }

        public int AddressPersistentLocalId => Request.PersistentLocalId;

        /// <summary>
        /// Map to RetireAddress command
        /// </summary>
        /// <returns>RetireAddress.</returns>
        public RetireAddress ToCommand()
        {
            return new RetireAddress(
                this.StreetNamePersistentLocalId(),
                new AddressPersistentLocalId(AddressPersistentLocalId),
                Provenance);
        }
    }
}
