namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions;
    using Abstractions.Converters;
    using Abstractions.Requests;
    using Abstractions.SqsRequests;
    using StreetName;
    using StreetName.Commands;
    using IHasAddressPersistentLocalId = Abstractions.IHasAddressPersistentLocalId;

    public sealed record ChangeAddressPositionLambdaRequest :
        AddressLambdaRequest,
        IHasBackOfficeRequest<ChangeAddressPositionRequest>,
        IHasAddressPersistentLocalId
    {
        public ChangeAddressPositionLambdaRequest(
            string groupId,
            ChangeAddressPositionSqsRequest sqsRequest)
        : base (
            groupId,
            sqsRequest.TicketId,
            sqsRequest.IfMatchHeaderValue,
            sqsRequest.ProvenanceData.ToProvenance(),
            sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
            AddressPersistentLocalId = sqsRequest.PersistentLocalId;
        }

        public ChangeAddressPositionRequest Request { get; init; }

        public int AddressPersistentLocalId { get; }

        /// <summary>
        /// Map to ChangeAddressPosition command
        /// </summary>
        /// <returns>ChangeAddressPosition.</returns>
        public ChangeAddressPosition ToCommand()
        {
            return new ChangeAddressPosition(
                this.StreetNamePersistentLocalId(),
                new AddressPersistentLocalId(AddressPersistentLocalId),
                Request.PositieGeometrieMethode.Map(),
                Request.PositieSpecificatie.Map(),
                Request.Positie.ToExtendedWkbGeometry(),
                Provenance);
        }
    }
}
