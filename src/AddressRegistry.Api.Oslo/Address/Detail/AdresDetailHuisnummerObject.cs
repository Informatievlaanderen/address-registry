namespace AddressRegistry.Api.Oslo.Address.Detail
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;

    /// <summary>
    /// Het huisnummer waaraan het busnummer is gekoppeld.
    /// </summary>
    [DataContract(Name = "AdresDetailHuisnummerObject", Namespace = "")]
    public class AdresDetailHuisnummerObject
    {
        /// <summary>
        /// De objectidentificator van het huisnummer waaraan het busnummer is gekoppeld.
        /// </summary>
        [DataMember(Name = "ObjectId", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public int ObjectId { get; set; }

        /// <summary>
        /// De URL die de details van de meest recente versie van het huisnummer waaraan het busnummer is gekoppeld weergeeft.
        /// </summary>
        [DataMember(Name = "Detail", Order = 2)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string Detail { get; set; }

        public AdresDetailHuisnummerObject(int objectId, string detail)
        {
            ObjectId = objectId;
            Detail = detail;
        }
    }
}
