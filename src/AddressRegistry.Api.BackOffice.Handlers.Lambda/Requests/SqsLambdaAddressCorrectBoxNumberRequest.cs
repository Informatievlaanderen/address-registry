namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Requests;
    using StreetName;
    using StreetName.Commands;

    public sealed class SqsLambdaAddressCorrectBoxNumberRequest :
        SqsLambdaRequest,
        IHasBackOfficeRequest<AddressBackOfficeCorrectBoxNumberRequest>,
        Abstractions.IHasAddressPersistentLocalId
    {
        public AddressBackOfficeCorrectBoxNumberRequest Request { get; set; }

        public int AddressPersistentLocalId { get; set; }

        /// <summary>
        /// Map to CorrectAddressBoxNumber command
        /// </summary>
        /// <returns>CorrectAddressBoxNumber.</returns>
        public CorrectAddressBoxNumber ToCommand()
        {
            return new CorrectAddressBoxNumber(
                StreetNamePersistentLocalId,
                new AddressPersistentLocalId(AddressPersistentLocalId),
                BoxNumber.Create(Request.Busnummer),
                Provenance);
        }
    }
}
