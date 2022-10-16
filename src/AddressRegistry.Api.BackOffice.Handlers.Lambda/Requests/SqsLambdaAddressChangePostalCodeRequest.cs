namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo.Extensions;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using StreetName;
    using StreetName.Commands;

    public sealed class SqsLambdaAddressChangePostalCodeRequest :
        SqsLambdaRequest,
        IHasBackOfficeRequest<AddressBackOfficeChangePostalCodeRequest>,
        Abstractions.IHasAddressPersistentLocalId
    {
        public AddressBackOfficeChangePostalCodeRequest Request { get; set; }

        public int AddressPersistentLocalId { get; set; }

        /// <summary>
        /// Map to ChangeAddressPostalCode command
        /// </summary>
        /// <returns>ChangeAddressPostalCode.</returns>
        public ChangeAddressPostalCode ToCommand()
        {
            var postInfoIdentifier = Request.PostInfoId
                .AsIdentifier()
                .Map(x => x);
            var postalCode = new PostalCode(postInfoIdentifier.Value);

            return new ChangeAddressPostalCode(
                this.StreetNamePersistentLocalId(),
                new AddressPersistentLocalId(AddressPersistentLocalId),
                postalCode,
                Provenance);
        }
    }
}
