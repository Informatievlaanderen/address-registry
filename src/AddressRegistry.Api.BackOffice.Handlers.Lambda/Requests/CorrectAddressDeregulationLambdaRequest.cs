namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Requests;
    using Abstractions.SqsRequests;
    using StreetName;
    using StreetName.Commands;
    using IHasAddressPersistentLocalId = Abstractions.IHasAddressPersistentLocalId;

    public sealed record CorrectAddressDeregulationLambdaRequest :
        AddressLambdaRequest,
        IHasBackOfficeRequest<CorrectAddressDeregulationRequest>,
        IHasAddressPersistentLocalId
    {
        public CorrectAddressDeregulationLambdaRequest(string groupId, CorrectAddressDeregulationSqsRequest regularizationSqsRequest)
            : base(
                groupId,
                regularizationSqsRequest.TicketId,
                regularizationSqsRequest.IfMatchHeaderValue,
                regularizationSqsRequest.ProvenanceData.ToProvenance(),
                regularizationSqsRequest.Metadata)
        {
            Request = regularizationSqsRequest.Request;
        }

        public CorrectAddressDeregulationRequest Request { get; init; }

        public int AddressPersistentLocalId => Request.PersistentLocalId;

        /// <summary>
        /// Map to command
        /// </summary>
        /// <returns>CorrectAddressDeregulation.</returns>
        public CorrectAddressDeregulation ToCommand()
        {
            return new CorrectAddressDeregulation(
                this.StreetNamePersistentLocalId(),
                new AddressPersistentLocalId(AddressPersistentLocalId),
                Provenance);
        }
    }
}
