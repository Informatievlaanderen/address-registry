namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Requests;
    using Abstractions.SqsRequests;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using StreetName;
    using StreetName.Commands;

    public sealed record CreateStreetNameSnapshotLambdaRequest :
        SqsLambdaRequest,
        IHasBackOfficeRequest<CreateStreetNameSnapshotRequest>
    {
        public CreateStreetNameSnapshotRequest Request { get; init; }

        public CreateStreetNameSnapshotLambdaRequest(
            string messageGroupId,
            CreateStreetNameSnapshotSqsRequest sqsRequest)
            : base(
                messageGroupId,
                sqsRequest.TicketId,
                null,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
        }

        /// <summary>
        /// Map to CreateSnapshot command.
        /// </summary>
        /// <returns>CreateSnapshot</returns>
        public CreateSnapshot ToCommand()
        {
            return new CreateSnapshot(
                new StreetNamePersistentLocalId(Request.StreetNamePersistentLocalId),
                Provenance);
        }
    }
}
