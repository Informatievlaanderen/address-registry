namespace AddressRegistry.Tests.ApiTests.AddressMatch
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Oslo.AddressMatch.Requests;
    using Api.Oslo.AddressMatch.V1;
    using Api.Oslo.AddressMatch.V1.Matching;
    using Api.Oslo.Infrastructure.Options;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Microsoft.Extensions.Options;
    using Moq;
    using NetTopologySuite.Geometries;
    using Oslo_Legacy.Framework;
    using Oslo_Legacy.Framework.Assert;
    using Projections.Legacy.AddressDetail;
    using Projections.Syndication.Municipality;
    using Projections.Syndication.PostalInfo;
    using Projections.Syndication.StreetName;
    using Xunit;
    using Xunit.Abstractions;

    public class AddressMatchTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly AddressMatchContextMemory _addresMatchContext;
        private readonly Mock<ILatestQueries> _latestQueries;
        private readonly AddressMatchHandler _handler;

        public AddressMatchTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _addresMatchContext = new AddressMatchContextMemory();
            _latestQueries = new Mock<ILatestQueries>();
            _handler = new AddressMatchHandler(
                Mock.Of<IKadRrService>(),
                _latestQueries.Object,
                _addresMatchContext,
                new BuildingContextMemory(),
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
        public async Task CanFindGemeenteByGemeentenaamMatch_ExactMatch()
        {
            //Arrange
            var nisCode = "11001";
            var request = new AddressMatchRequest().WithGemeenteAndStraatnaam();
            request.Gemeentenaam = "Springfield";

            _latestQueries
                .Setup(x => x.GetAllLatestMunicipalities())
                .Returns(new[]
                {
                    new MunicipalityLatestItem
                    {
                        NisCode = nisCode,
                        NameDutch = request.Gemeentenaam,
                        NameDutchSearch = request.Gemeentenaam.RemoveDiacritics()
                    }
                });

            //Act
            var response = await _handler.Handle(request, CancellationToken.None);

            //Assert
            response.Should().NotBeNull();
            response.Should().HaveMatches(1);

            var firstMatch = response.AdresMatches.First();

            firstMatch.Should().HaveGemeente()
                .Which.Should().HaveGemeentenaam(request.Gemeentenaam)
                .And.HaveObjectId(nisCode);

            firstMatch.Should().NotHaveVolledigAdres();

            response.Should().ContainWarning("'Straatnaam' niet interpreteerbaar.");
        }

        [Theory]
        [InlineData("Springfeld", "Springfield")]
        [InlineData("Springfelt", "Springfield")]
        public async Task CanFindGemeenteByGemeentenaamMatch_FuzzyMatch(string requestedGemeentenaam,
            string existingGemeentenaam)
        {
            //Arrange
            var nisCode = "11001";
            var request = new AddressMatchRequest().WithGemeenteAndStraatnaam();
            request.Gemeentenaam = requestedGemeentenaam;

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
            response.Should().ContainWarning("Onbekende 'Gemeentenaam'.");
        }

        [Fact]
        public async Task CanFindGemeenteByGemeentenaamMatch_NoMatch()
        {
            //Arrange
            var nisCode = "11001";
            var request = new AddressMatchRequest().WithGemeenteAndStraatnaam();
            request.Gemeentenaam = "Sprungfelt";

            _latestQueries
                .Setup(x => x.GetAllLatestMunicipalities())
                .Returns(new[]
                {
                    new MunicipalityLatestItem
                    {
                        NisCode = nisCode,
                        NameDutch = "Springfield",
                        NameDutchSearch = "Springfield".RemoveDiacritics()
                    }
                });

            //Act
            var response = await _handler.Handle(request, CancellationToken.None);

            //Assert
            response.Should().NotBeNull();
            response.Should().HaveMatches(0);
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
            var existingNisCode = "11001";
            var existingGemeentenaam = "Springfield";
            var requestedGemeentenaam = "Deelgemeente";

            //Arrange
            var request = new AddressMatchRequest().WithGemeenteAndStraatnaam();
            request.Gemeentenaam = requestedGemeentenaam;

            MockGetAllPostalInfo(existingNisCode, "9000", requestedGemeentenaam);
            MockGetAllLatestMunicipalities(existingNisCode, existingGemeentenaam);

            //Act
            var response = await _handler.Handle(request, CancellationToken.None);

            //Assert
            response.Should().NotBeNull();
            response.Should().HaveMatches(1);

            var firstMatch = response.AdresMatches.First();

            firstMatch.Should().HaveGemeente()
                .Which.Should().HaveGemeentenaam(existingGemeentenaam)
                .And.HaveObjectId(existingNisCode);

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

            MockGetAllLatestMunicipalities(new []{ "11001", "11002" }, request.Gemeentenaam);

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
        [InlineData("Onze Lieve Vrouw-Evergreen Terrace", "O.l.v.-Evergreen Terrace")] // replace abreviations in existing straatnaam
        [InlineData("Clevergreen Terrace Avenue", "Evergreen")] // containment of existing straatnaam
        [InlineData("Evergreen", "Clevergreen Terrace Avenue")] // containment of input
        [InlineData("Trammesantlei", "Evergreen Terrace", false)] // no match
        public async Task CanFindStraatnaamByStraatnaamMatch(string requestedStraatnaam, string existingStraatnaam, bool isMatch = true)
        {
            var existingNisCode = "11001";
            var existingStraatnaamId = "1";
            var existingGemeentenaam = "Springfield";

            //Arrange
            var request = new AddressMatchRequest().WithGemeenteAndStraatnaam();
            request.Straatnaam = requestedStraatnaam;

            MockGetAllLatestMunicipalities(existingNisCode, existingGemeentenaam);
            MockStreetNames(existingStraatnaam, existingNisCode, existingStraatnaamId, existingGemeentenaam);

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
                    .And.HaveObjectId(existingStraatnaamId);

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

        // [Fact]
        // public async Task CanFindStraatnaamByKadStraatCode()
        // {
        //     var existingNisCode = Generate.NisCode.Generate(Random);
        //     int existingStraatnaamId = Generate.VbrObjectIdInt.Generate(Random);
        //     var existingGemeentenaam = "Springfield";
        //     var existingStraatnaam = "Evergreen Terrace";
        //     var requestededKadStraatCode = "6789";
        //
        //     //Arrange
        //     var request = new AddressMatchRequest().WithGemeenteAndKadStraatcode();
        //     request.KadStraatcode = requestededKadStraatCode;
        //
        //     Latest.ArrangeLatestGemeente(existingNisCode, existingGemeentenaam);
        //     KadRrService.ArrangeKadStraatnamen(existingNisCode, existingGemeentenaam, existingStraatnaamId, existingStraatnaam, requestededKadStraatCode);
        //
        //     //Act
        //     var response = await Send(request);
        //
        //     //Assert
        //     response.Should().NotBeNull();
        //     response.Should().HaveMatches(1);
        //
        //     var firstMatch = response.AdresMatches.First();
        //
        //     firstMatch.Should().HaveGemeente()
        //         .Which.Should().HaveGemeentenaam(existingGemeentenaam)
        //         .And.HaveObjectId(existingNisCode);
        //
        //     firstMatch.Should().HaveStraatnaam()
        //         .Which.Should().HaveStraatnaam(existingStraatnaam)
        //         .And.HaveObjectId(existingStraatnaamId.ToString());
        //
        //     firstMatch.Should().NotHaveVolledigAdres();
        // }

        // [Fact]
        // public async Task CanFindStraatnaamByRrStraatCode()
        // {
        //     var existingNisCode = Generate.NisCode.Generate(Random);
        //     var existingStraatnaamId = Generate.VbrObjectIdInt.Generate(Random);
        //     var existingGemeentenaam = "Springfield";
        //     var existingStraatnaam = "Evergreen Terrace";
        //     var requestededRrStraatCode = "987";
        //
        //     //Arrange
        //     var request = new AddressMatchRequest().WithPostcodeAndRrStraatcode();
        //     request.RrStraatcode = requestededRrStraatCode;
        //
        //     Latest.ArrangeLatestGemeente(existingNisCode, existingGemeentenaam);
        //     KadRrService.ArrangeRrStraatnaam(request.Postcode, existingNisCode, existingGemeentenaam, existingStraatnaamId, existingStraatnaam, requestededRrStraatCode);
        //     Latest.ArrangeLatestPostInfo(existingNisCode, request.Postcode);
        //
        //     //Act
        //     var response = await Send(request);
        //
        //     //Assert
        //     response.Should().NotBeNull();
        //     response.Should().HaveMatches(1);
        //
        //     var firstMatch = response.AdresMatches.First();
        //
        //     firstMatch.Should().HaveGemeente()
        //         .Which.Should().HaveGemeentenaam(existingGemeentenaam)
        //         .And.HaveObjectId(existingNisCode);
        //
        //     firstMatch.Should().HaveStraatnaam()
        //         .Which.Should().HaveStraatnaam(existingStraatnaam)
        //         .And.HaveObjectId(existingStraatnaamId.ToString());
        //
        //     firstMatch.Should().NotHaveVolledigAdres();
        // }

        // [Fact]
        // public async Task CanFindRrAdres()
        // {
        //     //Arrange
        //     var existingNisCode = Generate.NisCode.Generate(Random);
        //     var existingGemeentenaam = "Springfield";
        //     var existingStraatnaamId = Generate.VbrObjectIdInt.Generate(Random);
        //     var existingStraatnaam = "Evergreen Terrace";
        //     var streetNameId = Guid.NewGuid();
        //
        //     var request = new AddressMatchRequest().WithPostcodeAndRrStraatcode();
        //     request.Huisnummer = "15";
        //
        //     var gemeente = Latest.ArrangeLatestGemeente(existingNisCode, existingGemeentenaam).First();
        //     var straat = Latest.ArrangeLatestStraatnaam(existingNisCode, existingGemeentenaam, existingStraatnaamId.ToString(), existingStraatnaam, streetNameId).First();
        //
        //     var mappedAdressen = KadRrService
        //         .Arrange(Produce.Exactly(6, Generate.TblHuisNummer.Select(x => x.WithStraatNaamId(streetNameId))),
        //             (when, x) => when.AdresMappingExistsFor(x, request.Huisnummer, request.Index, request.RrStraatcode, request.Postcode))
        //         .OrderBy(x => new VolledigAdres(existingStraatnaam, x.HouseNumber, x.BoxNumber, x.PostalCode, existingGemeentenaam, Taal.NL).GeografischeNaam.Spelling)
        //         .ToList();
        //
        //     //Act
        //     var response = await Send(request);
        //
        //     //Assert
        //     response.Should().NotBeNull();
        //     response.Should().HaveMatches(Math.Min(mappedAdressen.Count, 10));
        //
        //     var firstMatch = response.AdresMatches.OrderBy(x => x.Score).ThenBy(x => x.VolledigAdres?.GeografischeNaam?.Spelling).First();
        //
        //     firstMatch.Should().HaveGemeente()
        //         .Which.Should().HaveGemeentenaam(existingGemeentenaam)
        //         .And.HaveObjectId(existingNisCode);
        //
        //     firstMatch.Should().HaveStraatnaam()
        //         .Which.Should().HaveStraatnaam(existingStraatnaam)
        //         .And.HaveObjectId(existingStraatnaamId.ToString());
        //
        //     firstMatch.Should().HaveVolledigAdres()
        //         .Which.Should().HaveGeografischeNaam($"{existingStraatnaam} {mappedAdressen.First().HouseNumber} bus {mappedAdressen.First().BoxNumber}, {mappedAdressen.First().PostalCode} {existingGemeentenaam}");
        // }

        [Fact]
        public async Task CanFindAdresMatch()
        {
            var existingNisCode = "11001";
            var existingStraatnaamId = "1";
            var existingGemeentenaam = "Springfield";
            var postcode = "9000";

            //Arrange
            var request = new AddressMatchRequest().WithGemeenteAndStraatnaam();
            request.Huisnummer = "742";

            MockGetAllLatestMunicipalities(existingNisCode, existingGemeentenaam);
            var existingStraatnaam = MockStreetNames(request.Straatnaam, existingNisCode, existingStraatnaamId, existingGemeentenaam);
            MockGetLatestAddressesBy(existingStraatnaam.StreetNameId, existingStraatnaamId, postcode, request.Huisnummer);

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
                .And.HaveObjectId(existingStraatnaamId);

            firstMatch.Should().HaveVolledigAdres()
                .Which.Should().HaveGeografischeNaam($"{existingStraatnaam.NameDutch} {request.Huisnummer}, {postcode} {existingGemeentenaam}");
        }

        [Fact]
        public async Task AdresMatchWithBusnummerSkipsSanitization()
        {
            var existingNisCode = "11001";
            var existingStraatnaamId = "1";
            var existingGemeentenaam = "Springfield";
            var postcode = "9000";

            //Arrange
            var request = new AddressMatchRequest().WithGemeenteAndStraatnaam();
            request.Postcode = postcode;
            request.Huisnummer = "742";
            request.Busnummer = "C2";

            MockGetAllLatestMunicipalities(existingNisCode, existingGemeentenaam);
            var existingStraatnaam = MockStreetNames(request.Straatnaam, existingNisCode, existingStraatnaamId, existingGemeentenaam);
            MockGetLatestAddressesBy(existingStraatnaam.StreetNameId, existingStraatnaamId, postcode, request.Huisnummer, request.Busnummer);

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
                .And.HaveObjectId(existingStraatnaamId);

            firstMatch.Should().HaveVolledigAdres()
                .Which.Should().HaveGeografischeNaam($"{existingStraatnaam.NameDutch} {request.Huisnummer} bus {request.Busnummer}, {postcode} {existingGemeentenaam}");

            firstMatch.Should().HaveScore(100);
        }

        private MunicipalityLatestItem MockGetAllLatestMunicipalities(string nisCode, string municipalityName)
        {
            var municipalityLatestItem = new MunicipalityLatestItem
            {
                NisCode = nisCode,
                NameDutch = municipalityName,
                NameDutchSearch = municipalityName.RemoveDiacritics()
            };

            _latestQueries
                .Setup(x => x.GetAllLatestMunicipalities())
                .Returns(new[] { municipalityLatestItem });

            return municipalityLatestItem;
        }

        private void MockGetAllLatestMunicipalities(IEnumerable<string> nisCodes, string municipalityName)
        {
            _latestQueries
                .Setup(x => x.GetAllLatestMunicipalities())
                .Returns(
                    nisCodes.Select(x =>
                        new MunicipalityLatestItem
                        {
                            NisCode = x,
                            NameDutch = municipalityName,
                            NameDutchSearch = municipalityName.RemoveDiacritics()
                        })
                );
        }

        private PostalInfoLatestItem MockGetAllPostalInfo(string nisCode, string postalCode, string? postalName = null)
        {
            var postalInfoLatestItem = new PostalInfoLatestItem
            {
                NisCode = nisCode,
                PostalCode = postalCode,
                PostalNames = new List<PostalInfoPostalName>()
            };

            if (!string.IsNullOrWhiteSpace(postalName))
            {
                postalInfoLatestItem.PostalNames.Add(new PostalInfoPostalName{ PostalCode = postalCode, PostalName = postalName, Language = Taal.NL });
            }

            _latestQueries
                .Setup(x => x.GetAllPostalInfo())
                .Returns(new[] { postalInfoLatestItem });

            return postalInfoLatestItem;
        }

        private StreetNameLatestItem MockStreetNames(string streetName, string nisCode, string streetNameId, string municipalityName)
        {
            var streetNameLatestItems = new[]
            {
                new StreetNameLatestItem
                {
                    NisCode = nisCode,
                    NameDutch = streetName,
                    NameDutchSearch = streetName.RemoveDiacritics(),
                    PersistentLocalId = streetNameId,
                    StreetNameId = Guid.NewGuid()
                }
            };
            _latestQueries
                .Setup(x => x.GetAllLatestStreetNames())
                .Returns(streetNameLatestItems);
            _latestQueries
                .Setup(x => x.GetLatestStreetNamesBy(municipalityName))
                .Returns(streetNameLatestItems);
            _latestQueries
                .Setup(x => x.FindLatestStreetNameById(streetNameId))
                .Returns(streetNameLatestItems.Single);

            return streetNameLatestItems.Single();
        }

        private void MockGetLatestAddressesBy(Guid streetNameId, string streetNamePersistentLocalId, string postalCode, string houseNumber, string? boxNumber = null)
        {
            _latestQueries
                .Setup(x => x.GetLatestAddressesBy(streetNamePersistentLocalId, houseNumber, null))
                .Returns(new[]
                {
                    new AddressDetailItem
                    {
                        PersistentLocalId = 2,
                        PostalCode = postalCode,
                        StreetNameId = streetNameId,
                        HouseNumber = houseNumber,
                        BoxNumber = boxNumber,
                        Position = new Point(120, 45).AsBinary(),
                    }
                });
        }
    }
}
