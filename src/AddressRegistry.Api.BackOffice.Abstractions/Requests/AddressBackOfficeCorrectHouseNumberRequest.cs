namespace AddressRegistry.Api.BackOffice.Abstractions.Requests
{
    using MediatR;
    using Newtonsoft.Json;
    using Responses;
    using System.Runtime.Serialization;

    [DataContract(Name = "CorrigerenHuisnummerAdres", Namespace = "")]
    public sealed class AddressBackOfficeCorrectHouseNumberRequest
    {
        /// <summary>
        /// Het huisnummer van het adres.
        /// </summary>
        [DataMember(Name = "Huisnummer", Order = 0)]
        [JsonProperty(Required = Required.Always)]
        public string Huisnummer { get; set; }
    }
}
