namespace AddressRegistry.Tests.BackOffice.Api.WhenChangingAddressPosition
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice;
    using AddressRegistry.Api.BackOffice.Abstractions;
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

    public class GivenInvalidRequest : AddressRegistryBackOfficeTest
    {
        private readonly AddressController _controller;

        public GivenInvalidRequest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateApiBusControllerWithUser<AddressController>();
        }

        [Fact]
        public void WithGeometryMethodHasNoValue_ThenThrowsValidationException()
        {
            var act = SetupController(new AddressChangePositionRequest
            {
                PositieGeometrieMethode = null
            }); ;

            // Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                    x.Errors.Any(e => e.ErrorCode == "AdresPositieGeometriemethodeValidatie"
                                      && e.ErrorMessage == "Ongeldige geometriemethode."));
        }

        [Fact]
        public void WithGeometryMethodIsInvalid_ThenThrowsValidationException()
        {
            var act = SetupController(new AddressChangePositionRequest
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
                                      && e.ErrorMessage == "Ongeldige geometriemethode."));
        }

        [Fact]
        public void WithSpecificationHasNoValueAndMethodAppointedByAdmin_ThenThrowsValidationException()
        {
            var act = SetupController(new AddressChangePositionRequest
            {
                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                PositieSpecificatie = null
            });

            // Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                    x.Errors.Any(e => e.ErrorCode == "AdresspecificatieVerplichtBijManueleAanduiding"
                                      && e.ErrorMessage == "Positiespecificatie is verplicht bij een manuele aanduiding van de positie."));
        }

        [Fact]
        public void WithInvalidSpecificationAndMethodAppointedByAdmin_ThenThrowsValidationException()
        {
            var act = SetupController(new AddressChangePositionRequest
            {
                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                PositieSpecificatie = PositieSpecificatie.Gemeente
            });

            // Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                    x.Errors.Any(e => e.ErrorCode == "AdresspecificatieValidatie"
                                      && e.ErrorMessage == "Ongeldige positiespecificatie."));
        }

        [Fact]
        public void WithNoPositionAndMethodAppointedByAdmin_ThenThrowsValidationException()
        {
            var act = SetupController(new AddressChangePositionRequest
            {
                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                PositieSpecificatie = PositieSpecificatie.Ingang,
                Positie = null
            });

            // Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                    x.Errors.Any(e => e.ErrorCode == "AdresPositieGeometriemethodeValidatie"
                                      && e.ErrorMessage == "De parameter 'positie' is verplicht voor indien aangeduid door beheerder."));
        }

        [Fact]
        public void WithInvalidGml_ThenThrowsValidationException()
        {
            var act = SetupController(new AddressChangePositionRequest
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

        [Theory]
        [InlineData(PositieSpecificatie.Perceel)]
        [InlineData(PositieSpecificatie.Lot)]
        [InlineData(PositieSpecificatie.Standplaats)]
        [InlineData(PositieSpecificatie.Ligplaats)]
        [InlineData(PositieSpecificatie.Ingang)]
        public void WithInvalidSpecificationAndDerivedFromObject_ThenThrowsValidationException(PositieSpecificatie specificatie)
        {
            var act = SetupController(new AddressChangePositionRequest
            {
                PositieGeometrieMethode = PositieGeometrieMethode.AfgeleidVanObject,
                PositieSpecificatie = specificatie
            });

            // Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                    x.Errors.Any(e => e.ErrorCode == "AdresspecificatieValidatie"
                                      && e.ErrorMessage == "Ongeldige positiespecificatie."));
        }

        private Func<Task<IActionResult>> SetupController(AddressChangePositionRequest request)
        {
            var addressPersistentLocalId = new AddressPersistentLocalId(123);

            return async () => await _controller.ChangePosition(
                Mock.Of<BackOfficeContext>(),
                new AddressChangePositionRequestValidator(),
                Mock.Of<IIfMatchHeaderValidator>(),
                ResponseOptions,
                addressPersistentLocalId,
                request,
                null,
                CancellationToken.None);
        }
    }
}
