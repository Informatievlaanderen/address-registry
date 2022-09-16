namespace AddressRegistry.Api.BackOffice.Abstractions.Requests
{
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using StreetName;
    using StreetName.Commands;

    [DataContract(Name = "CorrigerenHuisnummerAdres", Namespace = "")]
    public class SqsAddressCorrectHouseNumberRequest : SqsRequest
    {
        /// <summary>
        /// De unieke en persistente identificator van het adres.
        /// </summary>
        [JsonIgnore]
        public int PersistentLocalId { get; set; }

        /// <summary>
        /// Het huisnummer van het adres.
        /// </summary>
        [DataMember(Name = "Huisnummer", Order = 0)]
        [JsonProperty(Required = Required.Always)]
        public string Huisnummer { get; set; }

        /// <summary>
        /// Map to CorrectAddressHouseNumber command
        /// </summary>
        /// <returns>CorrectAddressHouseNumber.</returns>
        public CorrectAddressHouseNumber ToCommand(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            Provenance provenance)
        {
            return new CorrectAddressHouseNumber(
                streetNamePersistentLocalId,
                new AddressPersistentLocalId(PersistentLocalId),
                HouseNumber.Create(Huisnummer),
                provenance);
        }
    }
}
