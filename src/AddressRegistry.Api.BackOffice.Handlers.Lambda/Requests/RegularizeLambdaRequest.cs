namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using Sqs.Requests;
    using StreetName;
    using StreetName.Commands;

    public record RegularizeLambdaRequest :
        SqsLambdaRequest,
        IHasBackOfficeRequest<RegularizeBackOfficeRequest>,
        Abstractions.IHasAddressPersistentLocalId
    {
        public RegularizeLambdaRequest(string groupId, RegularizeSqsRequest sqsRequest)
            : base(
                groupId,
                sqsRequest.TicketId,
                sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
        }

        public RegularizeBackOfficeRequest Request { get; init; }

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
                Provenance);
        }
    }
}
