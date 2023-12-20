namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Requests;
    using Abstractions.SqsRequests;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using StreetName.Commands;

    public sealed record ReaddressLambdaRequest :
        AddressLambdaRequest,
        IHasBackOfficeRequest<ReaddressRequest>
    {
        public ReaddressLambdaRequest(string groupId, ReaddressSqsRequest sqsRequest)
            : base(
                groupId,
                sqsRequest.TicketId,
                sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
        }

        public ReaddressRequest Request { get; init; }

        /// <summary>
        /// Map to Readdress command
        /// </summary>
        /// <returns>Readdress.</returns>
        public Readdress ToCommand(List<ReaddressAddressItem> readdressAddressItems, List<RetireAddressItem> retireAddressItems)
        {
            return new Readdress(
                this.StreetNamePersistentLocalId(),
                readdressAddressItems,
                retireAddressItems,
                CommandProvenance);
        }
    }
}
