namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using Sqs.Requests;
    using StreetName;
    using StreetName.Commands;

    public sealed record CorrectRegularizedAddressLambdaRequest :
        SqsLambdaRequest,
        IHasBackOfficeRequest<CorrectRegularizedAddressBackOfficeRequest>,
        Abstractions.IHasAddressPersistentLocalId
    {
        public CorrectRegularizedAddressLambdaRequest(string groupId, CorrectRegularizedAddressSqsRequest sqsRequest)
            : base(
                groupId,
                sqsRequest.TicketId,
                sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
        }

        public CorrectRegularizedAddressBackOfficeRequest Request { get; init; }

        public int AddressPersistentLocalId => Request.PersistentLocalId;

        /// <summary>
        /// Map to command
        /// </summary>
        /// <returns>CorrectRegularizedAddress.</returns>
        public CorrectRegularizedAddress ToCommand()
        {
            return new CorrectRegularizedAddress(
                this.StreetNamePersistentLocalId(),
                new AddressPersistentLocalId(AddressPersistentLocalId),
                Provenance);
        }
    }
}
