namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using Sqs.Requests;
    using StreetName;
    using StreetName.Commands;

    public sealed record RetireAddressLambdaRequest :
        SqsLambdaRequest,
        IHasBackOfficeRequest<RetireAddressBackOfficeRequest>,
        Abstractions.IHasAddressPersistentLocalId
    {
        public RetireAddressLambdaRequest(string groupId, RetireAddressSqsRequest sqsRequest)
            : base(
                groupId,
                sqsRequest.TicketId,
                sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
        }

        public RetireAddressBackOfficeRequest Request { get; init; }

        public int AddressPersistentLocalId => Request.PersistentLocalId;

        /// <summary>
        /// Map to RetireAddress command
        /// </summary>
        /// <returns>RetireAddress.</returns>
        public RetireAddress ToCommand()
        {
            return new RetireAddress(
                this.StreetNamePersistentLocalId(),
                new AddressPersistentLocalId(AddressPersistentLocalId),
                Provenance);
        }
    }
}
