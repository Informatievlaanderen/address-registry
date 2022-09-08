namespace AddressRegistry.Tests.BackOffice.Validators
{
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Validators;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using FluentAssertions;
    using FluentValidation.TestHelper;
    using Xunit;

    public class AddressChangePositionRequestValidatorTests
    {
        private readonly AddressChangePositionRequestValidator _sut;

        public AddressChangePositionRequestValidatorTests()
        {
            _sut = new AddressChangePositionRequestValidator();
        }

        [Fact]
        public void GivenNoPositionSpecificationAndPositionGeometryMethodIsAppointedByAdministrator_ThenReturnsExpectedFailure()
        {
            var result = _sut.TestValidate(new AddressChangePositionRequest
            {
                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                Positie = GeometryHelpers.GmlPointGeometry
            });

            result.Errors.Count.Should().Be(1);
            result.ShouldHaveValidationErrorFor(nameof(AddressProposeRequest.PositieSpecificatie))
                .WithErrorCode("AdresPositieSpecificatieVerplichtBijManueleAanduiding")
                .WithErrorMessage("PositieSpecificatie is verplicht bij een manuele aanduiding van de positie.");
        }

        [Theory]
        [InlineData(PositieSpecificatie.Gemeente)]
        public void GivenInvalidPositionSpecificationForPositionGeometryMethodAppointedByAdministrator_ThenReturnsExpectedFailure(PositieSpecificatie specificatie)
        {
            var result = _sut.TestValidate(new AddressChangePositionRequest
            {
                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                PositieSpecificatie = specificatie,
                Positie = GeometryHelpers.GmlPointGeometry
            });

            result.Errors.Count.Should().Be(1);
            result.ShouldHaveValidationErrorFor(nameof(AddressProposeRequest.PositieSpecificatie))
                .WithErrorCode("AdresPositieSpecificatieValidatie")
                .WithErrorMessage("Ongeldige positieSpecificatie.");
        }

        [Theory]
        [InlineData(PositieSpecificatie.Ingang)]
        [InlineData(PositieSpecificatie.Perceel)]
        [InlineData(PositieSpecificatie.Lot)]
        [InlineData(PositieSpecificatie.Standplaats)]
        [InlineData(PositieSpecificatie.Ligplaats)]
        public void GivenInvalidPositionSpecificationForPositionGeometryMethodDerivedFromObject_ThenReturnsExpectedFailure(PositieSpecificatie specificatie)
        {
            var result = _sut.TestValidate(new AddressChangePositionRequest
            {
                PositieGeometrieMethode = PositieGeometrieMethode.AfgeleidVanObject,
                PositieSpecificatie = specificatie
            });

            result.Errors.Count.Should().Be(1);
            result.ShouldHaveValidationErrorFor(nameof(AddressProposeRequest.PositieSpecificatie))
                .WithErrorCode("AdresPositieSpecificatieValidatie")
                .WithErrorMessage("Ongeldige positieSpecificatie.");
        }

        [Fact]
        public void GivenNoPositionAndPositionGeometryMethodIsAppointedByAdministrator_ThenReturnsExpectedFailure()
        {
            var result = _sut.TestValidate(new AddressChangePositionRequest
            {
                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                PositieSpecificatie = PositieSpecificatie.Ingang
            });

            result.Errors.Count.Should().Be(1);
            result.ShouldHaveValidationErrorFor(nameof(AddressProposeRequest.Positie))
                .WithErrorCode("AdresPositieGeometriemethodeValidatie")
                .WithErrorMessage("De parameter 'positie' is verplicht indien positieGeometrieMethode aangeduidDoorBeheerder is.");
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
            var result = _sut.TestValidate(new AddressChangePositionRequest
            {
                PositieGeometrieMethode = PositieGeometrieMethode.AfgeleidVanObject,
                PositieSpecificatie = PositieSpecificatie.Gemeente,
                Positie = position
            });

            result.Errors.Count.Should().Be(1);
            result.ShouldHaveValidationErrorFor(nameof(AddressProposeRequest.Positie))
                .WithErrorCode("AdresPositieformaatValidatie")
                .WithErrorMessage("De positie is geen geldige gml-puntgeometrie.");
        }
    }
}
