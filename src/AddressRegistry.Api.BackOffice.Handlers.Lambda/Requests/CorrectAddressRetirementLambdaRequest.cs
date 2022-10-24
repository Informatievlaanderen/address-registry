namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using Sqs.Requests;
    using StreetName;
    using StreetName.Commands;

    public record CorrectAddressRetirementLambdaRequest :
        SqsLambdaRequest,
        IHasBackOfficeRequest<CorrectAddressRetirementBackOfficeRequest>,
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

        public CorrectAddressRetirementBackOfficeRequest Request { get; init; }

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
