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
    using Asserts;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Consumer.Read.Municipality.Projections;
    using Consumer.Read.Postal.Projections;
    using Consumer.Read.StreetName.Projections;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Moq;
    using NetTopologySuite.Geometries;
    using NodaTime;
    using Projections.AddressMatch.AddressDetailV2WithParent;
    using StreetName;
    using Xunit;
    using Xunit.Abstractions;

    public class AddressMatchV2Tests : AddressMatchTestBase
    {
        private readonly Mock<ILatestQueries> _latestQueries;
        private readonly AddressMatchHandlerV2 _handler;

        public AddressMatchV2Tests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _latestQueries = new Mock<ILatestQueries>();
            _handler = new AddressMatchHandlerV2(_latestQueries.Object,
                new OptionsWrapper<ResponseOptions>(new ResponseOptions
                {
                    DetailUrl = "detail/{0}",
                    GemeenteDetailUrl = "gemeentedetail/{0}",
                    StraatnaamDetailUrl = "straatnaamdetail/{0}",
                    PostInfoDetailUrl = "postinfodetail/{0}",
                    MaxStreetNamesThreshold = 100,
                    SimilarityThreshold = 75.0,
                    AddressMatchBuildingUnitLink = "http://gebouweenheden?adresobjectid={0}",
                    AddressMatchParcelLink = "http://percelen?adresobjectid={0}"
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
        public async Task CanFindGemeenteByGemeentenaam_FuzzyMatch(string requestedGemeentenaam, string existingGemeentenaam)
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
                }.ToDictionary(x => x.NisCode));

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
                }.ToDictionary(x => x.NisCode));

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
        [InlineData("Onze Lieve Vrouw-Evergreen Terrace", "O.l.v.-Evergreen Terrace")] // replace abreviations in existing straatnaam
        [InlineData("onze-lieve-vrouwemarkt", "O.-L.-Vrouwemarkt")] // replace abreviations in existing straatnaam
        [InlineData("onze-lieve-vrouwstraat", "O.-L.-Vrouwstraat")] // replace abreviations in existing straatnaam
        [InlineData("Heilig Hartlaan", "H.-Hartlaan")] // replace abreviations in existing straatnaam
        [InlineData("Heilige Hartlaan", "H.-Hartlaan")] // replace abreviations in existing straatnaam
        [InlineData("k.elisabethlaan", "Koningin Elisabethlaan")] // replace abreviations in existing straatnaam
        [InlineData("Clevergreen Terrace Avenue", "Evergreen")] // containment of existing straatnaam
        [InlineData("Evergreen", "Clevergreen Terrace Avenue")] // containment of input
        [InlineData("Trammesantlei", "Evergreen Terrace", false)] // no match
        public async Task CanFindStraatnaamByStraatnaamMatch(string requestedStraatnaam, string existingStraatnaam, bool isMatch = true)
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
                .HaveGeografischeNaam($"{existingStraatnaam.NameDutch} {request.Huisnummer}, {postcode} {existingGemeentenaam}");
        }

        [Fact]
        public async Task CanFindAdresMatchButNotWithStreetName()
        {
            var existingNisCode = "11001";
            var existingStraatnaamId = 1;
            var existingGemeentenaam = "Geraardsbergen";
            var postcode = "9500";

            //Arrange
            var request = new AddressMatchRequest().WithGemeenteAndStraatnaam();
            request.Niscode = null;
            request.Straatnaam = "Eesbekestraat";
            request.Gemeentenaam = existingGemeentenaam;
            request.Postcode = postcode;
            request.Huisnummer = "45";

            MockGetAllLatestMunicipalities(existingNisCode, existingGemeentenaam);
            MockStreetNames("Atembekestraat", existingStraatnaamId, existingNisCode, existingGemeentenaam);
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

            firstMatch.Score.Should().NotBe(100);
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
                }.ToDictionary(x => x.NisCode));

            return municipalityLatestItem;
        }

        private void MockGetAllLatestMunicipalities(IEnumerable<string> nisCodes, string municipalityName)
        {
            _latestQueries
                .Setup(x => x.GetAllLatestMunicipalities())
                .Returns(
                    nisCodes.Select(x =>
                        new MunicipalityLatestItem(Guid.NewGuid(), x, SystemClock.Instance.GetCurrentInstant())
                        {
                            NameDutch = municipalityName,
                            NameDutchSearch = municipalityName.RemoveDiacritics(),
                            OfficialLanguages = new List<string> { "Dutch" }
                        }).ToDictionary(x => x.NisCode)
                );
        }

        private void MockGetAllPostalInfo(string nisCode, string postalCode, string postalName = "")
        {
            var postalInfoLatestItem = new PostalLatestItem { NisCode = nisCode, PostalCode = postalCode, };

            if (!string.IsNullOrWhiteSpace(postalName))
            {
                postalInfoLatestItem.PostalNames = new List<PostalInfoPostalName>
                {
                    new PostalInfoPostalName
                    {
                        Language = PostalLanguage.Dutch, PostalCode = postalCode, PostalName = postalName
                    }
                };
            }

            _latestQueries
                .Setup(x => x.GetAllPostalInfo())
                .Returns(new[] { postalInfoLatestItem });
        }

        private StreetNameLatestItem MockStreetNames(string streetName, int streetNamePersistentLocalId, string nisCode, string municipalityName)
        {

            var streetNameLatestItem = new StreetNameLatestItem(streetNamePersistentLocalId, nisCode)
            {
                NameDutch = streetName,
                NameDutchSearch = streetName.RemoveDiacritics()
            };

            _latestQueries
                .Setup(x => x.GetAllLatestStreetNamesByPersistentLocalId())
                .Returns(new[] { streetNameLatestItem }.ToDictionary(x => x.PersistentLocalId));
            _latestQueries
                .Setup(x => x.GetLatestStreetNamesBy(new[] { municipalityName }, It.IsAny<Consumer.Read.StreetName.Projections.StreetNameStatus?>()))
                .Returns((string[] _, Consumer.Read.StreetName.Projections.StreetNameStatus? status) =>
                    new[] { streetNameLatestItem }.Where(x => !status.HasValue || x.Status == status.Value));
            _latestQueries
                .Setup(x => x.FindLatestStreetNameById(streetNamePersistentLocalId))
                .Returns(streetNameLatestItem);

            return streetNameLatestItem;
        }

        private void MockGetLatestAddressesBy(int streetNamePersistentLocalId, string postcode, string huisnummer, string? busnummer = null)
        {
            var addresses = new[]
            {
                new AddressDetailItemV2WithParent(2, streetNamePersistentLocalId, null, postcode, huisnummer, busnummer,
                    AddressStatus.Current, true, new Point(120, 45).AsBinary(), GeometryMethod.DerivedFromObject,
                    GeometrySpecification.Entry, false, SystemClock.Instance.GetCurrentInstant())
            };

            _latestQueries
                .Setup(x => x.GetLatestAddressesBy(
                    It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<AddressStatus?>()))
                .Returns((int _, string _, string? _, AddressStatus? status) =>
                    addresses.Where(a => !status.HasValue || a.Status == status.Value));
        }

        private void MockGetLatestAddressesByWithStatus(int streetNamePersistentLocalId, string postcode, string huisnummer, AddressStatus addressStatus, string? busnummer = null)
        {
            var addresses = new[]
            {
                new AddressDetailItemV2WithParent(2, streetNamePersistentLocalId, null, postcode, huisnummer, busnummer,
                    addressStatus, true, new Point(120, 45).AsBinary(), GeometryMethod.DerivedFromObject,
                    GeometrySpecification.Entry, false, SystemClock.Instance.GetCurrentInstant())
            };

            _latestQueries
                .Setup(x => x.GetLatestAddressesBy(
                    It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<AddressStatus?>()))
                .Returns((int _, string _, string? _, AddressStatus? status) =>
                    addresses.Where(a => !status.HasValue || a.Status == status.Value));
        }

        private void MockGetLatestAddressesByWithMultipleStatuses(int streetNamePersistentLocalId, string postcode, string huisnummer)
        {
            var addresses = new[]
            {
                new AddressDetailItemV2WithParent(2, streetNamePersistentLocalId, null, postcode, huisnummer, null,
                    AddressStatus.Current, true, new Point(120, 45).AsBinary(), GeometryMethod.DerivedFromObject,
                    GeometrySpecification.Entry, false, SystemClock.Instance.GetCurrentInstant()),
                new AddressDetailItemV2WithParent(3, streetNamePersistentLocalId, null, postcode, huisnummer, "A",
                    AddressStatus.Proposed, true, new Point(121, 46).AsBinary(), GeometryMethod.DerivedFromObject,
                    GeometrySpecification.Entry, false, SystemClock.Instance.GetCurrentInstant())
            };

            _latestQueries
                .Setup(x => x.GetLatestAddressesBy(
                    It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<AddressStatus?>()))
                .Returns((int _, string _, string? _, AddressStatus? status) =>
                    addresses.Where(a => !status.HasValue || a.Status == status.Value));
        }

        [Fact]
        public async Task CanFilterAdresMatchByAdresStatus()
        {
            var existingNisCode = "11001";
            var existingStraatnaamId = 1;
            var existingGemeentenaam = "Springfield";
            var postcode = "9000";

            //Arrange
            var request = new AddressMatchRequest().WithGemeenteAndStraatnaam();
            request.Huisnummer = "742";
            request.Status = "InGebruik";

            MockGetAllLatestMunicipalities(existingNisCode, existingGemeentenaam);
            MockStreetNames(request.Straatnaam, existingStraatnaamId, existingNisCode, existingGemeentenaam);
            MockGetLatestAddressesByWithStatus(existingStraatnaamId, postcode, request.Huisnummer, AddressStatus.Current);

            //Act
            var response = await _handler.Handle(request, CancellationToken.None);

            //Assert
            response.Should().NotBeNull();
            response.Should().HaveMatches(1);

            var firstMatch = response.AdresMatches.First();
            firstMatch.AdresStatus.Should().Be(Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres.AdresStatus.InGebruik);
        }

        [Fact]
        public async Task CanFilterAdresMatchByAdresStatus_FiltersOutNonMatchingStatus()
        {
            var existingNisCode = "11001";
            var existingStraatnaamId = 1;
            var existingGemeentenaam = "Springfield";
            var postcode = "9000";

            //Arrange
            var request = new AddressMatchRequest().WithGemeenteAndStraatnaam();
            request.Huisnummer = "742";
            request.Status = "Voorgesteld";

            MockGetAllLatestMunicipalities(existingNisCode, existingGemeentenaam);
            MockStreetNames(request.Straatnaam, existingStraatnaamId, existingNisCode, existingGemeentenaam);
            MockGetLatestAddressesByWithStatus(existingStraatnaamId, postcode, request.Huisnummer, AddressStatus.Current);

            //Act
            var response = await _handler.Handle(request, CancellationToken.None);

            //Assert
            response.Should().NotBeNull();
            // Addresses are filtered out at query level (Current != Proposed),
            // and street names are filtered out at BuildResults level (null != Proposed),
            // so no matches are returned
            response.Should().HaveMatches(0);
        }

        [Fact]
        public async Task CanFilterAdresMatchByAdresStatus_MultipleAddresses()
        {
            var existingNisCode = "11001";
            var existingStraatnaamId = 1;
            var existingGemeentenaam = "Springfield";
            var postcode = "9000";

            //Arrange
            var request = new AddressMatchRequest().WithGemeenteAndStraatnaam();
            request.Huisnummer = "742";
            request.Status = "InGebruik";

            MockGetAllLatestMunicipalities(existingNisCode, existingGemeentenaam);
            MockStreetNames(request.Straatnaam, existingStraatnaamId, existingNisCode, existingGemeentenaam);
            MockGetLatestAddressesByWithMultipleStatuses(existingStraatnaamId, postcode, request.Huisnummer);

            //Act
            var response = await _handler.Handle(request, CancellationToken.None);

            //Assert
            response.Should().NotBeNull();
            response.Should().HaveMatches(1);
            response.AdresMatches.First().AdresStatus.Should().Be(Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres.AdresStatus.InGebruik);
        }

        [Fact]
        public async Task CanFilterStraatnaamByStatus()
        {
            var existingNisCode = "11001";
            var existingStraatnaamId = 1;
            var existingGemeentenaam = "Springfield";

            //Arrange
            var request = new AddressMatchRequest().WithGemeenteAndStraatnaam();
            request.Status = "InGebruik";

            MockGetAllLatestMunicipalities(existingNisCode, existingGemeentenaam);
            var streetName = MockStreetNames(request.Straatnaam, existingStraatnaamId, existingNisCode, existingGemeentenaam);
            streetName.Status = Consumer.Read.StreetName.Projections.StreetNameStatus.Current;

            //Act
            var response = await _handler.Handle(request, CancellationToken.None);

            //Assert
            response.Should().NotBeNull();
            response.Should().HaveMatches(1);
            response.AdresMatches.First().Should().HaveStraatnaam()
                .Which.Should().HaveStraatnaam(request.Straatnaam);
        }

        [Fact]
        public async Task CanFilterStraatnaamByStatus_FiltersOutNonMatchingStatus()
        {
            var existingNisCode = "11001";
            var existingStraatnaamId = 1;
            var existingGemeentenaam = "Springfield";

            //Arrange
            var request = new AddressMatchRequest().WithGemeenteAndStraatnaam();
            request.Status = "InGebruik";

            MockGetAllLatestMunicipalities(existingNisCode, existingGemeentenaam);
            var streetName = MockStreetNames(request.Straatnaam, existingStraatnaamId, existingNisCode, existingGemeentenaam);
            streetName.Status = Consumer.Read.StreetName.Projections.StreetNameStatus.Retired;

            //Act
            var response = await _handler.Handle(request, CancellationToken.None);

            //Assert
            response.Should().NotBeNull();
            // Street name matched by name but filtered out by status in BuildResults
            response.Should().HaveMatches(0);
        }

        [Fact]
        public async Task StatusFilter_NoFilter_ReturnsAllResults()
        {
            var existingNisCode = "11001";
            var existingStraatnaamId = 1;
            var existingGemeentenaam = "Springfield";
            var postcode = "9000";

            //Arrange
            var request = new AddressMatchRequest().WithGemeenteAndStraatnaam();
            request.Huisnummer = "742";

            MockGetAllLatestMunicipalities(existingNisCode, existingGemeentenaam);
            MockStreetNames(request.Straatnaam, existingStraatnaamId, existingNisCode, existingGemeentenaam);
            MockGetLatestAddressesByWithMultipleStatuses(existingStraatnaamId, postcode, request.Huisnummer);

            //Act
            var response = await _handler.Handle(request, CancellationToken.None);

            //Assert
            response.Should().NotBeNull();
            response.Should().HaveMatches(2);
        }
    }

    public class AddressMatchContextMemoryV2 : AddressMatchContextV2
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseInMemoryDatabase("DB", AddressMatchTestBase.InMemoryDatabaseRootRoot);
    }
}
