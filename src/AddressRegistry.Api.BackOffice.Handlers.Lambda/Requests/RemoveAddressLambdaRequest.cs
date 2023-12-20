namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Requests;
    using Abstractions.SqsRequests;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using StreetName;
    using StreetName.Commands;

    public sealed record RemoveAddressLambdaRequest :
        AddressLambdaRequest,
        IHasBackOfficeRequest<RemoveAddressRequest>,
        Abstractions.IHasAddressPersistentLocalId
    {
        public RemoveAddressLambdaRequest(string groupId, RemoveAddressSqsRequest sqsRequest)
            : base(
                groupId,
                sqsRequest.TicketId,
                sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
        }

        public RemoveAddressRequest Request { get; init; }

        public int AddressPersistentLocalId => Request.PersistentLocalId;

        /// <summary>
        /// Map to RemoveAddress command
        /// </summary>
        /// <returns>RemoveAddress.</returns>
        public RemoveAddress ToCommand()
        {
            return new RemoveAddress(
                this.StreetNamePersistentLocalId(),
                new AddressPersistentLocalId(AddressPersistentLocalId),
                CommandProvenance);
        }
    }
}
