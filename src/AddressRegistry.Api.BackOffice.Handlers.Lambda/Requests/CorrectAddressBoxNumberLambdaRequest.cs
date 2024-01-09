namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Requests;
    using Abstractions.SqsRequests;
    using StreetName;
    using StreetName.Commands;
    using IHasAddressPersistentLocalId = Abstractions.IHasAddressPersistentLocalId;

    public sealed record CorrectAddressBoxNumberLambdaRequest :
        AddressLambdaRequest,
        IHasBackOfficeRequest<CorrectAddressBoxNumberRequest>,
        IHasAddressPersistentLocalId
    {
        public CorrectAddressBoxNumberLambdaRequest(string groupId, CorrectAddressBoxNumberSqsRequest sqsRequest)
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

        public CorrectAddressBoxNumberRequest Request { get; init; }

        public int AddressPersistentLocalId { get; }

        /// <summary>
        /// Map to CorrectAddressBoxNumber command
        /// </summary>
        /// <returns>CorrectAddressBoxNumber.</returns>
        public CorrectAddressBoxNumber ToCommand()
        {
            return new CorrectAddressBoxNumber(
                this.StreetNamePersistentLocalId(),
                new AddressPersistentLocalId(AddressPersistentLocalId),
                BoxNumber.Create(Request.Busnummer),
                Provenance);
        }
    }
}
