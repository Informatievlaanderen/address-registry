namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Lambda.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo.Extensions;
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
                StreetNamePersistentLocalId,
                new AddressPersistentLocalId(AddressPersistentLocalId),
                postalCode,
                Provenance);
        }
    }
}
