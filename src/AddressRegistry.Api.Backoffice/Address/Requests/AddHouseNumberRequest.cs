namespace AddressRegistry.Api.Backoffice.Address.Requests
{
    using System;
    using GeoJSON.Net.Geometry;
    using Newtonsoft.Json;

    public class AddHouseNumberRequest
    {
        [JsonProperty("heeftStraatnaam")]
        public Uri StreetNameId { get; set; }

        [JsonProperty("huisnummer")]
        public string HouseNumber { get; set; }

        [JsonProperty("busnummer")]
        public string BoxNumber { get; set; }

        [JsonProperty("heeftPostinfo")]
        public Uri PostalCode { get; set; }

        [JsonProperty("status")]
        public Uri Status { get; set; }

        [JsonProperty("officieelToegekend")]
        public bool OfficiallyAssigned { get; set; }

        [JsonProperty("positie")]
        public AddressPositionRequest Position { get; set; }
    }

    public class AddressPositionRequest
    {
        [JsonProperty("methode")]
        public Uri Method { get; set; }

        [JsonProperty("specificatie")]
        public Uri Specification { get; set; }

        [JsonProperty("geometrie")]
        public Point Point { get; set; }
    }
}
