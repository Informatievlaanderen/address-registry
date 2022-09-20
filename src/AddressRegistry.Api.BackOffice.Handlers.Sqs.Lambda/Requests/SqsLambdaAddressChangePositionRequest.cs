namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Lambda.Requests
{
    using Abstractions.Converters;
    using Abstractions.Requests;
    using StreetName;
    using StreetName.Commands;
    using AddressRegistry.Api.BackOffice.Abstractions;

    public sealed class SqsLambdaAddressChangePositionRequest :
        SqsLambdaRequest,
        IHasBackOfficeRequest<AddressBackOfficeChangePositionRequest>,
        Abstractions.IHasAddressPersistentLocalId
    {
        public AddressBackOfficeChangePositionRequest Request { get; set; }

        public int AddressPersistentLocalId { get; set; }

        /// <summary>
        /// Map to ChangeAddressPosition command
        /// </summary>
        /// <returns>ChangeAddressPosition.</returns>
        public ChangeAddressPosition ToCommand()
        {
            return new ChangeAddressPosition(
                StreetNamePersistentLocalId,
                new AddressPersistentLocalId(AddressPersistentLocalId),
                Request.PositieGeometrieMethode.Map(),
                Request.PositieSpecificatie.Map(),
                string.IsNullOrWhiteSpace(Request.Positie) ? null : Request.Positie.ToExtendedWkbGeometry(),
                Provenance);
        }
    }
}
