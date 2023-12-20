namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Requests;
    using Abstractions.SqsRequests;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using StreetName;
    using StreetName.Commands;

    public sealed record DeregulateAddressLambdaRequest :
        AddressLambdaRequest,
        IHasBackOfficeRequest<DeregulateAddressRequest>,
        Abstractions.IHasAddressPersistentLocalId
    {
        public DeregulateAddressLambdaRequest(string groupId, DeregulateAddressSqsRequest sqsRequest)
            : base(
                groupId,
                sqsRequest.TicketId,
                sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
        }

        public DeregulateAddressRequest Request { get; init; }

        public int AddressPersistentLocalId => Request.PersistentLocalId;

        /// <summary>
        /// Map to DeregulateAddress command
        /// </summary>
        /// <returns>DeregulateAddress.</returns>
        public DeregulateAddress ToCommand()
        {
            return new DeregulateAddress(
                this.StreetNamePersistentLocalId(),
                new AddressPersistentLocalId(AddressPersistentLocalId),
                CommandProvenance);
        }
    }
}
