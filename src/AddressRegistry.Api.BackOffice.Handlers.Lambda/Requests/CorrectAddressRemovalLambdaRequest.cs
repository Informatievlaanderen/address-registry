namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Requests;
    using Abstractions.SqsRequests;
    using StreetName;
    using StreetName.Commands;
    using IHasAddressPersistentLocalId = Abstractions.IHasAddressPersistentLocalId;

    public sealed record CorrectAddressRemovalLambdaRequest :
        AddressLambdaRequest,
        IHasBackOfficeRequest<CorrectAddressRemovalRequest>,
        IHasAddressPersistentLocalId
    {
        public CorrectAddressRemovalLambdaRequest(string groupId, CorrectAddressRemovalSqsRequest sqsRequest)
            : base(
                groupId,
                sqsRequest.TicketId,
                sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
        }

        public CorrectAddressRemovalRequest Request { get; init; }

        public int AddressPersistentLocalId => Request.PersistentLocalId;

        /// <summary>
        /// Map to CorrectAddressRemoval command.
        /// </summary>
        /// <returns>CorrectAddressRemoval.</returns>
        public CorrectAddressRemoval ToCommand()
        {
            return new CorrectAddressRemoval(this.StreetNamePersistentLocalId(), new AddressPersistentLocalId(AddressPersistentLocalId), Provenance);
        }
    }
}
