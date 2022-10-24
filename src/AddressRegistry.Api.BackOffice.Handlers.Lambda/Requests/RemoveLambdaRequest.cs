namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using Sqs.Requests;
    using StreetName;
    using StreetName.Commands;

    public record RemoveLambdaRequest :
        SqsLambdaRequest,
        IHasBackOfficeRequest<RemoveBackOfficeRequest>,
        Abstractions.IHasAddressPersistentLocalId
    {
        public RemoveLambdaRequest(string groupId, RemoveSqsRequest sqsRequest)
            : base(
                groupId,
                sqsRequest.TicketId,
                sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
        }

        public RemoveBackOfficeRequest Request { get; init; }

        public int AddressPersistentLocalId => Request.PersistentLocalId;

        /// <summary>
        /// Map to RemoveAddress command
        /// </summary>
        /// <returns>RemoveAddress.</returns>
        public RemoveAddress ToCommand()
        {
            return new RemoveAddress(
                this.StreetNamePersistentLocalId(),
                new AddressPersistentLocalId(AddressPersistentLocalId),
                Provenance);
        }
    }
}
