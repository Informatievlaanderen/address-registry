namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Requests;
    using Abstractions.SqsRequests;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using StreetName;
    using StreetName.Commands;

    public sealed record RegularizeAddressLambdaRequest :
        AddressLambdaRequest,
        IHasBackOfficeRequest<RegularizeAddressRequest>,
        Abstractions.IHasAddressPersistentLocalId
    {
        public RegularizeAddressLambdaRequest(string groupId, RegularizeAddressSqsRequest sqsRequest)
            : base(
                groupId,
                sqsRequest.TicketId,
                sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
        }

        public RegularizeAddressRequest Request { get; init; }

        public int AddressPersistentLocalId => Request.PersistentLocalId;

        /// <summary>
        /// Map to RegularizeAddress command
        /// </summary>
        /// <returns>RegularizeAddress.</returns>
        public RegularizeAddress ToCommand()
        {
            return new RegularizeAddress(
                this.StreetNamePersistentLocalId(),
                new AddressPersistentLocalId(AddressPersistentLocalId),
                CommandProvenance);
        }
    }
}
