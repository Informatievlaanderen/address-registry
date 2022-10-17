namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using StreetName;
    using StreetName.Commands;

    public sealed class SqsLambdaAddressRemoveRequest :
        SqsLambdaRequest,
        IHasBackOfficeRequest<AddressBackOfficeRemoveRequest>,
        Abstractions.IHasAddressPersistentLocalId
    {
        public AddressBackOfficeRemoveRequest Request { get; set; }

        public int AddressPersistentLocalId => Request.PersistentLocalId;

        /// <summary>
        /// Map to RemoveAddress command
        /// </summary>
        /// <returns>RemoveAddress.</returns>
        public RemoveAddress ToCommand()
        {
            return new RemoveAddress(
                this.StreetNamePersistentLocalId(),
                new AddressPersistentLocalId(AddressPersistentLocalId),
                Provenance);
        }
    }
}
