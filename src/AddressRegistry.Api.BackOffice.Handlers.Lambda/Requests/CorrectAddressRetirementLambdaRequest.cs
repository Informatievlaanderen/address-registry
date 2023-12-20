namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Requests;
    using Abstractions.SqsRequests;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using StreetName;
    using StreetName.Commands;

    public sealed record CorrectAddressRetirementLambdaRequest :
        AddressLambdaRequest,
        IHasBackOfficeRequest<CorrectAddressRetirementRequest>,
        Abstractions.IHasAddressPersistentLocalId
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
            return new CorrectAddressRetirement(this.StreetNamePersistentLocalId(), new AddressPersistentLocalId(AddressPersistentLocalId), CommandProvenance);
        }
    }
}
