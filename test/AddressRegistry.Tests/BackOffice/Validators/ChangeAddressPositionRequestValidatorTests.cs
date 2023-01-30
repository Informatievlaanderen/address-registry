namespace AddressRegistry.Tests.BackOffice.Validators
{
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Validators;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using FluentAssertions;
    using FluentValidation.TestHelper;
    using Xunit;

    public class ChangeAddressPositionRequestValidatorTests
    {
        private readonly ChangeAddressPositionRequestValidator _sut;

        public ChangeAddressPositionRequestValidatorTests()
        {
            _sut = new ChangeAddressPositionRequestValidator();
        }

        [Fact]
        public void GivenNoPositionSpecification_ThenReturnsExpectedFailure()
        {
            var result = _sut.TestValidate(new ChangeAddressPositionRequest
            {
                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                Positie = GeometryHelpers.GmlPointGeometry
            });

            result.Errors.Count.Should().Be(1);
            result.ShouldHaveValidationErrorFor(nameof(ChangeAddressPositionRequest.PositieSpecificatie))
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
            var result = _sut.TestValidate(new ChangeAddressPositionRequest
            {
                PositieGeometrieMethode = PositieGeometrieMethode.AfgeleidVanObject,
                PositieSpecificatie = specificatie,
                Positie = GeometryHelpers.GmlPointGeometry
            });

            result.Errors.Count.Should().Be(1);
            result.ShouldHaveValidationErrorFor(nameof(ChangeAddressPositionRequest.PositieSpecificatie))
                .WithErrorCode("AdresPositieSpecificatieValidatie")
                .WithErrorMessage("Ongeldige positieSpecificatie.");
        }

        [Fact]
        public void GivenNoPosition_ThenReturnsExpectedFailure()
        {
            var result = _sut.TestValidate(new ChangeAddressPositionRequest
            {
                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                PositieSpecificatie = PositieSpecificatie.Ingang
            });

            result.Errors.Count.Should().Be(1);
            result.ShouldHaveValidationErrorFor(nameof(ChangeAddressPositionRequest.Positie))
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
            var result = _sut.TestValidate(new ChangeAddressPositionRequest
            {
                PositieGeometrieMethode = PositieGeometrieMethode.AfgeleidVanObject,
                PositieSpecificatie = PositieSpecificatie.Gebouweenheid,
                Positie = position
            });

            result.Errors.Count.Should().Be(1);
            result.ShouldHaveValidationErrorFor(nameof(ChangeAddressPositionRequest.Positie))
                .WithErrorCode("AdresPositieformaatValidatie")
                .WithErrorMessage("De positie is geen geldige gml-puntgeometrie.");
        }
    }
}
