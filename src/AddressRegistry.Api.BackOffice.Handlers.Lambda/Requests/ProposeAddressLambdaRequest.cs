namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions;
    using Abstractions.Converters;
    using Abstractions.Requests;
    using Abstractions.SqsRequests;
    using StreetName;
    using StreetName.Commands;

    public sealed record ProposeAddressLambdaRequest :
        AddressLambdaRequest,
        IHasBackOfficeRequest<ProposeAddressRequest>
    {
        public ProposeAddressLambdaRequest(string groupId, ProposeAddressSqsRequest sqsRequest)
            : base(
                groupId,
                sqsRequest.TicketId,
                null,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
            AddressPersistentLocalId = new AddressPersistentLocalId(sqsRequest.PersistentLocalId);
        }

        public ProposeAddressRequest Request { get; init; }

        public AddressPersistentLocalId AddressPersistentLocalId { get; }

        /// <summary>
        /// Map to ProposeAddress command
        /// </summary>
        /// <returns>ProposeAddress.</returns>
        public ProposeAddress ToCommand(
            PostalCode postalCode,
            MunicipalityId postalCodeMunicipalityId)
        {
            return new ProposeAddress(
                this.StreetNamePersistentLocalId(),
                postalCode,
                postalCodeMunicipalityId,
                AddressPersistentLocalId,
                HouseNumber.Create(Request.Huisnummer),
                string.IsNullOrWhiteSpace(Request.Busnummer) ? null : BoxNumber.Create(Request.Busnummer),
                Request.PositieGeometrieMethode.Map(),
                Request.PositieSpecificatie.Map(),
                Request.Positie.ToExtendedWkbGeometry(),
                Provenance);
        }
    }
}
