namespace AddressRegistry.Api.BackOffice.Abstractions.Requests
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Converters;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using MediatR;
    using Newtonsoft.Json;
    using Responses;
    using StreetName;
    using StreetName.Commands;
    using Swashbuckle.AspNetCore.Filters;

    [DataContract(Name = "VoorstelAdres", Namespace = "")]
    public class AddressProposeRequest : BackOfficeProposeRequest, IRequest<PersistentLocalIdETagResponse>
    {
        [JsonIgnore]
        public IDictionary<string, object> Metadata { get; set; }

        /// <summary>
        /// Map to ProposeAddress command
        /// </summary>
        /// <returns>ProposeAddress.</returns>
        public ProposeAddress ToCommand(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            PostalCode postalCode,
            MunicipalityId postalCodeMunicipalityId,
            AddressPersistentLocalId addressPersistentLocalId,
            Provenance provenance)
        {
            return new ProposeAddress(
                streetNamePersistentLocalId,
                postalCode,
                postalCodeMunicipalityId,
                addressPersistentLocalId,
                HouseNumber.Create(Huisnummer),
                string.IsNullOrWhiteSpace(Busnummer) ? null : new BoxNumber(Busnummer),
                PositieGeometrieMethode.Map(),
                PositieSpecificatie.Map(),
                string.IsNullOrWhiteSpace(Positie) ? null : Positie.ToExtendedWkbGeometry(),
                provenance);
        }
    }

    public class AddressProposeRequestExamples : IExamplesProvider<AddressProposeRequest>
    {
        public AddressProposeRequest GetExamples()
        {
            return new AddressProposeRequest
            {
                StraatNaamId = "https://data.vlaanderen.be/id/straatnaam/45041",
                PostInfoId = "https://data.vlaanderen.be/id/postinfo/9000",
                Huisnummer = "11",
                Busnummer = "3A",
                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                PositieSpecificatie = PositieSpecificatie.Ingang,
                Positie = "<gml:Point srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>103671.37 192046.71</gml:pos></gml:Point>",
            };
        }
    }
}
