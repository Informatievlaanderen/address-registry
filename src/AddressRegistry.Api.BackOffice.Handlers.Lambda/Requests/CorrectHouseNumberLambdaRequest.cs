namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using Sqs.Requests;
    using StreetName;
    using StreetName.Commands;

    public record CorrectHouseNumberLambdaRequest :
        SqsLambdaRequest,
        IHasBackOfficeRequest<BackOfficeCorrectHouseNumberRequest>,
        Abstractions.IHasAddressPersistentLocalId
    {
        public CorrectHouseNumberLambdaRequest(string groupId, CorrectHouseNumberSqsRequest sqsRequest)
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

        public BackOfficeCorrectHouseNumberRequest Request { get; set; }

        public int AddressPersistentLocalId { get; set; }

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
