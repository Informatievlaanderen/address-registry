namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Lambda.Requests
{
    using Abstractions.Converters;
    using Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Abstractions;
    using StreetName;
    using StreetName.Commands;

    public sealed class SqsLambdaAddressProposeRequest :
        SqsLambdaRequest,
        IHasBackOfficeRequest<AddressBackOfficeProposeRequest>
    {
        public AddressBackOfficeProposeRequest Request { get; set; }

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
                StreetNamePersistentLocalId,
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
