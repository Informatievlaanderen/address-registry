namespace AddressRegistry.Api.BackOffice.Abstractions.Requests
{
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using Swashbuckle.AspNetCore.Filters;

    public class ChangeAddressPositionRequestExamples : IExamplesProvider<ChangeAddressPositionRequest>
    {
        public ChangeAddressPositionRequest GetExamples()
        {
            return new ChangeAddressPositionRequest
            {
                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                PositieSpecificatie = PositieSpecificatie.Ingang,
                Positie = "<gml:Point srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>103671.37 192046.71</gml:pos></gml:Point>",
            };
        }
    }
}
