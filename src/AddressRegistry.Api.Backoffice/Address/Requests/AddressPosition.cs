namespace AddressRegistry.Api.Backoffice.Address.Requests
{
    using System;
    using GeoJSON.Net.Geometry;
    using Newtonsoft.Json;

    public class AddressPosition
    {
        [JsonProperty("methode")]
        public Uri Method { get; set; }

        [JsonProperty("specificatie")]
        public Uri Specification { get; set; }

        [JsonProperty("geometrie")]
        public Point Point { get; set; }
    }
}
