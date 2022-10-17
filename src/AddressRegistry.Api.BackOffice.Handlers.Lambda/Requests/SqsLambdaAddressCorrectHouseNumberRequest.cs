namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using StreetName;
    using StreetName.Commands;

    public sealed class SqsLambdaAddressCorrectHouseNumberRequest :
        SqsLambdaRequest,
        IHasBackOfficeRequest<AddressBackOfficeCorrectHouseNumberRequest>,
        Abstractions.IHasAddressPersistentLocalId
    {
        public AddressBackOfficeCorrectHouseNumberRequest Request { get; set; }

        public int AddressPersistentLocalId { get; set; }

        /// <summary>
        /// Map to CorrectAddressHouseNumber command
        /// </summary>
        /// <returns>CorrectAddressHouseNumber.</returns>
        public CorrectAddressHouseNumber ToCommand()
        {
            return new CorrectAddressHouseNumber(
                this.StreetNamePersistentLocalId(),
                new AddressPersistentLocalId(AddressPersistentLocalId),
                HouseNumber.Create(Request.Huisnummer),
                Provenance);
        }
    }
}
