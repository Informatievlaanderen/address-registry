namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Lambda.Requests
{
    using Abstractions.Requests;
    using StreetName;
    using StreetName.Commands;

    public sealed class SqsLambdaAddressRejectRequest :
        SqsLambdaRequest,
        IHasBackOfficeRequest<AddressBackOfficeRejectRequest>,
        Abstractions.IHasAddressPersistentLocalId
    {
        public AddressBackOfficeRejectRequest Request { get; set; }

        public int AddressPersistentLocalId => Request.PersistentLocalId;

        /// <summary>
        /// Map to RejectAddress command
        /// </summary>
        /// <returns>RejectAddress.</returns>
        public RejectAddress ToCommand()
        {
            return new RejectAddress(
                StreetNamePersistentLocalId,
                new AddressPersistentLocalId(AddressPersistentLocalId),
                Provenance);
        }
    }
}
