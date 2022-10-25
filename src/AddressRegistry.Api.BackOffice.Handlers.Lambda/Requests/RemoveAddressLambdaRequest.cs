namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using Sqs.Requests;
    using StreetName;
    using StreetName.Commands;

    public sealed record RemoveAddressLambdaRequest :
        SqsLambdaRequest,
        IHasBackOfficeRequest<RemoveAddressBackOfficeRequest>,
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

        public RemoveAddressBackOfficeRequest Request { get; init; }

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
                Provenance);
        }
    }
}
