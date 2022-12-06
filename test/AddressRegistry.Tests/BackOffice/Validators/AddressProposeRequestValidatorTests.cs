namespace AddressRegistry.Tests.BackOffice.Validators
{
    using System.Threading;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Validators;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using FluentValidation.TestHelper;
    using Infrastructure;
    using Moq;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using Xunit;

    public class AddressProposeRequestValidatorTests
    {
        private readonly AddressProposeRequestValidator _sut;
        private readonly Mock<IStreamStore> _streamStore;

        public AddressProposeRequestValidatorTests()
        {
            _streamStore = new Mock<IStreamStore>();
            _sut = new AddressProposeRequestValidator(
                new StreetNameExistsValidator(_streamStore.Object),
                new TestSyndicationContext());
        }

        private void WithStreamExists()
        {
            _streamStore
                .Setup(store => store.ReadStreamBackwards(It.IsAny<StreamId>(), StreamVersion.End, 1, false, CancellationToken.None))
                .ReturnsAsync(() => new ReadStreamPage("id", PageReadStatus.Success, 1, 2, 2, 2, ReadDirection.Backward, false, messages: new []{ new StreamMessage() }));
        }

        [Fact]
        public void GivenNoSpecificationPosition_ThenReturnsExpectedFailure()
        {
            WithStreamExists();

            var result = _sut.TestValidate(new AddressProposeRequest
            {
                PostInfoId = "12",
                StraatNaamId = "34",
                Huisnummer = "56",
                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                Positie = GeometryHelpers.GmlPointGeometry
            });

            result.ShouldHaveValidationErrorFor(nameof(AddressProposeRequest.PositieSpecificatie))
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
            WithStreamExists();

            var result = _sut.TestValidate(new AddressProposeRequest
            {
                PostInfoId = "12",
                StraatNaamId = "34",
                Huisnummer = "56",
                PositieGeometrieMethode = PositieGeometrieMethode.AfgeleidVanObject,
                PositieSpecificatie = specificatie
            });

            result.ShouldHaveValidationErrorFor(nameof(AddressProposeRequest.PositieSpecificatie))
                .WithErrorCode("AdresPositieSpecificatieValidatie")
                .WithErrorMessage("Ongeldige positieSpecificatie.");
        }

        [Fact]
        public void GivenNoPosition_ThenReturnsExpectedFailure()
        {
            WithStreamExists();

            var result = _sut.TestValidate(new AddressProposeRequest
            {
                PostInfoId = "12",
                StraatNaamId = "34",
                Huisnummer = "56",
                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                PositieSpecificatie = PositieSpecificatie.Ingang
            });

            result.ShouldHaveValidationErrorFor(nameof(AddressProposeRequest.Positie))
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
            WithStreamExists();

            var result = _sut.TestValidate(new AddressProposeRequest
            {
                PostInfoId = "12",
                StraatNaamId = "34",
                Huisnummer = "56",
                PositieGeometrieMethode = PositieGeometrieMethode.AfgeleidVanObject,
                PositieSpecificatie = PositieSpecificatie.Gebouweenheid,
                Positie = position
            });

            result.ShouldHaveValidationErrorFor(nameof(AddressProposeRequest.Positie))
                .WithErrorCode("AdresPositieformaatValidatie")
                .WithErrorMessage("De positie is geen geldige gml-puntgeometrie.");
        }
    }
}
