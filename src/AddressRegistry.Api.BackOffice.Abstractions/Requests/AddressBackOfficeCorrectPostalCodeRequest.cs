namespace AddressRegistry.Api.BackOffice.Abstractions.Requests
{
    using Newtonsoft.Json;
    using System.Runtime.Serialization;

    [DataContract(Name = "CorrigerenPostcodeAdres", Namespace = "")]
    public class AddressBackOfficeCorrectPostalCodeRequest
    {
        /// <summary>
        /// De unieke en persistente identificator van de postcode van het adres.
        /// </summary>
        [DataMember(Name = "PostinfoId", Order = 0)]
        [JsonProperty(Required = Required.Always)]
        public string PostInfoId { get; set; }
    }
}
