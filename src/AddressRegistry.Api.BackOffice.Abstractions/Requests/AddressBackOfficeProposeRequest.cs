namespace AddressRegistry.Api.BackOffice.Abstractions.Requests
{
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using Newtonsoft.Json;

    [DataContract(Name = "VoorstelAdres", Namespace = "")]
    public class AddressBackOfficeProposeRequest
    {
        /// <summary>
        /// De unieke en persistente identificator van de postcode van het adres.
        /// </summary>
        [DataMember(Name = "PostinfoId", Order = 0)]
        [JsonProperty(Required = Required.Always)]
        public string PostInfoId { get; set; }

        /// <summary>
        /// De unieke en persistente identificator van de straatnaam van het adres.
        /// </summary>
        [DataMember(Name = "StraatnaamId", Order = 1)]
        [JsonProperty(Required = Required.Always)]
        public string StraatNaamId { get; set; }

        /// <summary>
        /// Het huisnummer van het adres.
        /// </summary>
        [DataMember(Name = "Huisnummer", Order = 2)]
        [JsonProperty(Required = Required.Always)]
        public string Huisnummer { get; set; }

        /// <summary>
        /// Het busnummer van het adres (optioneel).
        /// </summary>
        [DataMember(Name = "Busnummer", Order = 3)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? Busnummer { get; set; }

        /// <summary>
        /// De geometriemethode van het adres.
        /// </summary>
        [DataMember(Name= "PositieGeometriemethode", Order = 4)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public PositieGeometrieMethode? PositieGeometrieMethode { get; set; }

        /// <summary>
        /// De specificatie van de adrespositie (optioneel).
        /// </summary>
        [DataMember(Name = "PositieSpecificatie", Order = 5)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public PositieSpecificatie? PositieSpecificatie { get; set; }

        /// <summary>
        /// Puntgeometrie van het adres in GML-3 formaat met Lambert 72 referentie systeem.
        /// </summary>
        [DataMember(Name = "Positie", Order = 6)]
        [JsonProperty(Required = Required.Default)]
        public string? Positie { get; set; }
    }
}
