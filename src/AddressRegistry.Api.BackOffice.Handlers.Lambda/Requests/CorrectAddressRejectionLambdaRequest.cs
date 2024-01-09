namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Requests;
    using Abstractions.SqsRequests;
    using StreetName;
    using StreetName.Commands;
    using IHasAddressPersistentLocalId = Abstractions.IHasAddressPersistentLocalId;

    public sealed record CorrectAddressRejectionLambdaRequest :
        AddressLambdaRequest,
        IHasBackOfficeRequest<CorrectAddressRejectionRequest>,
        IHasAddressPersistentLocalId
    {
        public CorrectAddressRejectionLambdaRequest(string groupId, CorrectAddressRejectionSqsRequest sqsRequest)
            : base(
                groupId,
                sqsRequest.TicketId,
                sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
        }

        public CorrectAddressRejectionRequest Request { get; init; }

        public int AddressPersistentLocalId => Request.PersistentLocalId;

        /// <summary>
        /// Map to CorrectAddressRejection command
        /// </summary>
        /// <returns>CorrectAddressRejection.</returns>
        public CorrectAddressRejection ToCommand()
        {
            return new CorrectAddressRejection(
                this.StreetNamePersistentLocalId(),
                new AddressPersistentLocalId(AddressPersistentLocalId),
                Provenance);
        }
    }
}
