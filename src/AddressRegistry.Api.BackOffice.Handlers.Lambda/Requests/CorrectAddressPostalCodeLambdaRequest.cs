namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Requests;
    using Abstractions.SqsRequests;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo.Extensions;
    using StreetName;
    using StreetName.Commands;
    using IHasAddressPersistentLocalId = Abstractions.IHasAddressPersistentLocalId;

    public sealed record CorrectAddressPostalCodeLambdaRequest :
        AddressLambdaRequest,
        IHasBackOfficeRequest<CorrectAddressPostalCodeRequest>,
        IHasAddressPersistentLocalId
    {
        public CorrectAddressPostalCodeLambdaRequest(string groupId, CorrectAddressPostalCodeSqsRequest sqsRequest)
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

        public CorrectAddressPostalCodeRequest Request { get; init; }

        public int AddressPersistentLocalId { get; }

        /// <summary>
        /// Map to CorrectAddressPostalCode command
        /// </summary>
        /// <returns>CorrectAddressPostalCode.</returns>
        public CorrectAddressPostalCode ToCommand(MunicipalityId municipalityId)
        {
            var postInfoIdentifier = Request.PostInfoId
                .AsIdentifier()
                .Map(x => x);
            var postalCode = new PostalCode(postInfoIdentifier.Value);

            return new CorrectAddressPostalCode(
                this.StreetNamePersistentLocalId(),
                new AddressPersistentLocalId(AddressPersistentLocalId),
                postalCode,
                municipalityId,
                Provenance);
        }
    }
}
