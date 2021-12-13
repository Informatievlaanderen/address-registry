namespace AddressRegistry.Api.Oslo.Address.Responses
{
    using System.Runtime.Serialization;
    using System.Xml.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.SpatialTools;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Newtonsoft.Json;

    /// <summary>
    /// De punt geometrie van het object.
    /// </summary>
    [DataContract(Name = "Adrespositie", Namespace = "")]
    public class AddressPosition
    {
        /// <summary>
        /// Een geometrie punt.
        /// </summary>
        [JsonProperty("geometrie")]
        [XmlIgnore]
        public GmlJsonPoint Geometry { get; set; }

        /// <summary>
        /// De gebruikte methode om de positie te bepalen.
        /// </summary>
        [DataMember(Name = "PositieGeometrieMethode", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public PositieGeometrieMethode? PositieGeometrieMethode { get; set; }

        /// <summary>
        /// De specificatie van het object, voorgesteld door de positie.
        /// </summary>
        [DataMember(Name = "PositieSpecificatie", Order = 2)]
        [JsonProperty(Required = Required.DisallowNull)]
        public PositieSpecificatie PositieSpecificatie { get; set; }

        public AddressPosition(GmlJsonPoint geometry,
            PositieGeometrieMethode? positieGeometrieMethode,
            PositieSpecificatie positieSpecificatie)
        {
            Geometry = geometry;
            PositieGeometrieMethode = positieGeometrieMethode;
            PositieSpecificatie = positieSpecificatie;
        }
    }
}
