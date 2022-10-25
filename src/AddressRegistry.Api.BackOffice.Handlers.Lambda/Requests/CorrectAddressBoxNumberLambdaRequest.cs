namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using Sqs.Requests;
    using StreetName;
    using StreetName.Commands;

    public sealed record CorrectAddressBoxNumberLambdaRequest :
        SqsLambdaRequest,
        IHasBackOfficeRequest<CorrectAddressBoxNumberBackOfficeRequest>,
        Abstractions.IHasAddressPersistentLocalId
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

        public CorrectAddressBoxNumberBackOfficeRequest Request { get; init; }

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
