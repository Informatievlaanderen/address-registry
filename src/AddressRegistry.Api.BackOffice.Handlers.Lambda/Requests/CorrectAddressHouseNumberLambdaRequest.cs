namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Requests;
    using Abstractions.SqsRequests;
    using StreetName;
    using StreetName.Commands;
    using IHasAddressPersistentLocalId = Abstractions.IHasAddressPersistentLocalId;

    public sealed record CorrectAddressHouseNumberLambdaRequest :
        AddressLambdaRequest,
        IHasBackOfficeRequest<CorrectAddressHouseNumberRequest>,
        IHasAddressPersistentLocalId
    {
        public CorrectAddressHouseNumberLambdaRequest(string groupId, CorrectAddressHouseNumberSqsRequest sqsRequest)
            : base(
                groupId,
                sqsRequest.TicketId,
                sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
            AddressPersistentLocalId = sqsRequest.PersistentLocalId;
        }

        public CorrectAddressHouseNumberRequest Request { get; init; }

        public int AddressPersistentLocalId { get; }

        /// <summary>
        /// Map to CorrectAddressHouseNumber command
        /// </summary>
        /// <returns>CorrectAddressHouseNumber.</returns>
        public CorrectAddressHouseNumber ToCommand()
        {
            return new CorrectAddressHouseNumber(
                this.StreetNamePersistentLocalId(),
                new AddressPersistentLocalId(AddressPersistentLocalId),
                HouseNumber.Create(Request.Huisnummer),
                Provenance);
        }
    }
}
