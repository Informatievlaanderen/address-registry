namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions;
    using Abstractions.Converters;
    using Abstractions.Requests;
    using Abstractions.SqsRequests;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using StreetName;
    using StreetName.Commands;

    public sealed record CorrectAddressPositionLambdaRequest :
        SqsLambdaRequest,
        IHasBackOfficeRequest<CorrectAddressPositionRequest>,
        Abstractions.IHasAddressPersistentLocalId
    {
        public CorrectAddressPositionLambdaRequest(string groupId, CorrectAddressPositionSqsRequest sqsRequest)
            : base(
                groupId,
                sqsRequest.TicketId,
                sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
            AddressPersistentLocalId = sqsRequest.PersistentLocalId;
        }

        public CorrectAddressPositionRequest Request { get; init; }

        public int AddressPersistentLocalId { get; }

        /// <summary>
        /// Map to CorrectAddressPosition command
        /// </summary>
        /// <returns>CorrectAddressPosition.</returns>
        public CorrectAddressPosition ToCommand()
        {
            return new CorrectAddressPosition(
                this.StreetNamePersistentLocalId(),
                new AddressPersistentLocalId(AddressPersistentLocalId),
                Request.PositieGeometrieMethode.Map(),
                Request.PositieSpecificatie.Map(),
                Request.Positie.ToExtendedWkbGeometry(),
                Provenance);
        }
    }
}
