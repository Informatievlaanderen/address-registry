namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using Sqs.Requests;
    using StreetName;
    using StreetName.Commands;

    public sealed record CorrectAddressRegularizationLambdaRequest :
        SqsLambdaRequest,
        IHasBackOfficeRequest<CorrectAddressRegularizationBackOfficeRequest>,
        Abstractions.IHasAddressPersistentLocalId
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

        public CorrectAddressRegularizationBackOfficeRequest Request { get; init; }

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
