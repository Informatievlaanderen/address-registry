namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Requests;
    using Abstractions.SqsRequests;
    using StreetName;
    using StreetName.Commands;
    using IHasAddressPersistentLocalId = Abstractions.IHasAddressPersistentLocalId;

    public sealed record ApproveAddressLambdaRequest :
        AddressLambdaRequest,
        IHasBackOfficeRequest<ApproveAddressRequest>,
        IHasAddressPersistentLocalId
    {
        public ApproveAddressLambdaRequest(string groupId, ApproveAddressSqsRequest sqsRequest)
            : base(
                groupId,
                sqsRequest.TicketId,
                sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
        }

        public ApproveAddressRequest Request { get; init; }

        public int AddressPersistentLocalId => Request.PersistentLocalId;

        /// <summary>
        /// Map to ApproveAddress command.
        /// </summary>
        /// <returns>ApproveAddress.</returns>
        public ApproveAddress ToCommand()
        {
            return new ApproveAddress(this.StreetNamePersistentLocalId(), new AddressPersistentLocalId(AddressPersistentLocalId), Provenance);
        }
    }
}
