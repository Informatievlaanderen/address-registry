namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Requests;
    using Abstractions.SqsRequests;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo.Extensions;
    using StreetName;
    using StreetName.Commands;

    public sealed record CorrectAddressBoxNumbersLambdaRequest :
        AddressLambdaRequest,
        IHasBackOfficeRequest<CorrectAddressBoxNumbersRequest>
    {
        public CorrectAddressBoxNumbersLambdaRequest(string groupId, CorrectAddressBoxNumbersSqsRequest sqsRequest)
            : base(
                groupId,
                sqsRequest.TicketId,
                sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
        }

        public CorrectAddressBoxNumbersRequest Request { get; init; }

        /// <summary>
        /// Map to CorrectAddressBoxNumbers command
        /// </summary>
        /// <returns>CorrectAddressBoxNumbers.</returns>
        public CorrectAddressBoxNumbers ToCommand()
        {
            return new CorrectAddressBoxNumbers(
                this.StreetNamePersistentLocalId(),
                Request.Busnummers.ToDictionary(x =>
                    new AddressPersistentLocalId(x.AdresId.AsIdentifier().Map(int.Parse)),
                    x => BoxNumber.Create(x.Busnummer)),
                Provenance);
        }
    }
}
