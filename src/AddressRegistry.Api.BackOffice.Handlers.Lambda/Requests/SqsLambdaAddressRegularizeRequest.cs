namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Requests;
    using StreetName;
    using StreetName.Commands;

    public sealed class SqsLambdaAddressRegularizeRequest :
        SqsLambdaRequest,
        IHasBackOfficeRequest<AddressBackOfficeRegularizeRequest>,
        Abstractions.IHasAddressPersistentLocalId
    {
        public AddressBackOfficeRegularizeRequest Request { get; set; }

        public int AddressPersistentLocalId => Request.PersistentLocalId;

        /// <summary>
        /// Map to RegularizeAddress command
        /// </summary>
        /// <returns>RegularizeAddress.</returns>
        public RegularizeAddress ToCommand()
        {
            return new RegularizeAddress(
                StreetNamePersistentLocalId,
                new AddressPersistentLocalId(AddressPersistentLocalId),
                Provenance);
        }
    }
}
