namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Requests;
    using Abstractions.SqsRequests;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo.Extensions;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using StreetName;
    using StreetName.Commands;

    public sealed record ChangeAddressPostalCodeLambdaRequest :
        SqsLambdaRequest,
        IHasBackOfficeRequest<ChangeAddressPostalCodeRequest>,
        Abstractions.IHasAddressPersistentLocalId
    {
        public ChangeAddressPostalCodeLambdaRequest(string groupId, ChangeAddressPostalCodeSqsRequest sqsRequest)
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

        public ChangeAddressPostalCodeRequest Request { get; init; }

        public int AddressPersistentLocalId { get; }

        /// <summary>
        /// Map to ChangeAddressPostalCode command
        /// </summary>
        /// <returns>ChangeAddressPostalCode.</returns>
        public ChangeAddressPostalCode ToCommand()
        {
            var postInfoIdentifier = Request.PostInfoId
                .AsIdentifier()
                .Map(x => x);
            var postalCode = new PostalCode(postInfoIdentifier.Value);

            return new ChangeAddressPostalCode(
                this.StreetNamePersistentLocalId(),
                new AddressPersistentLocalId(AddressPersistentLocalId),
                postalCode,
                Provenance);
        }
    }
}
