namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Lambda.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo.Extensions;
    using StreetName;
    using StreetName.Commands;

    public sealed class SqsLambdaAddressCorrectPostalCodeRequest :
        SqsLambdaRequest,
        IHasBackOfficeRequest<AddressBackOfficeCorrectPostalCodeRequest>,
        Abstractions.IHasAddressPersistentLocalId
    {
        public AddressBackOfficeCorrectPostalCodeRequest Request { get; set; }

        public int AddressPersistentLocalId { get; set; }

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
                StreetNamePersistentLocalId,
                new AddressPersistentLocalId(AddressPersistentLocalId),
                postalCode,
                municipalityId,
                Provenance);
        }
    }
}
