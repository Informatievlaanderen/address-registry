namespace AddressRegistry.Tests.BackOffice.Api.WhenCorrectingAddressPosition
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Infrastructure;
    using AddressRegistry.Api.BackOffice.Validators;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using FluentAssertions;
    using FluentValidation;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using StreetName;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenInvalidRequest : BackOfficeApiTest
    {
        private readonly AddressController _controller;

        public GivenInvalidRequest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateApiBusControllerWithUser<AddressController>();
        }

        [Fact]
        public void WithGeometryMethodIsInvalid_ThenThrowsValidationException()
        {
            var act = SetupController(new CorrectAddressPositionRequest
            {
                PositieGeometrieMethode = 0
            });

            // Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                    x.Errors.Any(e => e.ErrorCode == "AdresPositieGeometriemethodeValidatie"
                                      && e.ErrorMessage == "Ongeldige positieGeometrieMethode."));
        }

        [Theory]
        [InlineData(PositieSpecificatie.Lot)]
        [InlineData(PositieSpecificatie.Standplaats)]
        [InlineData(PositieSpecificatie.Ligplaats)]
        [InlineData(PositieSpecificatie.Ingang)]
        public void WithInvalidSpecificationAndDerivedFromObject_ThenThrowsValidationException(PositieSpecificatie specificatie)
        {
            var act = SetupController(new CorrectAddressPositionRequest
            {
                PositieGeometrieMethode = PositieGeometrieMethode.AfgeleidVanObject,
                PositieSpecificatie = specificatie,
                Positie = GeometryHelpers.GmlPointGeometry
            });

            // Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                    x.Errors.Any(e => e.ErrorCode == "AdresPositieSpecificatieValidatie"
                                      && e.ErrorMessage == "Ongeldige positieSpecificatie."));
        }

        [Fact]
        public void WithNoPosition_ThenThrowsValidationException()
        {
            var act = SetupController(new CorrectAddressPositionRequest
            {
                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                PositieSpecificatie = PositieSpecificatie.Ingang
            });

            // Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                    x.Errors.Any(e => e.ErrorCode == "AdresPositieVerplicht"
                                      && e.ErrorMessage == "De positie is verplicht."));
        }

        [Fact]
        public void WithInvalidGml_ThenThrowsValidationException()
        {
            var act = SetupController(new CorrectAddressPositionRequest
            {
                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                PositieSpecificatie = PositieSpecificatie.Ingang,
                Positie = "invalid gml"
            });

            // Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                    x.Errors.Any(e => e.ErrorCode == "AdresPositieformaatValidatie"
                                      && e.ErrorMessage == "De positie is geen geldige gml-puntgeometrie."));
        }

        private Func<Task<IActionResult>> SetupController(CorrectAddressPositionRequest request)
        {
            var addressPersistentLocalId = new AddressPersistentLocalId(123);

            return async () => await _controller.CorrectPosition(new CorrectAddressPositionRequestValidator(),
                Mock.Of<IIfMatchHeaderValidator>(),
                addressPersistentLocalId,
                request,
                null,
                CancellationToken.None);
        }
    }
}
