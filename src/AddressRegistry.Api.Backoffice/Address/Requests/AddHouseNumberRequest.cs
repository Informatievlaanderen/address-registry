namespace AddressRegistry.Api.Backoffice.Address.Requests
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.SpatialTools;
    using Newtonsoft.Json;

    public class AddHouseNumberRequest
    {
        [JsonProperty("heeftStraatnaam")]
        public string StreetNameId { get; set; }

        [JsonProperty("huisnummer")]
        public string HouseNumber { get; set; }

        [JsonProperty("busnummer")]
        public string BoxNumber { get; set; }

        [JsonProperty("heeftPostinfo")]
        public string PostalCode { get; set; }

        [JsonProperty("status")]
        public AdresStatus Status { get; set; } // TODO: Map from codelijsten string -> enum

        [JsonProperty("officieelToegekend")]
        public bool OfficiallyAssigned { get; set; }

        [JsonProperty("positie")]
        public AddressPositionRequest Position { get; set; }
    }

    public class AddressPositionRequest
    {
        [JsonProperty("methode")]
        public PositieGeometrieMethode Method { get; set; }

        [JsonProperty("specificatie")]
        public PositieSpecificatie Specification { get; set; }

        [JsonProperty("geometrie")]
        public GeoJSONPoint Point { get; set; }
    }
}
