namespace AddressRegistry.Api.BackOffice.Abstractions.Requests
{
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Converters;
    using Newtonsoft.Json;
    using StreetName;
    using StreetName.Commands;

    [DataContract(Name = "CorrigerenAdresPositie", Namespace = "")]
    public class SqsAddressCorrectPositionRequest : SqsRequest
    {
        /// <summary>
        /// De unieke en persistente identificator van het adres.
        /// </summary>
        [JsonIgnore]
        public int PersistentLocalId { get; set; }

        /// <summary>
        /// De geometriemethode van het adres.
        /// </summary>
        [DataMember(Name = "PositieGeometriemethode", Order = 1)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public PositieGeometrieMethode? PositieGeometrieMethode { get; set; }

        /// <summary>
        /// De specificatie van de adrespositie (optioneel).
        /// </summary>
        [DataMember(Name = "PositieSpecificatie", Order = 2)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public PositieSpecificatie? PositieSpecificatie { get; set; }

        /// <summary>
        /// Puntgeometrie van het adres in GML-3 formaat met Lambert 72 referentie systeem.
        /// </summary>
        [DataMember(Name = "Positie", Order = 3)]
        [JsonProperty(Required = Required.Default)]
        public string? Positie { get; set; }

        /// <summary>
        /// Map to CorrectAddressPosition command
        /// </summary>
        /// <returns>CorrectAddressPosition.</returns>
        public CorrectAddressPosition ToCommand(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            ExtendedWkbGeometry? position,
            Provenance provenance)
        {
            return new CorrectAddressPosition(
                streetNamePersistentLocalId,
                new AddressPersistentLocalId(PersistentLocalId),
                PositieGeometrieMethode.Map(),
                PositieSpecificatie.Map(),
                position,
                provenance);
        }
    }
}
