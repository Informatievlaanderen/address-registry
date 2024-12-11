namespace AddressRegistry.Api.BackOffice.Abstractions.Requests
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;

    [DataContract(Name = "SnapshotStraatnaam", Namespace = "")]
    public sealed class CreateStreetNameSnapshotRequest
    {
        /// <summary>
        /// De unieke en persistente identificator van de straatnaam.
        /// </summary>
        [DataMember(Name = "StreetNamePersistentLocalId", Order = 0)]
        [JsonProperty(Required = Required.Always)]
        public int StreetNamePersistentLocalId { get; set; }
    }
}
