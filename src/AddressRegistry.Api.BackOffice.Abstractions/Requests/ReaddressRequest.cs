namespace AddressRegistry.Api.BackOffice.Abstractions.Requests
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;

    [DataContract(Name = "HerAdresseerAdressen", Namespace = "")]
    public sealed class ReaddressRequest
    {
        [DataMember(Name = "DoelStraatnaamId", Order = 0)]
        [JsonProperty(Required = Required.Always)]
        public string DoelStraatnaamId { get; set; }

        [DataMember(Name = "HerAdresseer", Order = 1)]
        [JsonProperty(Required = Required.Always)]
        public List<AddressToReaddressItem> HerAdresseer { get; set; }

        [DataMember(Name = "OpheffenAdressen", Order = 2)]
        [JsonProperty(Required = Required.Default)]
        public List<string>? OpheffenAdressen { get; set; }
    }

    [DataContract(Name = "HernummerAdresItem", Namespace = "")]
    public sealed record AddressToReaddressItem
    {
        [DataMember(Name = "BronAdresId", Order = 0)]
        [JsonProperty(Required = Required.Always)]
        public string BronAdresId { get; set; }

        [DataMember(Name = "DoelHuisnummer", Order = 1)]
        [JsonProperty(Required = Required.Always)]
        public string DoelHuisnummer { get; set; }
    }
}
