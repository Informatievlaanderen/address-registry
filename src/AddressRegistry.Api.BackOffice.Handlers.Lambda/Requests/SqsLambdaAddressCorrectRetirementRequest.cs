namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using StreetName;
    using StreetName.Commands;

    public sealed class SqsLambdaAddressCorrectRetirementRequest :
        SqsLambdaRequest,
        IHasBackOfficeRequest<AddressBackOfficeCorrectRetirementRequest>,
        Abstractions.IHasAddressPersistentLocalId
    {
        public AddressBackOfficeCorrectRetirementRequest Request { get; set; }

        public int AddressPersistentLocalId => Request.PersistentLocalId;

        /// <summary>
        /// Map to CorrectAddressRetirement command.
        /// </summary>
        /// <returns>CorrectAddressRetirement.</returns>
        public CorrectAddressRetirement ToCommand()
        {
            return new CorrectAddressRetirement(this.StreetNamePersistentLocalId(), new AddressPersistentLocalId(AddressPersistentLocalId), Provenance);
        }
    }
}
