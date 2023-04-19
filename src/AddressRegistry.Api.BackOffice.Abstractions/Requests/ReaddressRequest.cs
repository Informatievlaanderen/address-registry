namespace AddressRegistry.Api.BackOffice.Abstractions.Requests
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;

    [DataContract(Name = "HerAdresseerAdressen", Namespace = "")]
    public sealed class ReaddressRequest
    {
        /// <summary>
        /// De unieke en persistente identificator van de straatnaam naar welke het bronadres dient verhangen te worden.
        /// </summary>
        [DataMember(Name = "DoelStraatnaamId", Order = 0)]
        [JsonProperty(Required = Required.Always)]
        public string DoelStraatnaamId { get; set; }

        /// <summary>
        /// Lijst van te hernummeren adressen welke een bron- en doeladres bevatten.
        /// </summary>
        [DataMember(Name = "HerAdresseer", Order = 1)]
        [JsonProperty(Required = Required.Always)]
        public List<AddressToReaddressItem> HerAdresseer { get; set; }

        /// <summary>
        ///  Lijst van adressen welke afgekeurd of gehistoreerd worden.
        /// </summary>
        [DataMember(Name = "OpheffenAdressen", Order = 2)]
        [JsonProperty(Required = Required.Default)]
        public List<string>? OpheffenAdressen { get; set; }
    }

    [DataContract(Name = "HernummerAdresItem", Namespace = "")]
    public sealed record AddressToReaddressItem
    {
        /// <summary>
        /// De unieke en persistente identificator van het adres.
        /// </summary>
        [DataMember(Name = "BronAdresId", Order = 0)]
        [JsonProperty(Required = Required.Always)]
        public string BronAdresId { get; set; }

        /// <summary>
        /// Het huisnummer van het adres.
        /// </summary>
        [DataMember(Name = "DoelHuisnummer", Order = 1)]
        [JsonProperty(Required = Required.Always)]
        public string DoelHuisnummer { get; set; }
    }
}
