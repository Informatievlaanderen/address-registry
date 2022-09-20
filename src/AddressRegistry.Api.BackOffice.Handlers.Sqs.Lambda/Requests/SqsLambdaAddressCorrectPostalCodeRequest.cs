namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Lambda.Requests
{
    using Abstractions.Requests;
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
            return new CorrectAddressPostalCode(
                StreetNamePersistentLocalId,
                new AddressPersistentLocalId(AddressPersistentLocalId),
                new PostalCode(Request.PostInfoId),
                municipalityId,
                Provenance);
        }
    }
}
