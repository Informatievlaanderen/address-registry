namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using Sqs.Requests;
    using StreetName;
    using StreetName.Commands;

    public sealed record ApproveAddressLambdaRequest :
        SqsLambdaRequest,
        IHasBackOfficeRequest<ApproveAddressBackOfficeRequest>,
        Abstractions.IHasAddressPersistentLocalId
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

        public ApproveAddressBackOfficeRequest Request { get; init; }

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
