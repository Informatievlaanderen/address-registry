namespace AddressRegistry.Api.BackOffice.Abstractions.Requests
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using System.Runtime.Serialization;

    [DataContract(Name = "CorrigerenBusnummerAdressen", Namespace = "")]
    public sealed class CorrectAddressBoxNumbersRequest
    {
        /// <summary>
        /// Het busnummer per adres.
        /// </summary>
        [DataMember(Name = "Busnummers", Order = 0)]
        [JsonProperty(Required = Required.Always)]
        public List<CorrectAddressBoxNumbersRequestItem> Busnummers { get; set; }
    }

    [DataContract(Name = "CorrigerenBusnummerAdressenItem", Namespace = "")]
    public sealed class CorrectAddressBoxNumbersRequestItem
    {
        /// <summary>
        /// De unieke en persistente identificator van het adres.
        /// </summary>
        [DataMember(Name = "AdresId", Order = 0)]
        [JsonProperty(Required = Required.Always)]
        public string AdresId { get; set; }

        /// <summary>
        /// Het busnummer van het adres.
        /// </summary>
        [DataMember(Name = "Busnummer", Order = 1)]
        [JsonProperty(Required = Required.Always)]
        public string Busnummer { get; set; }
    }
}
