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

    [DataContract(Name = "WijzigenAdresPositie", Namespace = "")]
    public class AddressChangePositionRequest : ChangePositionBackOfficeRequest, IRequest<ETagResponse>
    {
        /// <summary>
        /// De unieke en persistente identificator van het adres.
        /// </summary>
        [JsonIgnore]
        public int PersistentLocalId { get; set; }

        [JsonIgnore]
        public IDictionary<string, object> Metadata { get; set; }

        /// <summary>
        /// Map to ChangeAddressPosition command
        /// </summary>
        /// <returns>ChangeAddressPosition.</returns>
        public ChangeAddressPosition ToCommand(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            Provenance provenance)
        {
            return new ChangeAddressPosition(
                streetNamePersistentLocalId,
                new AddressPersistentLocalId(PersistentLocalId),
                PositieGeometrieMethode.Map(),
                PositieSpecificatie.Map(),
                string.IsNullOrWhiteSpace(Positie) ? null : Positie.ToExtendedWkbGeometry(),
                provenance);
        }
    }

    public class AddressChangePositionRequestExamples : IExamplesProvider<AddressChangePositionRequest>
    {
        public AddressChangePositionRequest GetExamples()
        {
            return new AddressChangePositionRequest
            {
                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                PositieSpecificatie = PositieSpecificatie.Ingang,
                Positie = "<gml:Point srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>103671.37 192046.71</gml:pos></gml:Point>",
            };
        }
    }
}
