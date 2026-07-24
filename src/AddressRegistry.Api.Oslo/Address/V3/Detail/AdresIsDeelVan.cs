namespace AddressRegistry.Api.Oslo.Address.V3.Detail
{
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo;
    using Newtonsoft.Json;

    /// <summary>
    /// Het huisnummer waaraan het busnummer is gekoppeld.
    /// </summary>
    public class AdresIsDeelVan
    {
        [JsonProperty(PropertyName = "@type", Order = 0, Required = Required.DisallowNull)]
        public string Type => "Referentie";

        /// <summary>
        /// De unieke en persistente identificator van het huisnummer adres (volgt de Vlaamse URI-standaard).
        /// </summary>
        [JsonProperty(PropertyName = "@id", Order = 1, Required = Required.DisallowNull)]
        public string Id { get; set; }

        /// <summary>
        /// De URL die de details van de meest recente versie van het huisnummer waaraan het busnummer is gekoppeld weergeeft.
        /// </summary>
        [JsonProperty(PropertyName = "detail", Order = 2, Required = Required.DisallowNull)]
        public string Detail { get; set; }

        public AdresIsDeelVan(int objectId, string detail)
        {
            Id = OsloNamespaces.Adres.ToPuri(objectId.ToString());
            Detail = detail;
        }
    }
}
