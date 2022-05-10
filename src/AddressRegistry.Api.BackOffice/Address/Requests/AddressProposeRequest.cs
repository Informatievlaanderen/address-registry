namespace AddressRegistry.Api.BackOffice.Address.Requests
{
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using StreetName;
    using StreetName.Commands;
    using Swashbuckle.AspNetCore.Filters;

    [DataContract(Name = "VoorstelAdres", Namespace = "")]
    public class AddressProposeRequest
    {
        /// <summary>
        /// De unieke en persistente identificator van de postinfo die het adres toekent.
        /// </summary>
        [DataMember(Name = "PostInfoId", Order = 0)]
        [JsonProperty(Required = Required.Always)]
        public string PostInfoId { get; set; }

        /// <summary>
        /// De unieke en persistente identificator van de straatnaam die het adres toekent.
        /// </summary>
        [DataMember(Name = "StraatNaamId", Order = 1)]
        [JsonProperty(Required = Required.Always)]
        public string StraatNaamId { get; set; }

        /// <summary>
        /// Het huisnummer.
        /// </summary>
        [DataMember(Name = "HuisNummer", Order = 2)]
        [JsonProperty(Required = Required.Always)]
        public string HouseNumber { get; set; }

        /// <summary>
        /// Het busnummer.
        /// </summary>
        [DataMember(Name = "BusNummer", Order = 3)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? BoxNumber { get; set; }

        /// <summary>
        /// Map to ProposeAddress command
        /// </summary>
        /// <returns>ProposeAddress.</returns>
        public ProposeAddress ToCommand(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            PostalCode postalCode,
            AddressPersistentLocalId addressPersistentLocalId,
            Provenance provenance)
        {
            return new ProposeAddress(
                streetNamePersistentLocalId,
                postalCode,
                addressPersistentLocalId,
                new HouseNumber(HouseNumber),
                string.IsNullOrWhiteSpace(BoxNumber) ? null : new BoxNumber(BoxNumber),
                provenance);
        }
    }

    public class AddressProposeRequestExamples : IExamplesProvider<AddressProposeRequest>
    {
        public AddressProposeRequest GetExamples()
        {
            return new AddressProposeRequest
            {
                StraatNaamId = "https://data.vlaanderen.be/id/straatnaam/45041",
                PostInfoId = "https://data.vlaanderen.be/id/postinfo/9000",
                HouseNumber = "11",
                BoxNumber = "3A"
            };
        }
    }
}
