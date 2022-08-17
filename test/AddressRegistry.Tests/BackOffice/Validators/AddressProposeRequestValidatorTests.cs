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
        public void GivenNoPositionSpecificationAndPositionGeometryMethodIsAppointedByAdministration_ThenReturnsExpectedFailure()
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
        [InlineData(PositieSpecificatie.Wegsegment)]
        public void GivenInvalidPositionSpecificationForPositionGeometryMethodAppointedByAdministration_ThenReturnsExpectedFailure(PositieSpecificatie specificatie)
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
    }
}
