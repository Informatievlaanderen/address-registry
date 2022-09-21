namespace AddressRegistry.Api.BackOffice.Abstractions.Requests
{
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using Newtonsoft.Json;
    using System.Runtime.Serialization;

    [DataContract(Name = "CorrigerenAdresPositie", Namespace = "")]
    public sealed class AddressBackOfficeCorrectPositionRequest
    {
        /// <summary>
        /// De geometriemethode van het adres.
        /// </summary>
        [DataMember(Name = "PositieGeometrieMethode", Order = 1)]
        [JsonProperty(Required = Required.Always)]
        public PositieGeometrieMethode PositieGeometrieMethode { get; set; }

        /// <summary>
        /// De specificatie van de adrespositie (optioneel).
        /// </summary>
        [DataMember(Name = "PositieSpecificatie", Order = 2)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public PositieSpecificatie? PositieSpecificatie { get; set; }

        /// <summary>
        /// Puntgeometrie van het adres in GML-3 formaat met Lambert 72 referentie systeem.
        /// </summary>
        [DataMember(Name = "Positie", Order = 3)]
        [JsonProperty(Required = Required.Default)]
        public string? Positie { get; set; }
    }
}
