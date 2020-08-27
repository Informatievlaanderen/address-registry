namespace AddressRegistry.Api.Backoffice.Address.Requests
{
    using System;
    using Newtonsoft.Json;

    public class CorrectAddressRequest
    {
        [JsonProperty("heeftPostinfo")]
        public Uri PostalCode { get; set; }

        [JsonProperty("status")]
        public Uri Status { get; set; }

        [JsonProperty("officieelToegekend")]
        public bool OfficiallyAssigned { get; set; }

        [JsonProperty("positie")]
        public AddressPosition Position { get; set; }
    }
}
