namespace AddressRegistry.Tests.ApiTests.AddressMatch
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Oslo.AddressMatch.Requests;
    using Api.Oslo.AddressMatch.V2;
    using Api.Oslo.AddressMatch.V2.Matching;
    using Api.Oslo.Infrastructure.Options;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Consumer.Read.Municipality.Projections;
    using Consumer.Read.StreetName.Projections;
    using Microsoft.Extensions.Options;
    using Moq;
    using NetTopologySuite.Geometries;
    using NodaTime;
    using Oslo_Legacy.Framework;
    using Oslo_Legacy.Framework.Assert;
    using Projections.Legacy.AddressDetailV2;
    using Projections.Syndication.PostalInfo;
    using StreetName;
    using Xunit;
    using Xunit.Abstractions;

    public class AddressMatchV2Tests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly AddressMatchContextMemoryV2 _addresMatchContext;
        private readonly Mock<ILatestQueries> _latestQueries;
        private readonly AddressMatchHandlerV2 _handler;

        public AddressMatchV2Tests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _addresMatchContext = new AddressMatchContextMemoryV2();
            _latestQueries = new Mock<ILatestQueries>();
            _handler = new AddressMatchHandlerV2(_latestQueries.Object, _addresMatchContext,
                new OptionsWrapper<ResponseOptions>(new ResponseOptions
                {
                    DetailUrl = "detail/{0}",
                    GemeenteDetailUrl = "gemeentedetail/{0}",
                    StraatnaamDetailUrl = "straatnaamdetail/{0}",
                    PostInfoDetailUrl = "postinfodetail/{0}",
                    MaxStreetNamesThreshold = 100,
                    SimilarityThreshold = 75.0
                }));
        }

        [Fact]
        public async Task CanFindGemeenteByGemeentenaam_ExactMatch()
        {
            //Arrange
            var nisCode = "11001";
            var gemeentenaam = "Springfield";

            var request = new AddressMatchRequest().WithGemeenteAndStraatnaam();
            request.Gemeentenaam = gemeentenaam;

            MockGetAllLatestMunicipalities(nisCode, gemeentenaam);

            //Act
            var response = await _handler.Handle(request, CancellationToken.None);

            //Assert
            response.Should().NotBeNull();
            response.Should().HaveMatches(1);

            var firstMatch = response.AdresMatches.First();
            firstMatch.Should().HaveGemeente()
                .Which.Should().HaveGemeentenaam(gemeentenaam)
                .And.HaveObjectId(nisCode);
            firstMatch.Should().NotHaveVolledigAdres();

            response.Should().ContainWarning("'Straatnaam' niet interpreteerbaar.");
        }

        [Theory]
        [InlineData("Springfeld", "Springfield")]
        [InlineData("Springfelt", "Springfield")]
        public async Task CanFindGemeenteByGemeentenaam_FuzzyMatch(string requestedGemeentenaam,
            string existingGemeentenaam)
        {
            //Arrange
            var nisCode = "11001";

            var request = new AddressMatchRequest().WithGemeenteAndStraatnaam();
            request.Gemeentenaam = requestedGemeentenaam;

            _latestQueries
                .Setup(x => x.GetAllLatestMunicipalities())
                .Returns(new[]
                {
                    new MunicipalityLatestItem(Guid.NewGuid(), nisCode, SystemClock.Instance.GetCurrentInstant())
                    {
                        NameDutch = existingGemeentenaam,
                        NameDutchSearch = existingGemeentenaam.RemoveDiacritics(),
                        OfficialLanguages = new List<string> { "Dutch" }
                    }
                });

            //Act
            var response = await _handler.Handle(request, CancellationToken.None);

            //Assert
            response.Should().NotBeNull();
            response.Should().HaveMatches(1);

            var firstMatch = response.AdresMatches.First();
            firstMatch.Should().HaveGemeente()
                .Which.Should().HaveGemeentenaam(existingGemeentenaam)
                .And.HaveObjectId(nisCode);
            firstMatch.Should().NotHaveVolledigAdres();

            response.Should().ContainWarning("'Straatnaam' niet interpreteerbaar.");
            response.Should().ContainWarning("Onbekende 'Gemeentenaam'.");
        }

        [Fact]
        public async Task CanFindGemeenteByGemeentenaam_NoMatch()
        {
            //Arrange
            var nisCode = "11001";

            var request = new AddressMatchRequest().WithGemeenteAndStraatnaam();
            request.Gemeentenaam = "Sprungfelt";

            _latestQueries
                .Setup(x => x.GetAllLatestMunicipalities())
                .Returns(new[]
                {
                    new MunicipalityLatestItem(Guid.NewGuid(), nisCode, SystemClock.Instance.GetCurrentInstant())
                    {
                        NameDutch = "Springfield",
                        NameDutchSearch = "Springfield".RemoveDiacritics(),
                        OfficialLanguages = new List<string> { "Dutch" }
                    }
                });

            //Act
            var response = await _handler.Handle(request, CancellationToken.None);

            //Assert
            response.Should().NotBeNull();
            response.Should().HaveMatches(0);
        }

        private MunicipalityLatestItem MockGetAllLatestMunicipalities(string nisCode, string gemeentenaam)
        {
            var municipalityLatestItem =
                new MunicipalityLatestItem(Guid.NewGuid(), nisCode, SystemClock.Instance.GetCurrentInstant())
                {
                    NameDutch = gemeentenaam,
                    NameDutchSearch = gemeentenaam.RemoveDiacritics(),
                    OfficialLanguages = new List<string> { "Dutch" }
                };

            _latestQueries
                .Setup(x => x.GetAllLatestMunicipalities())
                .Returns(new[]
                {
                    municipalityLatestItem
                });

            return municipalityLatestItem;
        }

        private void MockGetAllLatestMunicipalities(IEnumerable<string> nisCodes, string gemeentenaam)
        {
            _latestQueries
                .Setup(x => x.GetAllLatestMunicipalities())
                .Returns(
                    nisCodes.Select(x =>
                        new MunicipalityLatestItem(Guid.NewGuid(), x, SystemClock.Instance.GetCurrentInstant())
                        {
                            NameDutch = gemeentenaam,
                            NameDutchSearch = gemeentenaam.RemoveDiacritics(),
                            OfficialLanguages = new List<string> { "Dutch" }
                        })
                );
        }

        private void MockGetAllPostalInfo(string nisCode, string postcode, string postnaam = "")
        {
            var postalInfoLatestItem = new PostalInfoLatestItem { NisCode = nisCode, PostalCode = postcode, };

            if (!string.IsNullOrWhiteSpace(postnaam))
            {
                postalInfoLatestItem.PostalNames = new List<PostalInfoPostalName>
                {
                    new PostalInfoPostalName
                    {
                        Language = Taal.NL, PostalCode = postcode, PostalName = postnaam
                    }
                };
            }

            _latestQueries
                .Setup(x => x.GetAllPostalInfo())
                .Returns(new[]
                {
                    postalInfoLatestItem
                });
        }

        [Fact]
        public async Task CanFindGemeenteByPostcode()
        {
            var nisCode = "11001";
            var gemeentenaam = "Springfield";
            var postcode = "4900";

            //Arrange
            var request = new AddressMatchRequest().WithPostcodeAndStraatnaam();
            request.Postcode = postcode;

            MockGetAllPostalInfo(nisCode, postcode);
            MockGetAllLatestMunicipalities(nisCode, gemeentenaam);

            //Act
            var response = await _handler.Handle(request, CancellationToken.None);

            //Assert
            response.Should().NotBeNull();
            response.Should().HaveMatches(1);

            var firstMatch = response.AdresMatches.First();

            firstMatch.Should().HaveGemeente()
                .Which.Should().HaveGemeentenaam(gemeentenaam)
                .And.HaveObjectId(nisCode);

            firstMatch.Should().NotHaveVolledigAdres();

            response.Should().ContainWarning("'Straatnaam' niet interpreteerbaar.");
        }

        [Fact]
        public async Task CanFindGemeenteByDeelgemeente()
        {
            var nisCode = "11001";
            var existingGemeentenaam = "Springfield";
            var requestedGemeentenaam = "Deelgemeente";

            //Arrange
            var request = new AddressMatchRequest().WithGemeenteAndStraatnaam();
            request.Gemeentenaam = requestedGemeentenaam;

            MockGetAllPostalInfo(nisCode, "9000", requestedGemeentenaam);
            MockGetAllLatestMunicipalities(nisCode, existingGemeentenaam);

            //Act
            var response = await _handler.Handle(request, CancellationToken.None);

            //Assert
            response.Should().NotBeNull();
            response.Should().HaveMatches(1);

            var firstMatch = response.AdresMatches.First();

            firstMatch.Should().HaveGemeente()
                .Which.Should().HaveGemeentenaam(existingGemeentenaam)
                .And.HaveObjectId(nisCode);

            firstMatch.Should().NotHaveVolledigAdres();

            response.Should().ContainWarning("'Straatnaam' niet interpreteerbaar.");
        }

        [Fact]
        public async Task CanFindGemeenteByNiscode()
        {
            //Arrange
            var request = new AddressMatchRequest().WithGemeenteAndNisCodeAndStraatnaam();

            MockGetAllLatestMunicipalities(request.Niscode, request.Gemeentenaam);

            //Act
            var response = await _handler.Handle(request, CancellationToken.None);

            //Assert
            response.Should().NotBeNull();
            response.Should().HaveMatches(1);

            var firstMatch = response.AdresMatches.First();

            firstMatch.Should().HaveGemeente()
                .Which.Should().HaveGemeentenaam(request.Gemeentenaam)
                .And.HaveObjectId(request.Niscode);

            firstMatch.Should().NotHaveVolledigAdres();

            response.Should().ContainWarning("'Straatnaam' niet interpreteerbaar.");
        }

        [Fact]
        public async Task CanFindFusieGemeenteByNiscode()
        {
            //Arrange
            var request = new AddressMatchRequest().WithGemeenteAndNisCodeAndStraatnaam();

            MockGetAllLatestMunicipalities(new[] { "11001", "11002" }, request.Gemeentenaam);

            //Act
            var response = await _handler.Handle(request, CancellationToken.None);

            //Assert
            response.Should().NotBeNull();
            response.Should().HaveMatches(2);

            var firstMatch = response.AdresMatches.First();

            firstMatch.Should().HaveGemeente()
                .Which.Should().HaveGemeentenaam(request.Gemeentenaam);

            firstMatch.Should().NotHaveVolledigAdres();

            var secondMatch = response.AdresMatches.First();

            secondMatch.Should().HaveGemeente()
                .Which.Should().HaveGemeentenaam(request.Gemeentenaam);

            secondMatch.Should().NotHaveVolledigAdres();

            response.Should().ContainWarning("'Straatnaam' niet interpreteerbaar.");
            response.Should().NotContainWarning("Geen overeenkomst tussen 'Niscode' en 'Gemeentenaam'.");
        }

        [Theory]
        [InlineData("Evergreen Terrace", "Evergreen Terrace")] // exact match
        [InlineData("Evergreen Terras", "Evergreen Terrace")] // fuzzy match
        [InlineData("evergreen terras", "Evergreen Terrace")] // case insensitive
        [InlineData("St-Evergreen Terrace", "Sint-Evergreen Terrace")] // replace abreviations in input
        [InlineData("Onze Lieve Vrouw-Evergreen Terrace",
            "O.l.v.-Evergreen Terrace")] // replace abreviations in existing straatnaam
        [InlineData("Clevergreen Terrace Avenue", "Evergreen")] // containment of existing straatnaam
        [InlineData("Evergreen", "Clevergreen Terrace Avenue")] // containment of input
        [InlineData("Trammesantlei", "Evergreen Terrace", false)] // no match
        public async Task CanFindStraatnaamByStraatnaamMatch(string requestedStraatnaam, string existingStraatnaam,
            bool isMatch = true)
        {
            var existingNisCode = "11001";
            var existingStraatnaamId = 1;
            var existingGemeentenaam = "Springfield";

            //Arrange
            var request = new AddressMatchRequest().WithGemeenteAndStraatnaam();
            request.Straatnaam = requestedStraatnaam;

            MockGetAllLatestMunicipalities(existingNisCode, existingGemeentenaam);
            MockStreetNames(existingStraatnaam, existingStraatnaamId, existingNisCode, existingGemeentenaam);

            //Act
            var response = await _handler.Handle(request, CancellationToken.None);

            //Assert
            response.Should().NotBeNull();

            if (isMatch)
            {
                response.Should().HaveMatches(1);

                var firstMatch = response.AdresMatches.First();

                firstMatch.Should().HaveGemeente()
                    .Which.Should().HaveGemeentenaam(existingGemeentenaam)
                    .And.HaveObjectId(existingNisCode);

                firstMatch.Should().HaveStraatnaam()
                    .Which.Should().HaveStraatnaam(existingStraatnaam)
                    .And.HaveObjectId(existingStraatnaamId.ToString());

                firstMatch.Should().NotHaveVolledigAdres();
            }
            else
            {
                response.Should().HaveMatches(1);


                var firstMatch = response.AdresMatches.First();
                firstMatch.Should().HaveGemeente()
                    .Which.Should().HaveGemeentenaam(existingGemeentenaam)
                    .And.HaveObjectId(existingNisCode);

                firstMatch.Should().HaveNoStraatnaam();

                response.Should().ContainWarning("'Straatnaam' niet interpreteerbaar.");
            }
        }

        private StreetNameLatestItem MockStreetNames(string existingStraatnaam, int existingStraatnaamId,
            string existingNisCode,
            string existingGemeentenaam)
        {
            var streetNameLatestItem = new StreetNameLatestItem(existingStraatnaamId, existingNisCode)
            {
                NameDutch = existingStraatnaam,
                NameDutchSearch = existingStraatnaam.RemoveDiacritics()
            };

            _latestQueries
                .Setup(x => x.GetAllLatestStreetNames())
                .Returns(new[]
                {
                    streetNameLatestItem
                });
            _latestQueries
                .Setup(x => x.GetLatestStreetNamesBy(existingGemeentenaam))
                .Returns(new[]
                {
                    streetNameLatestItem
                });
            _latestQueries
                .Setup(x => x.FindLatestStreetNameById(existingStraatnaamId))
                .Returns(
                    streetNameLatestItem);

            return streetNameLatestItem;
        }

        [Fact]
        public async Task CanFindAdresMatch()
        {
            var existingNisCode = "11001";
            var existingStraatnaamId = 1;
            var existingGemeentenaam = "Springfield";
            var postcode = "9000";

            //Arrange
            var request = new AddressMatchRequest().WithGemeenteAndStraatnaam();
            request.Huisnummer = "742";

            MockGetAllLatestMunicipalities(existingNisCode, existingGemeentenaam);
            var existingStraatnaam = MockStreetNames(request.Straatnaam, existingStraatnaamId, existingNisCode, existingGemeentenaam);
            MockGetLatestAddressesBy(existingStraatnaamId, postcode, request.Huisnummer);

            //Act
            var response = await _handler.Handle(request, CancellationToken.None);

            //Assert
            response.Should().NotBeNull();
            response.Should().HaveMatches(1);

            var firstMatch = response.AdresMatches.First();

            firstMatch.Should().HaveGemeente()
                .Which.Should().HaveGemeentenaam(existingGemeentenaam)
                .And.HaveObjectId(existingNisCode);

            firstMatch.Should().HaveStraatnaam()
                .Which.Should().HaveStraatnaam(request.Straatnaam)
                .And.HaveObjectId(existingStraatnaamId.ToString());

            firstMatch.Should().HaveVolledigAdres()
                .Which.Should()
                .HaveGeografischeNaam(
                    $"{existingStraatnaam.NameDutch} {request.Huisnummer}, {postcode} {existingGemeentenaam}");
        }

        private void MockGetLatestAddressesBy(int existingStraatnaamId, string postcode, string huisnummer, string? busnummer = null)
        {
            _latestQueries
                .Setup(x => x.GetLatestAddressesBy(existingStraatnaamId, huisnummer, busnummer))
                .Returns(new[]
                {
                    new AddressDetailItemV2(2, existingStraatnaamId, postcode, huisnummer, busnummer,
                        AddressStatus.Current, true, new Point(120, 45).AsBinary(), GeometryMethod.DerivedFromObject,
                        GeometrySpecification.Entry, false, SystemClock.Instance.GetCurrentInstant())
                });
        }

        [Fact]
        public async Task AdresMatchWithBusnummerSkipsSanitization()
        {
            var existingNisCode = "11001";
            var existingStraatnaamId = 1;
            var existingGemeentenaam = "Springfield";
            var postcode = "9000";

            //Arrange
            var request = new AddressMatchRequest().WithGemeenteAndStraatnaam();
            request.Postcode = postcode;
            request.Huisnummer = "742";
            request.Busnummer = "C2";

            MockGetAllLatestMunicipalities(existingNisCode, existingGemeentenaam);
            var existingStraatnaam = MockStreetNames(request.Straatnaam, existingStraatnaamId, existingNisCode, existingGemeentenaam);
            MockGetLatestAddressesBy(existingStraatnaamId, postcode, request.Huisnummer, request.Busnummer);

            //Act
            var response = await _handler.Handle(request, CancellationToken.None);

            //Assert
            response.Should().NotBeNull();
            response.Should().HaveMatches(1);

            var firstMatch = response.AdresMatches.First();

            firstMatch.Should().HaveGemeente()
                .Which.Should().HaveGemeentenaam(existingGemeentenaam)
                .And.HaveObjectId(existingNisCode);

            firstMatch.Should().HaveStraatnaam()
                .Which.Should().HaveStraatnaam(request.Straatnaam)
                .And.HaveObjectId(existingStraatnaamId.ToString());

            firstMatch.Should().HaveVolledigAdres()
                .Which.Should().HaveGeografischeNaam($"{existingStraatnaam.NameDutch} {request.Huisnummer} bus {request.Busnummer}, {postcode} {existingGemeentenaam}");

            firstMatch.Should().HaveScore(100);
        }
    }
}
