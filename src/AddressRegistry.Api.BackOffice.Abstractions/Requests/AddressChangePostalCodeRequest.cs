namespace AddressRegistry.Api.BackOffice.Abstractions.Requests
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using MediatR;
    using Newtonsoft.Json;
    using Responses;
    using StreetName;
    using StreetName.Commands;
    using Swashbuckle.AspNetCore.Filters;

    [DataContract(Name = "WijzigenPostcodeAdres", Namespace = "")]
    public class AddressChangePostalCodeRequest : AddressBackOfficeChangePostalCodeRequest, IRequest<ETagResponse>
    {
        /// <summary>
        /// De unieke en persistente identificator van het adres.
        /// </summary>
        [JsonIgnore]
        public int PersistentLocalId { get; set; }

        [JsonIgnore]
        public IDictionary<string, object> Metadata { get; set; }

        /// <summary>
        /// Map to ChangeAddressPostalCode command
        /// </summary>
        /// <returns>ChangeAddressPostalCode.</returns>
        public ChangeAddressPostalCode ToCommand(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            PostalCode postalCode,
            Provenance provenance)
        {
            return new ChangeAddressPostalCode(
                streetNamePersistentLocalId,
                new AddressPersistentLocalId(PersistentLocalId),
                postalCode,
                provenance);
        }
    }

    public class AddressChangePostalCodeRequestExamples : IExamplesProvider<AddressChangePostalCodeRequest>
    {
        public AddressChangePostalCodeRequest GetExamples()
        {
            return new AddressChangePostalCodeRequest
            {
                PostInfoId = "https://data.vlaanderen.be/id/postinfo/9000"
            };
        }
    }
}
