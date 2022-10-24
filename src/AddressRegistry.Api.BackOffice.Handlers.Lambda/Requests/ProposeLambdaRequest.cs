namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions;
    using Abstractions.Converters;
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using Sqs.Requests;
    using StreetName;
    using StreetName.Commands;

    public record ProposeLambdaRequest :
        SqsLambdaRequest,
        IHasBackOfficeRequest<ProposeBackOfficeRequest>
    {
        public ProposeLambdaRequest(string groupId, ProposeSqsRequest sqsRequest)
            : base(
                groupId,
                sqsRequest.TicketId,
                null,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
        }

        public ProposeBackOfficeRequest Request { get; init; }

        /// <summary>
        /// Map to ProposeAddress command
        /// </summary>
        /// <returns>ProposeAddress.</returns>
        public ProposeAddress ToCommand(
            AddressPersistentLocalId addressPersistentLocalId,
            PostalCode postalCode,
            MunicipalityId postalCodeMunicipalityId)
        {
            return new ProposeAddress(
                this.StreetNamePersistentLocalId(),
                postalCode,
                postalCodeMunicipalityId,
                addressPersistentLocalId,
                HouseNumber.Create(Request.Huisnummer),
                string.IsNullOrWhiteSpace(Request.Busnummer) ? null : new BoxNumber(Request.Busnummer),
                Request.PositieGeometrieMethode.Map(),
                Request.PositieSpecificatie.Map(),
                string.IsNullOrWhiteSpace(Request.Positie) ? null : Request.Positie.ToExtendedWkbGeometry(),
                Provenance);
        }
    }
}
