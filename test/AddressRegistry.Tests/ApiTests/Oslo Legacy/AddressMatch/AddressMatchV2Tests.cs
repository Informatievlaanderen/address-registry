namespace AddressRegistry.Tests.ApiTests.Oslo_Legacy.AddressMatch
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using AddressRegistry.Api.Oslo.AddressMatch.Requests;
    using Framework;
    using Framework.Assert;
    using Framework.Generate;
    using Framework.Mocking;
    using Xunit;
    using Xunit.Abstractions;

    public class AddressMatchV2Tests : BehavioralTestBaseV2
    {
        public AddressMatchV2Tests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper, m => { testOutputHelper.WriteLine(m); Trace.WriteLine(m); }) { }

        [Theory]
        [InlineData("Springfield", "Springfield", true, true)]
        [InlineData("Springfeld", "Springfield", true, false)]
        [InlineData("Springfelt", "Springfield", true, false)]
        [InlineData("Sprungfelt", "Springfield", false, false)] // no match
        public async Task CanFindGemeenteByGemeentenaamMatch(string requestedGemeentenaam, string existingGemeentenaam, bool isMatch, bool isExactMatch)
        {
            var existingNisCode = Generate.NisCode.Generate(Random);

            //Arrange
            var request = new AddressMatchRequest().WithGemeenteAndStraatnaam();
            request.Gemeentenaam = requestedGemeentenaam;

            Latest.ArrangeLatestGemeente(existingNisCode, existingGemeentenaam);

            //Act
            var response = await Send(request);

            //Assert
            response.Should().NotBeNull();

            if (isMatch)
            {
                response.Should().HaveMatches(1);

                var firstMatch = response.AdresMatches.First();

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
        }

        [Fact]
        public async Task CanFindGemeenteByPostcode()
        {
            var existingNisCode = Generate.NisCode.Generate(Random);
            var existingGemeentenaam = "Springfield";
            var requestedPostcode = "4900";

            //Arrange
            var request = new AddressMatchRequest().WithPostcodeAndStraatnaam();
            request.Postcode = requestedPostcode;

            Latest.ArrangeLatestPostInfo(existingNisCode, requestedPostcode);
            Latest.ArrangeLatestGemeente(existingNisCode, existingGemeentenaam);

            //Act
            var response = await Send(request);

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
        public async Task CanFindGemeenteByDeelgemeente()
        {
            var existingNisCode = Generate.NisCode.Generate(Random);
            var existingGemeentenaam = "Springfield";
            var requestedGemeentenaam = "Deelgemeente";

            //Arrange
            AddressMatchRequest request = new AddressMatchRequest().WithGemeenteAndStraatnaam();
            request.Gemeentenaam = requestedGemeentenaam;

            Latest.ArrangeLatestPostInfoForPartOfMunicipality(existingNisCode, requestedGemeentenaam);
            Latest.ArrangeLatestGemeente(existingNisCode, existingGemeentenaam);

            //Act
            var response = await Send(request);

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

            Latest.ArrangeLatestGemeente(request.Niscode, request.Gemeentenaam);

            //Act
            var response = await Send(request);

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

            Latest.ArrangeLatestGemeenteWithRandomNisCodes(request.Gemeentenaam, 2);

            //Act
            var response = await Send(request);

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
            var existingNisCode = Generate.NisCode.Generate(Random);
            var existingStraatnaamId = Generate.VbrObjectIdInt.Generate(Random);
            var existingGemeentenaam = "Springfield";

            //Arrange
            var request = new AddressMatchRequest().WithGemeenteAndStraatnaam();
            request.Straatnaam = requestedStraatnaam;

            Latest.ArrangeLatestGemeente(existingNisCode, existingGemeentenaam);
            Latest.ArrangeLatestStraatnaam(existingNisCode, existingGemeentenaam, existingStraatnaamId, existingStraatnaam, Guid.NewGuid());

            //Act
            var response = await Send(request);

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
            var existingNisCode = Generate.NisCode.Generate(Random);
            var existingStraatnaamId = Generate.VbrObjectIdInt.Generate(Random);
            var existingGemeentenaam = "Springfield";
            var postcode = Generate.Postcode.Generate(Random);
            var streetNameId = Guid.NewGuid();

            //Arrange
            var request = new AddressMatchRequest().WithGemeenteAndStraatnaam();
            request.Huisnummer = "742";

            var existingGemeente = Latest.ArrangeLatestGemeente(existingNisCode, existingGemeentenaam).Single();
            var existingStraatnaam = Latest.ArrangeLatestStraatnaam(existingNisCode, existingGemeentenaam, existingStraatnaamId, request.Straatnaam, streetNameId).Single();
            var existingAdres = Latest.ArrangeLatestAdres(existingGemeente, existingStraatnaam, postcode, request.Huisnummer, null).Single();

            //Act
            var response = await Send(request);

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
                .Which.Should().HaveGeografischeNaam($"{existingStraatnaam.NameDutch} {request.Huisnummer}, {postcode} {existingGemeentenaam}");
        }

        [Fact]
        public async Task AdresMatchWithBusnummerSkipsSanitization()
        {
            var existingNisCode = Generate.NisCode.Generate(Random);
            var existingStraatnaamId = Generate.VbrObjectIdInt.Generate(Random);
            var existingGemeentenaam = "Springfield";
            var postcode = Generate.Postcode.Generate(Random);

            //Arrange
            var request = new AddressMatchRequest().WithGemeenteAndStraatnaam();
            request.Postcode = postcode;
            request.Huisnummer = "742";
            request.Busnummer = "C2";

            var existingGemeente = Latest.ArrangeLatestGemeente(existingNisCode, existingGemeentenaam).Single();
            var existingStraatnaam = Latest.ArrangeLatestStraatnaam(existingNisCode, existingGemeentenaam, existingStraatnaamId, request.Straatnaam, Guid.NewGuid()).Single();
            Latest.ArrangeLatestAdres(existingGemeente, existingStraatnaam, postcode, request.Huisnummer, request.Busnummer);

            //Act
            var response = await Send(request);

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
