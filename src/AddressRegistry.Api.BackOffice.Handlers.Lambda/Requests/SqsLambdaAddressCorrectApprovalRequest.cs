namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using StreetName;
    using StreetName.Commands;

    public sealed class SqsLambdaAddressCorrectApprovalRequest :
        SqsLambdaRequest,
        IHasBackOfficeRequest<AddressBackOfficeCorrectApprovalRequest>,
        Abstractions.IHasAddressPersistentLocalId
    {
        public AddressBackOfficeCorrectApprovalRequest Request { get; set; }

        public int AddressPersistentLocalId => Request.PersistentLocalId;

        /// <summary>
        /// Map to CorrectAddressApproval command.
        /// </summary>
        /// <returns>CorrectAddressApproval.</returns>
        public CorrectAddressApproval ToCommand()
        {
            return new CorrectAddressApproval(this.StreetNamePersistentLocalId(), new AddressPersistentLocalId(AddressPersistentLocalId), Provenance);
        }
    }
}
