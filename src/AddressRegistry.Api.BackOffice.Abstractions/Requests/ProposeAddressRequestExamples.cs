namespace AddressRegistry.Api.BackOffice.Abstractions.Requests
{
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using Swashbuckle.AspNetCore.Filters;

    public class ProposeAddressRequestExamples : IExamplesProvider<ProposeAddressRequest>
    {
        public ProposeAddressRequest GetExamples()
        {
            return new ProposeAddressRequest
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
