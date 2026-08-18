namespace AddressRegistry.Api.Oslo.Address.V3
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.Gml;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// De geometrie van het object in gml-formaat.
    /// </summary>
    public class AddressPositionV3
    {
        [JsonProperty("@type", Order = 0, Required = Required.DisallowNull)]
        public string Type => "GeografischePositie";

        /// <summary>
        /// De geometrie.
        /// </summary>
        [JsonProperty("geometrie", Order = 1, Required = Required.DisallowNull)]
        public List<PointGeometrie> Geometry { get; set; }

        /// <summary>
        /// De gebruikte methode om de positie te bepalen.
        /// </summary>
        [JsonProperty(PropertyName = "methode", Order = 1, Required = Required.DisallowNull)]
        public AdresPositieGeometrieMethodeV3? Methode { get; set; }

        /// <summary>
        /// De specificatie van het object, voorgesteld door de positie.
        /// </summary>
        [JsonProperty(PropertyName = "specificatie", Order = 2, Required = Required.DisallowNull)]
        public AdresPositieSpecificatieV3 PositieSpecificatie { get; set; }

        public AddressPositionV3(IEnumerable<PointGeometrie> geometries,
            PositieGeometrieMethode positieGeometrieMethode,
            PositieSpecificatie positieSpecificatie)
        {
            Geometry = new List<PointGeometrie>(geometries);
            Methode = new AdresPositieGeometrieMethodeV3(positieGeometrieMethode);
            PositieSpecificatie = new AdresPositieSpecificatieV3(positieSpecificatie);
        }
    }

    /// <summary>
    /// De gebruikte methode om de positie te bepalen.
    /// </summary>
    public class AdresPositieGeometrieMethodeV3
    {
        private static readonly CamelCaseNamingStrategy NamingStrategy = new();

        /// <summary>
        /// Identificatie van de methode.
        /// </summary>
        [JsonProperty("@id", Required = Required.DisallowNull, Order = 1)]
        public string Id { get; set; }

        /// <summary>
        /// Linked data type van het object.
        /// </summary>
        [JsonProperty("@type", Required = Required.DisallowNull, Order = 2)]
        public string Type => "Concept";

        /// <summary>
        /// De beschrijving van de methode.
        /// </summary>
        [JsonProperty("code", Required = Required.DisallowNull, Order = 3)]
        public PositieGeometrieMethode Label { get; set; }

        public AdresPositieGeometrieMethodeV3(PositieGeometrieMethode positieGeometrieMethode)
        {
            Label = positieGeometrieMethode;
            Id = OsloNamespaces.AdresGeometrieMethode.ToPuri(NamingStrategy.GetPropertyName(positieGeometrieMethode.ToString(), false));
        }
    }

    /// <summary>
    /// De specificatie van het object, voorgesteld door de positie.
    /// </summary>
    public class AdresPositieSpecificatieV3
    {
        private static readonly CamelCaseNamingStrategy NamingStrategy = new();

        /// <summary>
        /// Identificatie van de specificatie.
        /// </summary>
        [JsonProperty("@id", Required = Required.DisallowNull, Order = 1)]
        public string Id { get; set; }

        /// <summary>
        /// Linked data type van het object.
        /// </summary>
        [JsonProperty("@type", Required = Required.DisallowNull, Order = 2)]
        public string Type => "Concept";

        /// <summary>
        /// De beschrijving van de specificatie.
        /// </summary>
        [JsonProperty("code", Required = Required.DisallowNull, Order = 3)]
        public PositieSpecificatie Label { get; set; }

        public AdresPositieSpecificatieV3(PositieSpecificatie positieSpecificatie)
        {
            Label = positieSpecificatie;
            Id = OsloNamespaces.AdresGeometrieSpecificatie.ToPuri(NamingStrategy.GetPropertyName(positieSpecificatie.ToString(), false));
        }
    }
}
