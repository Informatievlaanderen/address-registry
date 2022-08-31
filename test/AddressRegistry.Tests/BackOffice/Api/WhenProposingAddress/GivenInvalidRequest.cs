namespace AddressRegistry.Tests.BackOffice.Api.WhenProposingAddress
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Abstractions.Responses;
    using AddressRegistry.Api.BackOffice.Validators;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using FluentAssertions;
    using FluentValidation;
    using Infrastructure;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using Projections.Syndication.PostalInfo;
    using Xunit;
    using Xunit.Abstractions;
    using PositieGeometrieMethode = Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts.PositieGeometrieMethode;
    using PositieSpecificatie = Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts.PositieSpecificatie;

    public class GivenInvalidRequest : AddressRegistryBackOfficeTest
    {
        private readonly AddressController _controller;

        public GivenInvalidRequest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            MockMediator.Setup(x => x.Send(It.IsAny<AddressProposeRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(new PersistentLocalIdETagResponse(123, "lasteventhash")));

            _controller = CreateApiBusControllerWithUser<AddressController>();
        }

        [Fact]
        public void WithInvalidStreetName_ThenThrowsValidationException()
        {
            var invalidStraatNaamId = "invalid";

            var act = SetupController(new AddressProposeRequest
            {
                StraatNaamId = invalidStraatNaamId,
                Huisnummer = "11"
            });

            // Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                    x.Errors.Any(e => e.ErrorCode == "AdresStraatnaamNietGekendValidatie"
                                      && e.ErrorMessage == $"De straatnaam '{invalidStraatNaamId}' is niet gekend in het straatnaamregister."));
        }

        [Fact]
        public void WithNonExistentPostInfo_ThenThrowsValidationException()
        {
            var nonExistentPostInfo = PostInfoPuri + "123456";

            var act = SetupController(new AddressProposeRequest
            {
                StraatNaamId = StraatNaamPuri + "123",
                Huisnummer = "11",
                PostInfoId = nonExistentPostInfo
            });

            // Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                    x.Errors.Any(e => e.ErrorCode == "AdresPostinfoNietGekendValidatie"
                                      && e.ErrorMessage == $"De postinfo '{nonExistentPostInfo}' is niet gekend in het postinforegister."));
        }

        [Fact]
        public void WithInvalidHouseNumber_ThenThrowsValidationException()
        {
            var act = SetupController(new AddressProposeRequest
            {
                StraatNaamId = StraatNaamPuri + "123",
                Huisnummer = "AA",
                PostInfoId = PostInfoPuri + "101"
            });

            // Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                    x.Errors.Any(e => e.ErrorCode == "AdresOngeldigHuisnummerformaat"
                                      && e.ErrorMessage == "Ongeldig huisnummerformaat."));
        }

        [Fact]
        public void WithInvalidBoxNumber_ThenThrowsValidationException()
        {
            var act = SetupController(new AddressProposeRequest
            {
                StraatNaamId = StraatNaamPuri + "123",
                Huisnummer = "11",
                Busnummer = "$Invalid$",
                PostInfoId = PostInfoPuri + "101"
            });

            // Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                    x.Errors.Any(e => e.ErrorCode == "AdresOngeldigBusnummerformaat"
                                      && e.ErrorMessage == "Ongeldig busnummerformaat."));
        }

        [Fact]
        public void WithGeometryMethodIsInvalid_ThenThrowsValidationException()
        {
            var act = SetupController(new AddressProposeRequest
            {
                StraatNaamId = StraatNaamPuri + "123",
                Huisnummer = "11",
                Busnummer = "AA",
                PostInfoId = PostInfoPuri + "101",
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

        [Fact]
        public void WithSpecificationHasNoValueAndMethodAppointedByAdmin_ThenThrowsValidationException()
        {
            var act = SetupController(new AddressProposeRequest
            {
                StraatNaamId = StraatNaamPuri + "123",
                Huisnummer = "11",
                Busnummer = "AA",
                PostInfoId = PostInfoPuri + "101",

                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                PositieSpecificatie = null
            });

            // Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                    x.Errors.Any(e => e.ErrorCode == "AdresPositieSpecificatieVerplichtBijManueleAanduiding"
                                      && e.ErrorMessage == "PositieSpecificatie is verplicht bij een manuele aanduiding van de positie."));
        }

        [Fact]
        public void WithInvalidSpecificationAndMethodAppointedByAdmin_ThenThrowsValidationException()
        {
            var act = SetupController(new AddressProposeRequest
            {
                StraatNaamId = StraatNaamPuri + "123",
                Huisnummer = "11",
                Busnummer = "AA",
                PostInfoId = PostInfoPuri + "101",

                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                PositieSpecificatie = PositieSpecificatie.Gemeente
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
        public void WithNoPositionAndMethodAppointedByAdmin_ThenThrowsValidationException()
        {
            var act = SetupController(new AddressProposeRequest
            {
                StraatNaamId = StraatNaamPuri + "123",
                Huisnummer = "11",
                Busnummer = "AA",
                PostInfoId = PostInfoPuri + "101",

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
            var act = SetupController(new AddressProposeRequest
            {
                StraatNaamId = StraatNaamPuri + "123",
                Huisnummer = "11",
                Busnummer = "AA",
                PostInfoId = PostInfoPuri + "101",

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
            var act = SetupController(new AddressProposeRequest
            {
                StraatNaamId = StraatNaamPuri + "123",
                Huisnummer = "11",
                Busnummer = "AA",
                PostInfoId = PostInfoPuri + "101",

                PositieGeometrieMethode = PositieGeometrieMethode.AfgeleidVanObject,
                PositieSpecificatie = specificatie
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

        private Func<Task<IActionResult>> SetupController(AddressProposeRequest request)
        {
            var syndicationContext = new FakeSyndicationContextFactory().CreateDbContext();

            syndicationContext.PostalInfoLatestItems.Add(new PostalInfoLatestItem
            {
                NisCode = "validniscode",
                PostalCode = "101",
                PostalNames = new List<PostalInfoPostalName>
                {
                    new PostalInfoPostalName
                    {
                        PostalCode = "101",
                        Language = Taal.NL,
                        PostalName = "postalname"
                    }
                }
            });

            syndicationContext.SaveChanges();

            return async () => await _controller.Propose(
                ResponseOptions,
                new AddressProposeRequestValidator(syndicationContext),
                request,
                CancellationToken.None);
        }
    }
}
