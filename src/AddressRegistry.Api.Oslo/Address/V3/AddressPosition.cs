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
    public class AddressPosition
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
        public AdresPositieGeometrieMethode? Methode { get; set; }

        /// <summary>
        /// De specificatie van het object, voorgesteld door de positie.
        /// </summary>
        [JsonProperty(PropertyName = "specificatie", Order = 2, Required = Required.DisallowNull)]
        public AdresPositieSpecificatie PositieSpecificatie { get; set; }

        public AddressPosition(IEnumerable<PointGeometrie> geometries,
            PositieGeometrieMethode positieGeometrieMethode,
            PositieSpecificatie positieSpecificatie)
        {
            Geometry = new List<PointGeometrie>(geometries);
            Methode = new AdresPositieGeometrieMethode(positieGeometrieMethode);
            PositieSpecificatie = new AdresPositieSpecificatie(positieSpecificatie);
        }
    }

    /// <summary>
    /// De gebruikte methode om de positie te bepalen.
    /// </summary>
    public class AdresPositieGeometrieMethode
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
        public string Type => "skos:Concept";

        /// <summary>
        /// De beschrijving van de methode.
        /// </summary>
        [JsonProperty("skos:prefLabel", Required = Required.DisallowNull, Order = 3)]
        public PositieGeometrieMethode Label { get; set; }

        public AdresPositieGeometrieMethode(PositieGeometrieMethode positieGeometrieMethode)
        {
            Label = positieGeometrieMethode;
            Id = OsloNamespaces.AdresGeometrieMethode.ToPuri(NamingStrategy.GetPropertyName(positieGeometrieMethode.ToString(), false));
        }
    }

    /// <summary>
    /// De specificatie van het object, voorgesteld door de positie.
    /// </summary>
    public class AdresPositieSpecificatie
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
        public string Type => "skos:Concept";

        /// <summary>
        /// De beschrijving van de specificatie.
        /// </summary>
        [JsonProperty("skos:prefLabel", Required = Required.DisallowNull, Order = 3)]
        public PositieSpecificatie Label { get; set; }

        public AdresPositieSpecificatie(PositieSpecificatie positieSpecificatie)
        {
            Label = positieSpecificatie;
            Id = OsloNamespaces.AdresGeometrieSpecificatie.ToPuri(NamingStrategy.GetPropertyName(positieSpecificatie.ToString(), false));
        }
    }
}
