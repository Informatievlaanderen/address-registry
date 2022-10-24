namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using Sqs.Requests;
    using StreetName;
    using StreetName.Commands;

    public record DeregulateLambdaRequest :
        SqsLambdaRequest,
        IHasBackOfficeRequest<DeregulateBackOfficeRequest>,
        Abstractions.IHasAddressPersistentLocalId
    {
        public DeregulateLambdaRequest(string groupId, DeregulateSqsRequest sqsRequest)
            : base(
                groupId,
                sqsRequest.TicketId,
                sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
        }

        public DeregulateBackOfficeRequest Request { get; init; }

        public int AddressPersistentLocalId => Request.PersistentLocalId;

        /// <summary>
        /// Map to DeregulateAddress command
        /// </summary>
        /// <returns>DeregulateAddress.</returns>
        public DeregulateAddress ToCommand()
        {
            return new DeregulateAddress(
                this.StreetNamePersistentLocalId(),
                new AddressPersistentLocalId(AddressPersistentLocalId),
                Provenance);
        }
    }
}
