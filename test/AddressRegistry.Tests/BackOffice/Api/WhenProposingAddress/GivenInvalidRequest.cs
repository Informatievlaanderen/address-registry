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
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using Xunit;
    using Xunit.Abstractions;
    using PositieGeometrieMethode = Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts.PositieGeometrieMethode;
    using PositieSpecificatie = Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts.PositieSpecificatie;

    public class GivenInvalidRequest : BackOfficeApiTest
    {
        private readonly AddressController _controller;
        private readonly Mock<IStreamStore> _streamStore;

        public GivenInvalidRequest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _streamStore = new Mock<IStreamStore>();

            MockMediator
                .Setup(x => x.Send(It.IsAny<AddressProposeRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(new PersistentLocalIdETagResponse(123, "lasteventhash")));

            _controller = CreateApiBusControllerWithUser<AddressController>();
        }

        [Fact]
        public void WithInvalidStreetName_ThenThrowsValidationException()
        {
            WithStreamExists();

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
        public void WithUnexistingStreetName_ThenThrowsValidationException()
        {
            WithStreamDoesNotExist();

            var straatNaamId = StraatNaamPuri + "123";

            var act = SetupController(new AddressProposeRequest
            {
                StraatNaamId = straatNaamId,
                Huisnummer = "11"
            });

            // Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                    x.Errors.Any(e =>
                        e.ErrorCode == "AdresStraatnaamNietGekendValidatie"
                        && e.ErrorMessage == $"De straatnaam '{straatNaamId}' is niet gekend in het straatnaamregister."));
        }

        [Fact]
        public void WithNonExistentPostInfo_ThenThrowsValidationException()
        {
            WithStreamExists();

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
            WithStreamExists();

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
            WithStreamExists();

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
            WithStreamExists();

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
        public void WithInvalidSpecificationAndMethodAppointedByAdmin_ThenThrowsValidationException()
        {
            WithStreamExists();

            var act = SetupController(new AddressProposeRequest
            {
                StraatNaamId = StraatNaamPuri + "123",
                Huisnummer = "11",
                Busnummer = "AA",
                PostInfoId = PostInfoPuri + "101",

                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                PositieSpecificatie = PositieSpecificatie.Gemeente,
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

        [Theory]
        [InlineData(PositieSpecificatie.Perceel)]
        [InlineData(PositieSpecificatie.Lot)]
        [InlineData(PositieSpecificatie.Standplaats)]
        [InlineData(PositieSpecificatie.Ligplaats)]
        [InlineData(PositieSpecificatie.Ingang)]
        [InlineData(PositieSpecificatie.Gebouweenheid)]
        public void WithInvalidSpecificationAndDerivedFromObject_ThenThrowsValidationException(PositieSpecificatie specificatie)
        {
            WithStreamExists();

            var act = SetupController(new AddressProposeRequest
            {
                StraatNaamId = StraatNaamPuri + "123",
                Huisnummer = "11",
                Busnummer = "AA",
                PostInfoId = PostInfoPuri + "101",

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
            WithStreamExists();

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
                    x.Errors.Any(e => e.ErrorCode == "AdresPositieVerplicht"
                                      && e.ErrorMessage == "De positie is verplicht."));
        }

        [Fact]
        public void WithInvalidGml_ThenThrowsValidationException()
        {
            WithStreamExists();

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
                new AddressProposeRequestValidator(
                    new StreetNameExistsValidator(_streamStore.Object),
                    syndicationContext),
                request,
                CancellationToken.None);
        }

        private void WithStreamExists()
        {
            _streamStore
                .Setup(store => store.ReadStreamBackwards(It.IsAny<StreamId>(), StreamVersion.End, 1, false, CancellationToken.None))
                .ReturnsAsync(() => new ReadStreamPage("id", PageReadStatus.Success, 1, 2, 2, 2, ReadDirection.Backward, false, messages: new []{ new StreamMessage() }));
        }

        private void WithStreamDoesNotExist()
        {
            _streamStore
                .Setup(store => store.ReadStreamBackwards(It.IsAny<StreamId>(), StreamVersion.End, 1, false, CancellationToken.None))
                .ReturnsAsync(() => new ReadStreamPage("id", PageReadStatus.StreamNotFound, -1, -1, -1, -1, ReadDirection.Backward, false, messages: Array.Empty<StreamMessage>()));
        }
    }
}
