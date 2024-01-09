namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions;
    using Abstractions.Converters;
    using Abstractions.Requests;
    using Abstractions.SqsRequests;
    using StreetName;
    using StreetName.Commands;
    using IHasAddressPersistentLocalId = Abstractions.IHasAddressPersistentLocalId;

    public sealed record CorrectAddressPositionLambdaRequest :
        AddressLambdaRequest,
        IHasBackOfficeRequest<CorrectAddressPositionRequest>,
        IHasAddressPersistentLocalId
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
