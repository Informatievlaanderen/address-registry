namespace AddressRegistry.Api.BackOffice.Abstractions.Requests
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Converters;
    using MediatR;
    using Newtonsoft.Json;
    using Responses;
    using StreetName;
    using StreetName.Commands;
    using Swashbuckle.AspNetCore.Filters;

    [DataContract(Name = "CorrigerenAdresPositie", Namespace = "")]
    public class AddressCorrectPositionRequest : IRequest<ETagResponse>
    {
        /// <summary>
        /// De unieke en persistente identificator van het adres.
        /// </summary>
        [JsonIgnore]
        public int PersistentLocalId { get; set; }

        /// <summary>
        /// De geometriemethode van het adres.
        /// </summary>
        [DataMember(Name = "PositieGeometrieMethode", Order = 1)]
        [JsonProperty(Required = Required.Always)]
        public PositieGeometrieMethode PositieGeometrieMethode { get; set; }

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

        [JsonIgnore]
        public IDictionary<string, object> Metadata { get; set; }

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

    public class AddressCorrectPositionRequestExamples : IExamplesProvider<AddressCorrectPositionRequest>
    {
        public AddressCorrectPositionRequest GetExamples()
        {
            return new AddressCorrectPositionRequest
            {
                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                PositieSpecificatie = PositieSpecificatie.Ingang,
                Positie = "<gml:Point srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>103671.37 192046.71</gml:pos></gml:Point>",
            };
        }
    }
}
