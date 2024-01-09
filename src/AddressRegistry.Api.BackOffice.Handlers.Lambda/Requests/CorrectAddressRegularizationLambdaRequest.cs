namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Requests;
    using Abstractions.SqsRequests;
    using StreetName;
    using StreetName.Commands;
    using IHasAddressPersistentLocalId = Abstractions.IHasAddressPersistentLocalId;

    public sealed record CorrectAddressRegularizationLambdaRequest :
        AddressLambdaRequest,
        IHasBackOfficeRequest<CorrectAddressRegularizationRequest>,
        IHasAddressPersistentLocalId
    {
        public CorrectAddressRegularizationLambdaRequest(string groupId, CorrectAddressRegularizationSqsRequest regularizationSqsRequest)
            : base(
                groupId,
                regularizationSqsRequest.TicketId,
                regularizationSqsRequest.IfMatchHeaderValue,
                regularizationSqsRequest.ProvenanceData.ToProvenance(),
                regularizationSqsRequest.Metadata)
        {
            Request = regularizationSqsRequest.Request;
        }

        public CorrectAddressRegularizationRequest Request { get; init; }

        public int AddressPersistentLocalId => Request.PersistentLocalId;

        /// <summary>
        /// Map to command
        /// </summary>
        /// <returns>CorrectAddressRegularization.</returns>
        public CorrectAddressRegularization ToCommand()
        {
            return new CorrectAddressRegularization(
                this.StreetNamePersistentLocalId(),
                new AddressPersistentLocalId(AddressPersistentLocalId),
                Provenance);
        }
    }
}
