namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Requests;
    using Abstractions.SqsRequests;
    using StreetName;
    using StreetName.Commands;
    using IHasAddressPersistentLocalId = Abstractions.IHasAddressPersistentLocalId;

    public sealed record CorrectAddressRetirementLambdaRequest :
        AddressLambdaRequest,
        IHasBackOfficeRequest<CorrectAddressRetirementRequest>,
        IHasAddressPersistentLocalId
    {
        public CorrectAddressRetirementLambdaRequest(string groupId, CorrectAddressRetirementSqsRequest sqsRequest)
            : base(
                groupId,
                sqsRequest.TicketId,
                sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
        }

        public CorrectAddressRetirementRequest Request { get; init; }

        public int AddressPersistentLocalId => Request.PersistentLocalId;

        /// <summary>
        /// Map to CorrectAddressRetirement command.
        /// </summary>
        /// <returns>CorrectAddressRetirement.</returns>
        public CorrectAddressRetirement ToCommand()
        {
            return new CorrectAddressRetirement(this.StreetNamePersistentLocalId(), new AddressPersistentLocalId(AddressPersistentLocalId), Provenance);
        }
    }
}
