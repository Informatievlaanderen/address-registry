namespace AddressRegistry.Api.BackOffice.Abstractions.Requests
{
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Converters;
    using Newtonsoft.Json;
    using StreetName;
    using StreetName.Commands;

    [DataContract(Name = "WijzigenPostcodeAdres", Namespace = "")]
    public class SqsAddressChangePostalCodeRequest : SqsRequest
    {
        /// <summary>
        /// De unieke en persistente identificator van het adres.
        /// </summary>
        [JsonIgnore]
        public int PersistentLocalId { get; set; }

        /// <summary>
        /// De unieke en persistente identificator van de postcode van het adres.
        /// </summary>
        [DataMember(Name = "PostinfoId", Order = 0)]
        [JsonProperty(Required = Required.Always)]
        public string PostInfoId { get; set; }

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
}
