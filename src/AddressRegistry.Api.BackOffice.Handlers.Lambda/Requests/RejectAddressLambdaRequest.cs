namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Requests;
    using Abstractions.SqsRequests;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using StreetName;
    using StreetName.Commands;

    public sealed record RejectAddressLambdaRequest :
        AddressLambdaRequest,
        IHasBackOfficeRequest<RejectAddressRequest>,
        Abstractions.IHasAddressPersistentLocalId
    {
        public RejectAddressLambdaRequest(string groupId, RejectAddressSqsRequest sqsRequest)
            : base(
                groupId,
                sqsRequest.TicketId,
                sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
        }

        public RejectAddressRequest Request { get; init; }

        public int AddressPersistentLocalId => Request.PersistentLocalId;

        /// <summary>
        /// Map to RejectAddress command
        /// </summary>
        /// <returns>RejectAddress.</returns>
        public RejectAddress ToCommand()
        {
            return new RejectAddress(
                this.StreetNamePersistentLocalId(),
                new AddressPersistentLocalId(AddressPersistentLocalId),
                CommandProvenance);
        }
    }
}
