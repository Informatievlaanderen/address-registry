namespace AddressRegistry.Tests.BackOffice.Validators
{
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Validators;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using FluentAssertions;
    using FluentValidation.TestHelper;
    using Xunit;

    public class CorrectAddressPositionRequestValidatorTests
    {
        private readonly CorrectAddressPositionRequestValidator _sut;

        public CorrectAddressPositionRequestValidatorTests()
        {
            _sut = new CorrectAddressPositionRequestValidator();
        }

        [Fact]
        public void GivenNoPositionSpecification_ThenReturnsExpectedFailure()
        {
            var result = _sut.TestValidate(new CorrectAddressPositionRequest
            {
                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                Positie = GeometryHelpers.GmlPointGeometry
            });

            result.Errors.Count.Should().Be(1);
            result.ShouldHaveValidationErrorFor(nameof(CorrectAddressPositionRequest.PositieSpecificatie))
                .WithoutErrorCode("AdresPositieSpecificatieValidatie")
                .WithErrorCode("AdresPositieSpecificatieVerplicht")
                .WithErrorMessage("PositieSpecificatie is verplicht.");
        }

        [Theory]
        [InlineData(PositieSpecificatie.Ingang)]
        [InlineData(PositieSpecificatie.Lot)]
        [InlineData(PositieSpecificatie.Standplaats)]
        [InlineData(PositieSpecificatie.Ligplaats)]
        public void GivenInvalidPositionSpecificationForPositionGeometryMethodDerivedFromObject_ThenReturnsExpectedFailure(PositieSpecificatie specificatie)
        {
            var result = _sut.TestValidate(new CorrectAddressPositionRequest
            {
                PositieGeometrieMethode = PositieGeometrieMethode.AfgeleidVanObject,
                PositieSpecificatie = specificatie,
                Positie = GeometryHelpers.GmlPointGeometry
            });

            result.ShouldHaveValidationErrorFor(nameof(CorrectAddressPositionRequest.PositieSpecificatie))
                .WithErrorCode("AdresPositieSpecificatieValidatie")
                .WithErrorMessage("Ongeldige positieSpecificatie.");
        }

        [Fact]
        public void GivenNoPositionAndPositionGeometryMethodIsAppointedByAdministrator_ThenReturnsExpectedFailure()
        {
            var result = _sut.TestValidate(new CorrectAddressPositionRequest
            {
                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                PositieSpecificatie = PositieSpecificatie.Ingang
            });

            result.ShouldHaveValidationErrorFor(nameof(CorrectAddressPositionRequest.Positie))
                .WithErrorCode("AdresPositieVerplicht")
                .WithErrorMessage("De positie is verplicht.");
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
            var result = _sut.TestValidate(new CorrectAddressPositionRequest
            {
                PositieGeometrieMethode = PositieGeometrieMethode.AfgeleidVanObject,
                PositieSpecificatie = PositieSpecificatie.Gebouweenheid,
                Positie = position
            });

            result.ShouldHaveValidationErrorFor(nameof(CorrectAddressPositionRequest.Positie))
                .WithErrorCode("AdresPositieformaatValidatie")
                .WithErrorMessage("De positie is geen geldige gml-puntgeometrie.");
        }
    }
}
