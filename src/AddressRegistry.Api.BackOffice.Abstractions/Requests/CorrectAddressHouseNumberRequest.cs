namespace AddressRegistry.Api.BackOffice.Abstractions.Requests
{
    using Newtonsoft.Json;
    using System.Runtime.Serialization;

    [DataContract(Name = "CorrigerenHuisnummerAdres", Namespace = "")]
    public sealed class CorrectAddressHouseNumberRequest
    {
        /// <summary>
        /// Het huisnummer van het adres.
        /// </summary>
        [DataMember(Name = "Huisnummer", Order = 0)]
        [JsonProperty(Required = Required.Always)]
        public string Huisnummer { get; set; }
    }
}
