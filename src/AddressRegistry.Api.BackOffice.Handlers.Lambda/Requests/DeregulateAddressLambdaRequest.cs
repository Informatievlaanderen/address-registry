namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Requests;
    using Abstractions.SqsRequests;
    using StreetName;
    using StreetName.Commands;
    using IHasAddressPersistentLocalId = Abstractions.IHasAddressPersistentLocalId;

    public sealed record DeregulateAddressLambdaRequest :
        AddressLambdaRequest,
        IHasBackOfficeRequest<DeregulateAddressRequest>,
        IHasAddressPersistentLocalId
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
                Provenance);
        }
    }
}
