namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Requests;
    using Abstractions.SqsRequests;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using StreetName;
    using StreetName.Commands;

    public sealed record RetireAddressLambdaRequest :
        SqsLambdaRequest,
        IHasBackOfficeRequest<RetireAddressRequest>,
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

        public RetireAddressRequest Request { get; init; }

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
