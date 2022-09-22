namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Lambda.Requests
{
    using Abstractions.Requests;
    using StreetName;
    using StreetName.Commands;

    public sealed class SqsLambdaAddressApproveRequest :
        SqsLambdaRequest,
        IHasBackOfficeRequest<AddressBackOfficeApproveRequest>,
        Abstractions.IHasAddressPersistentLocalId
    {
        public AddressBackOfficeApproveRequest Request { get; set; }

        public int AddressPersistentLocalId => Request.PersistentLocalId;

        /// <summary>
        /// Map to ApproveAddress command.
        /// </summary>
        /// <returns>ApproveAddress.</returns>
        public ApproveAddress ToCommand()
        {
            return new ApproveAddress(StreetNamePersistentLocalId, new AddressPersistentLocalId(AddressPersistentLocalId), Provenance);
        }
    }
}
