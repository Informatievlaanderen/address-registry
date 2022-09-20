namespace AddressRegistry.Api.BackOffice.Abstractions.Requests
{
    using MediatR;
    using Newtonsoft.Json;
    using Responses;
    using System.Runtime.Serialization;

    [DataContract(Name = "CorrigerenPostcodeAdres", Namespace = "")]
    public sealed class AddressBackOfficeCorrectPostalCodeRequest
    {
        /// <summary>
        /// De unieke en persistente identificator van het adres.
        /// </summary>
        [JsonIgnore]
        public int PersistentLocalId { get; set; }

        /// <summary>
        /// De unieke en persistente identificator van de postcode van het adres.
        /// </summary>
        [DataMember(Name = "PostinfoId", Order = 0)]
        [JsonProperty(Required = Required.Always)]
        public string PostInfoId { get; set; }
    }
}
