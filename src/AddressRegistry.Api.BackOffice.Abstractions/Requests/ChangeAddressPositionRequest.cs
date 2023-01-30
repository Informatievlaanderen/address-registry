namespace AddressRegistry.Api.BackOffice.Abstractions.Requests
{
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using Newtonsoft.Json;
    using System.Runtime.Serialization;

    [DataContract(Name = "WijzigenAdresPositie", Namespace = "")]
    public sealed class ChangeAddressPositionRequest
    {
        /// <summary>
        /// De geometriemethode van het adres.
        /// </summary>
        [DataMember(Name = "PositieGeometriemethode", Order = 1)]
        [JsonProperty(Required = Required.Always)]
        public PositieGeometrieMethode PositieGeometrieMethode { get; set; }

        /// <summary>
        /// De specificatie van de adrespositie (optioneel).
        /// </summary>
        [DataMember(Name = "PositieSpecificatie", Order = 2)]
        [JsonProperty(Required = Required.Always)]
        public PositieSpecificatie PositieSpecificatie { get; set; }

        /// <summary>
        /// Puntgeometrie van het adres in GML-3 formaat met Lambert 72 referentie systeem.
        /// </summary>
        [DataMember(Name = "Positie", Order = 3)]
        [JsonProperty(Required = Required.Always)]
        public string Positie { get; set; }
    }
}
