namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using Sqs.Requests;
    using StreetName;
    using StreetName.Commands;

    public sealed record CorrectAddressDeregularizationLambdaRequest :
        SqsLambdaRequest,
        IHasBackOfficeRequest<CorrectAddressDeregularizationBackOfficeRequest>,
        Abstractions.IHasAddressPersistentLocalId
    {
        public CorrectAddressDeregularizationLambdaRequest(string groupId, CorrectAddressDeregularizationSqsRequest regularizationSqsRequest)
            : base(
                groupId,
                regularizationSqsRequest.TicketId,
                regularizationSqsRequest.IfMatchHeaderValue,
                regularizationSqsRequest.ProvenanceData.ToProvenance(),
                regularizationSqsRequest.Metadata)
        {
            Request = regularizationSqsRequest.Request;
        }

        public CorrectAddressDeregularizationBackOfficeRequest Request { get; init; }

        public int AddressPersistentLocalId => Request.PersistentLocalId;

        /// <summary>
        /// Map to command
        /// </summary>
        /// <returns>CorrectAddressDeregularization.</returns>
        public CorrectAddressDeregularization ToCommand()
        {
            return new CorrectAddressDeregularization(
                this.StreetNamePersistentLocalId(),
                new AddressPersistentLocalId(AddressPersistentLocalId),
                Provenance);
        }
    }
}
