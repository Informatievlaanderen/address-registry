namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using Sqs.Requests;
    using StreetName;
    using StreetName.Commands;

    public record CorrectBoxNumberLambdaRequest :
        SqsLambdaRequest,
        IHasBackOfficeRequest<BackOfficeCorrectBoxNumberRequest>,
        Abstractions.IHasAddressPersistentLocalId
    {
        public CorrectBoxNumberLambdaRequest(string groupId, CorrectBoxNumberSqsRequest sqsRequest)
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

        public BackOfficeCorrectBoxNumberRequest Request { get; set; }

        public int AddressPersistentLocalId { get; set; }

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
