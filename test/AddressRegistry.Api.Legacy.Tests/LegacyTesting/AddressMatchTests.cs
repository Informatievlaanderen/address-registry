namespace AddressRegistry.Api.Legacy.Tests.LegacyTesting
{
    using AddressMatch.Requests;
    using AddressMatch.Responses;
    using Assert;
    using Generate;
    using Microsoft.AspNetCore.Mvc;
    using Mocking;
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;
    using Xunit.Abstractions;

    public class AddressMatchTests : BehavioralTestBase
    {
        public AddressMatchTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper, m => { testOutputHelper.WriteLine(m); Trace.WriteLine(m); })
        { }

        [Theory]
        [InlineData("Springfield", "Springfield", true, true)]
        [InlineData("Springfeld", "Springfield", true, false)]
        [InlineData("Springfelt", "Springfield", true, false)]
        [InlineData("Sprungfelt", "Springfield", false, false)]//no match
        public async Task CanFindGemeenteByGemeentenaamMatch(string requestedGemeentenaam, string existingGemeentenaam, bool isMatch, bool isExactMatch)
        {
            string existingNisCode = Generate.Generate.NISCode.Generate(_random);

            //Arrange
            AddressMatchRequest request = new AddressMatchRequest().WithGemeenteAndStraatnaam();
            request.Gemeentenaam = requestedGemeentenaam;

            _latest.ArrangeLatestGemeente(existingNisCode, existingGemeentenaam);

            //Act
            var response = (AddressMatchCollection)((OkObjectResult)await Send(request)).Value;

            //Assert
            response.Should().NotBeNull();
            if (isMatch)
            {
                response.Should().HaveMatches(1);
                AdresMatchItem firstMatch = response.AdresMatches.First();
                firstMatch.Should().HaveGemeente()
                    .Which.Should().HaveGemeentenaam(existingGemeentenaam)
                    .And.HaveObjectId(existingNisCode);
                firstMatch.Should().NotHaveVolledigAdres();
                response.Should().ContainWarning("'Straatnaam' niet interpreteerbaar.");
            }
            else
            {
                response.Should().HaveMatches(0);
            }
            if (!isExactMatch)
                response.Should().ContainWarning("Onbekende 'Gemeentenaam'.");

            //response.Should().BeEquivalentTo(DoMatchTheOldWay(request));
        }

        [Fact]
        public async Task CanFindGemeenteByPostcode()
        {
            string existingNisCode = Generate.Generate.NISCode.Generate(_random);
            string existingGemeentenaam = "Springfield";
            string requestedPostcode = "49007";
            //Arrange
            AddressMatchRequest request = new AddressMatchRequest().WithPostcodeAndStraatnaam();
            request.Postcode = requestedPostcode;

            _latest.ArrangeLatestPostInfo(existingNisCode, requestedPostcode);
            _latest.ArrangeLatestGemeente(existingNisCode, existingGemeentenaam);

            //Act
            var response = (AddressMatchCollection)((OkObjectResult)await Send(request)).Value;

            //Assert
            response.Should().NotBeNull();
            response.Should().HaveMatches(1);
            AdresMatchItem firstMatch = response.AdresMatches.First();
            firstMatch.Should().HaveGemeente()
                .Which.Should().HaveGemeentenaam(existingGemeentenaam)
                .And.HaveObjectId(existingNisCode);
            firstMatch.Should().NotHaveVolledigAdres();
            response.Should().ContainWarning("'Straatnaam' niet interpreteerbaar.");

            //response.Should().BeEquivalentTo(DoMatchTheOldWay(request));
        }

        [Fact]
        public async Task CanFindGemeenteByDeelgemeente()
        {
            string existingNisCode = Generate.Generate.NISCode.Generate(_random);
            string existingGemeentenaam = "Springfield";
            string requestedGemeentenaam = "Deelgemeente";

            //Arrange
            AddressMatchRequest request = new AddressMatchRequest().WithGemeenteAndStraatnaam();
            request.Gemeentenaam = requestedGemeentenaam;

            _latest.ArrangeLatestPostInfoForPartOfMunicipality(existingNisCode, requestedGemeentenaam);
            _latest.ArrangeLatestGemeente(existingNisCode, existingGemeentenaam);

            //Act
            var response = (AddressMatchCollection)((OkObjectResult)await Send(request)).Value;

            //Assert
            response.Should().NotBeNull();
            response.Should().HaveMatches(1);
            AdresMatchItem firstMatch = response.AdresMatches.First();
            firstMatch.Should().HaveGemeente()
                .Which.Should().HaveGemeentenaam(existingGemeentenaam)
                .And.HaveObjectId(existingNisCode);
            firstMatch.Should().NotHaveVolledigAdres();
            response.Should().ContainWarning("'Straatnaam' niet interpreteerbaar.");

            //response.Should().BeEquivalentTo(DoMatchTheOldWay(request));
        }

        [Fact]
        public async Task CanFindGemeenteByNiscode()
        {
            string existingGemeentenaam = "Springfield";
            string requestedNiscode = "12345";
            //Arrange
            //request
            AddressMatchRequest request = new AddressMatchRequest().WithNISCodeAndStraatnaam();
            request.Niscode = requestedNiscode;

            _latest.ArrangeLatestGemeente(requestedNiscode, existingGemeentenaam);

            //Act
            var response = (AddressMatchCollection)((OkObjectResult)await Send(request)).Value;

            //Assert
            response.Should().NotBeNull();
            response.Should().HaveMatches(1);
            AdresMatchItem firstMatch = response.AdresMatches.First();
            firstMatch.Should().HaveGemeente()
                .Which.Should().HaveGemeentenaam(existingGemeentenaam)
                .And.HaveObjectId(requestedNiscode);
            firstMatch.Should().NotHaveVolledigAdres();

            response.Should().ContainWarning("'Straatnaam' niet interpreteerbaar.");

            //response.Should().BeEquivalentTo(DoMatchTheOldWay(request));
        }

        [Theory]
        [InlineData("Evergreen Terrace", "Evergreen Terrace")]//exact match
        [InlineData("Evergreen Terras", "Evergreen Terrace")]//fuzzy match
        [InlineData("evergreen terras", "Evergreen Terrace")]//case insensitive
        [InlineData("St-Evergreen Terrace", "Sint-Evergreen Terrace")]//replace abreviations in input
        [InlineData("Onze Lieve Vrouw-Evergreen Terrace", "O.l.v.-Evergreen Terrace")]//replace abreviations in existing straatnaam
        [InlineData("Clevergreen Terrace Avenue", "Evergreen")]//containment of existing straatnaam
        [InlineData("Evergreen", "Clevergreen Terrace Avenue")]//containment of input
        [InlineData("Trammesantlei", "Evergreen Terrace", false)]//no match
        public async Task CanFindStraatnaamByStraatnaamMatch(string requestedStraatnaam, string existingStraatnaam, bool isMatch = true)
        {
            string existingNisCode = Generate.Generate.NISCode.Generate(_random);
            int existingStraatnaamId = Generate.Generate.VbrObjectIDInt.Generate(_random);
            string existingGemeentenaam = "Springfield";

            //Arrange
            AddressMatchRequest request = new AddressMatchRequest().WithGemeenteAndStraatnaam();
            request.Straatnaam = requestedStraatnaam;

            _latest.ArrangeLatestGemeente(existingNisCode, existingGemeentenaam);
            _latest.ArrangeLatestStraatnaam(existingNisCode, existingGemeentenaam, existingStraatnaamId.ToString(), existingStraatnaam, Guid.NewGuid());

            //Act
            var response = (AddressMatchCollection)((OkObjectResult)await Send(request)).Value;

            //Assert
            response.Should().NotBeNull();
            if (isMatch)
            {
                response.Should().HaveMatches(1);
                AdresMatchItem firstMatch = response.AdresMatches.First();
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
                AdresMatchItem firstMatch = response.AdresMatches.First();
                firstMatch.Should().HaveGemeente()
                    .Which.Should().HaveGemeentenaam(existingGemeentenaam)
                    .And.HaveObjectId(existingNisCode);
                firstMatch.Should().HaveNoStraatnaam();
                response.Should().ContainWarning("'Straatnaam' niet interpreteerbaar.");
            }

            //response.Should().BeEquivalentTo(DoMatchTheOldWay(request));
        }

        [Fact]
        public async Task CanFindStraatnaamByKadStraatCode()
        {
            string existingNisCode = Generate.Generate.NISCode.Generate(_random);
            int existingStraatnaamId = Generate.Generate.VbrObjectIDInt.Generate(_random);
            string existingGemeentenaam = "Springfield";
            string existingStraatnaam = "Evergreen Terrace";
            string requestededKadStraatCode = "6789";
            //Arrange
            AddressMatchRequest request = new AddressMatchRequest().WithGemeenteAndKadStraatcode();
            request.KadStraatcode = requestededKadStraatCode;

            _latest.ArrangeLatestGemeente(existingNisCode, existingGemeentenaam);
            _kadRrService.ArrangeKadStraatnamen(existingNisCode, existingGemeentenaam, existingStraatnaamId, existingStraatnaam, requestededKadStraatCode);

            //Act
            var response = (AddressMatchCollection)((OkObjectResult)await Send(request)).Value;

            //Assert
            response.Should().NotBeNull();
            response.Should().HaveMatches(1);
            AdresMatchItem firstMatch = response.AdresMatches.First();
            firstMatch.Should().HaveGemeente()
                .Which.Should().HaveGemeentenaam(existingGemeentenaam)
                .And.HaveObjectId(existingNisCode);
            firstMatch.Should().HaveStraatnaam()
                .Which.Should().HaveStraatnaam(existingStraatnaam)
                .And.HaveObjectId(existingStraatnaamId.ToString());
            firstMatch.Should().NotHaveVolledigAdres();

            //response.Should().BeEquivalentTo(DoMatchTheOldWay(request));
        }

        [Fact]
        public async Task CanFindStraatnaamByRrStraatCode()
        {
            string existingNisCode = Generate.Generate.NISCode.Generate(_random);
            int existingStraatnaamId = Generate.Generate.VbrObjectIDInt.Generate(_random);
            string existingGemeentenaam = "Springfield";
            string existingStraatnaam = "Evergreen Terrace";
            string requestededRrStraatCode = "987";

            //Arrange
            AddressMatchRequest request = new AddressMatchRequest().WithPostcodeAndRrStraatcode();
            request.RrStraatcode = requestededRrStraatCode;

            _latest.ArrangeLatestGemeente(existingNisCode, existingGemeentenaam);
            _kadRrService.ArrangeRrStraatnaam(request.Postcode, existingNisCode, existingGemeentenaam, existingStraatnaamId, existingStraatnaam, requestededRrStraatCode);
            _latest.ArrangeLatestPostInfo(existingNisCode, request.Postcode);

            //Act
            var response = (AddressMatchCollection)((OkObjectResult)await Send(request)).Value;

            //Assert
            response.Should().NotBeNull();
            response.Should().HaveMatches(1);
            AdresMatchItem firstMatch = response.AdresMatches.First();
            firstMatch.Should().HaveGemeente()
                .Which.Should().HaveGemeentenaam(existingGemeentenaam)
                .And.HaveObjectId(existingNisCode);
            firstMatch.Should().HaveStraatnaam()
                .Which.Should().HaveStraatnaam(existingStraatnaam)
                .And.HaveObjectId(existingStraatnaamId.ToString());
            firstMatch.Should().NotHaveVolledigAdres();

            //response.Should().BeEquivalentTo(DoMatchTheOldWay(request));
        }

        [Fact]
        public async Task CanFindRrAdres()
        {
            //Arrange
            string existingNisCode = Generate.Generate.NISCode.Generate(_random);
            string existingGemeentenaam = "Springfield";
            int existingStraatnaamId = Generate.Generate.VbrObjectIDInt.Generate(_random);
            string existingStraatnaam = "Evergreen Terrace";
            var streetNameId = Guid.NewGuid();

            AddressMatchRequest request = new AddressMatchRequest().WithPostcodeAndRrStraatcode();
            request.Huisnummer = "15";

            var gemeente = _latest.ArrangeLatestGemeente(existingNisCode, existingGemeentenaam).First();
            var straat = _latest.ArrangeLatestStraatnaam(existingNisCode, existingGemeentenaam, existingStraatnaamId.ToString(), existingStraatnaam, streetNameId).First();

            var mappedAdressen = _kadRrService
                .Arrange(Produce.Many(Generate.Generate.tblHuisNummer.Select(x => x.WithStraatNaamId(streetNameId))),
                    (when, x) => when.AdresMappingExistsFor(x, request.Huisnummer, request.Index, request.RrStraatcode, request.Postcode));

            //Act
            var response = (AddressMatchCollection)((OkObjectResult)await Send(request)).Value;

            //Assert
            response.Should().NotBeNull();
            response.Should().HaveMatches(Math.Min(mappedAdressen.Count(), 10));
            AdresMatchItem firstMatch = response.AdresMatches.First();
            firstMatch.Should().HaveGemeente()
                .Which.Should().HaveGemeentenaam(existingGemeentenaam)
                .And.HaveObjectId(existingNisCode);
            firstMatch.Should().HaveStraatnaam()
                .Which.Should().HaveStraatnaam(existingStraatnaam)
                .And.HaveObjectId(existingStraatnaamId.ToString());
            firstMatch.Should().HaveVolledigAdres()
                .Which.Should().HaveGeografischeNaam($"{existingStraatnaam} {mappedAdressen.First().HouseNumber} bus {mappedAdressen.First().BoxNumber}, {mappedAdressen.First().PostalCode} {existingGemeentenaam}");
        }

        [Fact]
        public async Task CanFindAdresMatch()
        {
            string existingNisCode = Generate.Generate.NISCode.Generate(_random);
            int existingStraatnaamId = Generate.Generate.VbrObjectIDInt.Generate(_random);
            string existingGemeentenaam = "Springfield";
            string postcode = Generate.Generate.Postcode.Generate(_random);
            var streetNameId = Guid.NewGuid();

            //Arrange
            AddressMatchRequest request = new AddressMatchRequest().WithGemeenteAndStraatnaam();
            request.Huisnummer = "742";

            var existingGemeente = _latest.ArrangeLatestGemeente(existingNisCode, existingGemeentenaam).Single();
            var existingStraatnaam = _latest.ArrangeLatestStraatnaam(existingNisCode, existingGemeentenaam, existingStraatnaamId.ToString(), request.Straatnaam, streetNameId).Single();
            var existingAdres = _latest.ArrangeLatestAdres(existingGemeente, existingStraatnaam, postcode, request.Huisnummer, null).Single();

            //Act
            var response = (AddressMatchCollection)((OkObjectResult)await Send(request)).Value;

            //Assert
            response.Should().NotBeNull();
            response.Should().HaveMatches(1);
            AdresMatchItem firstMatch = response.AdresMatches.First();
            firstMatch.Should().HaveGemeente()
                .Which.Should().HaveGemeentenaam(existingGemeentenaam)
                .And.HaveObjectId(existingNisCode);
            firstMatch.Should().HaveStraatnaam()
                .Which.Should().HaveStraatnaam(request.Straatnaam)
                .And.HaveObjectId(existingStraatnaamId.ToString());
            firstMatch.Should().HaveVolledigAdres()
                .Which.Should().HaveGeografischeNaam($"{existingStraatnaam.NameDutch} {request.Huisnummer}, {postcode} {existingGemeentenaam}");

            //response.Should().BeEquivalentTo(DoMatchTheOldWay(request));
        }

        [Fact]
        public async Task AdresMatchWithBusnummerSkipsSanitization()
        {
            string existingNisCode = Generate.Generate.NISCode.Generate(_random);
            int existingStraatnaamId = Generate.Generate.VbrObjectIDInt.Generate(_random);
            string existingGemeentenaam = "Springfield";
            string postcode = Generate.Generate.Postcode.Generate(_random);

            //Arrange
            AddressMatchRequest request = new AddressMatchRequest().WithGemeenteAndStraatnaam();
            request.Postcode = postcode;
            request.Huisnummer = "742";
            request.Busnummer = "C2";

            var existingGemeente = _latest.ArrangeLatestGemeente(existingNisCode, existingGemeentenaam).Single();
            var existingStraatnaam = _latest.ArrangeLatestStraatnaam(existingNisCode, existingGemeentenaam, existingStraatnaamId.ToString(), request.Straatnaam, Guid.NewGuid()).Single();
            var existingAdres = _latest.ArrangeLatestAdres(existingGemeente, existingStraatnaam, postcode, request.Huisnummer, request.Busnummer).Single();

            //Act
            var response = (AddressMatchCollection)((OkObjectResult)await Send(request)).Value;

            //Assert
            response.Should().NotBeNull();
            response.Should().HaveMatches(1);
            AdresMatchItem firstMatch = response.AdresMatches.First();
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
