namespace AddressRegistry.Tests.BackOffice.Validators
{
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Validators;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using FluentValidation.TestHelper;
    using Infrastructure;
    using Xunit;

    public class AddressProposeRequestValidatorTests
    {
        private readonly AddressProposeRequestValidator _sut;

        public AddressProposeRequestValidatorTests()
        {
            _sut = new AddressProposeRequestValidator(new TestSyndicationContext());
        }

        [Fact]
        public void GivenNoPositionSpecificationAndPositionGeometryMethodIsAppointedByAdministrator_ThenReturnsExpectedFailure()
        {
            var result = _sut.TestValidate(new AddressProposeRequest
            {
                PostInfoId = "12",
                StraatNaamId = "34",
                Huisnummer = "56",
                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder
            });

            result.ShouldHaveValidationErrorFor(nameof(AddressProposeRequest.PositieSpecificatie))
                .WithErrorCode("AdresspecificatieVerplichtBijManueleAanduiding")
                .WithErrorMessage("Positiespecificatie is verplicht bij een manuele aanduiding van de positie.");
        }

        [Theory]
        [InlineData(PositieSpecificatie.Gemeente)]
        public void GivenInvalidPositionSpecificationForPositionGeometryMethodAppointedByAdministrator_ThenReturnsExpectedFailure(PositieSpecificatie specificatie)
        {
            var result = _sut.TestValidate(new AddressProposeRequest
            {
                PostInfoId = "12",
                StraatNaamId = "34",
                Huisnummer = "56",
                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                PositieSpecificatie = specificatie
            });

            result.ShouldHaveValidationErrorFor(nameof(AddressProposeRequest.PositieSpecificatie))
                .WithErrorCode("AdresspecificatieValidatie")
                .WithErrorMessage("Ongeldige positiespecificatie.");
        }

        [Theory]
        [InlineData(PositieSpecificatie.Ingang)]
        [InlineData(PositieSpecificatie.Perceel)]
        [InlineData(PositieSpecificatie.Lot)]
        [InlineData(PositieSpecificatie.Standplaats)]
        [InlineData(PositieSpecificatie.Ligplaats)]
        public void GivenInvalidPositionSpecificationForPositionGeometryMethodDerivedFromObject_ThenReturnsExpectedFailure(PositieSpecificatie specificatie)
        {
            var result = _sut.TestValidate(new AddressProposeRequest
            {
                PostInfoId = "12",
                StraatNaamId = "34",
                Huisnummer = "56",
                PositieGeometrieMethode = PositieGeometrieMethode.AfgeleidVanObject,
                PositieSpecificatie = specificatie
            });

            result.ShouldHaveValidationErrorFor(nameof(AddressProposeRequest.PositieSpecificatie))
                .WithErrorCode("AdresspecificatieValidatie")
                .WithErrorMessage("Ongeldige positiespecificatie.");
        }

        [Fact]
        public void GivenNoPositionAndPositionGeometryMethodIsAppointedByAdministrator_ThenReturnsExpectedFailure()
        {
            var result = _sut.TestValidate(new AddressProposeRequest
            {
                PostInfoId = "12",
                StraatNaamId = "34",
                Huisnummer = "56",
                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                PositieSpecificatie = PositieSpecificatie.Ingang
            });

            result.ShouldHaveValidationErrorFor(nameof(AddressProposeRequest.Positie))
                .WithErrorCode("AdresGeometriemethodeValidatie")
                .WithErrorMessage("De parameter 'positie' is verplicht voor indien aangeduid door beheerder.");
        }

        [Theory]
        [InlineData("<gml:Point srsName=\"https://INVALIDURL\" xmlns:gml=\"http://www.opengis.net/gml/3.2\">" +
                    "<gml:pos>140285.15277253836 186725.74131567031</gml:pos></gml:Point>")]
        [InlineData("<gml:Point missingSrSNameAttribute=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\">" +
                    "<gml:pos>140285.15277253836 186725.74131567031</gml:pos></gml:Point>")]
        [InlineData("<gml:Point srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\">" +
                    "<gml:missingPositionAttribute>140285.15277253836 186725.74131567031</gml:pos></gml:Point>")]
        public void GivenInvalidPosition_ThenReturnsExpectedFailure(string position)
        {
            var result = _sut.TestValidate(new AddressProposeRequest
            {
                PostInfoId = "12",
                StraatNaamId = "34",
                Huisnummer = "56",
                PositieGeometrieMethode = PositieGeometrieMethode.AfgeleidVanObject,
                PositieSpecificatie = PositieSpecificatie.Gemeente,
                Positie = position
            });

            result.ShouldHaveValidationErrorFor(nameof(AddressProposeRequest.Positie))
                .WithErrorCode("AdrespositieFormaatValidatie")
                .WithErrorMessage("De positie is geen geldige gml-puntgeometrie.");
        }
    }
}
