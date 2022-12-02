namespace AddressRegistry.Api.BackOffice.Abstractions.Requests
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using Converters;
    using MediatR;
    using Newtonsoft.Json;
    using StreetName;
    using StreetName.Commands;
    using Swashbuckle.AspNetCore.Filters;

    [DataContract(Name = "CorrigerenAdresPositie", Namespace = "")]
    public class AddressCorrectPositionRequest : CorrectAddressPositionBackOfficeRequest, IRequest<ETagResponse>
    {
        /// <summary>
        /// De unieke en persistente identificator van het adres.
        /// </summary>
        [JsonIgnore]
        public int PersistentLocalId { get; set; }

        [JsonIgnore]
        public IDictionary<string, object> Metadata { get; set; }

        /// <summary>
        /// Map to CorrectAddressPosition command
        /// </summary>
        /// <returns>CorrectAddressPosition.</returns>
        public CorrectAddressPosition ToCommand(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            Provenance provenance)
        {
            return new CorrectAddressPosition(
                streetNamePersistentLocalId,
                new AddressPersistentLocalId(PersistentLocalId),
                PositieGeometrieMethode.Map(),
                PositieSpecificatie.Map(),
                Positie.ToExtendedWkbGeometry(),
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
