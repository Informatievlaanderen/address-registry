namespace AddressRegistry.Tests.ApiTests.AddressMatch
{
    using System.Linq;
    using Api.Oslo.AddressMatch;
    using FluentAssertions;
    using Xunit;

    public class AdresMatchSanitizationTests
    {
        private readonly Sanitizer _sanitizer;

        public AdresMatchSanitizationTests()
        {
            _sanitizer = new Sanitizer();
        }

        [Theory]
        [InlineData("4", new[] { "4" })]
        [InlineData("10", new[] { "10" })]
        [InlineData("4b", new[] { "4", "4B" })]
        [InlineData("10e", new[] { "10", "10E" })]
        [InlineData("10E", new[] { "10", "10E" })]
        [InlineData("10 e", new[] { "10", "10E" })]
        [InlineData("10 E", new[] { "10", "10E" })]
        [InlineData("10bis", new[] { "10BIS" })]
        [InlineData("30-", new[] { "30" })]
        [InlineData("naast nr 30", new[] { "30" })]
        [InlineData("0030", new[] { "30" })]
        public void HouseNumberAsHouseNumber(string input, string[] expectedHouseNumbers)
        {
            var result = _sanitizer.Sanitize("teststraat", input, null);

            result.Select(x => x.HouseNumber).Should().BeEquivalentTo(expectedHouseNumbers);
        }

        [Theory]
        [InlineData("5 teststraat", "5")]
        [InlineData("10 teststraat", "10")]
        [InlineData("teststraat 5", "5")]
        [InlineData("teststraat 10", "10")]
        [InlineData("teststraat, 10", "10")]
        [InlineData("teststraat. 10", "10")]
        public void HouseNumberInStreetNameIsParsed(string input, string expectedHouseNumber)
        {
            var result = _sanitizer.Sanitize(input, null, null);

            result.Should().ContainSingle();
            result.Single().HouseNumber.Should().Be(expectedHouseNumber);
        }

        [Fact]
        public void CommaSeparatedHouseNumberInStreetNameIsNotSupported()
        {
            var result = _sanitizer.Sanitize("10, teststraat", null, null);

            result.Should().BeEmpty();
        }

        [Fact]
        public void InvalidStreetNameIsNotSupported()
        {
            var result = _sanitizer.Sanitize("00", null, null);

            result.Should().BeEmpty();
        }

        [Theory]
        [InlineData("4b teststraat", new[] { "4B", "4" })]
        [InlineData("10b teststraat", new[] { "10B", "10" })]
        [InlineData("10e teststraat", new[] { "10E", "10" })]
        [InlineData("10E teststraat", new[] { "10E", "10" })]
        [InlineData("teststraat 4b", new[] { "4B", "4" })]
        [InlineData("teststraat 10b", new[] { "10B", "10" })]
        [InlineData("teststraat 10e", new[] { "10E", "10" })]
        [InlineData("teststraat 10E", new[] { "10E", "10" })]
        public void StreetNameWithHouseNumberWithValidLetterIsParsed(string input, string[] expectedHouseNumbers)
        {
            var result = _sanitizer.Sanitize(input, null, null);

            result.Should().HaveCount(expectedHouseNumbers.Length);
            result.Select(x => x.HouseNumber).Should().BeEquivalentTo(expectedHouseNumbers);
        }

        [Fact]
        public void StreetNameWithHouseNumberWithInvalidLettersIsNotSupported()
        {
            var result = _sanitizer.Sanitize("10bis teststraat", null, null);

            result.Should().BeEmpty();
        }

        [Theory]
        [InlineData("30/20", "30", "20")]
        [InlineData("30-20", "30", "20")]
        [InlineData("30+20", "30", "20")]
        [InlineData("30 20", "30", "20")]
        public void HouseNumberWithTwoNumbersAndFirstNumberIsHighest_ThenHouseNumberAndBoxNumber(string input, string expectedHouseNumber, string expectedBoxNumber)
        {
            var result = _sanitizer.Sanitize("teststraat", input, null);

            result.Should().ContainSingle();
            result.Single().HouseNumber.Should().Be(expectedHouseNumber);
            result.Single().BoxNumber.Should().Be(expectedBoxNumber);
        }

        [Theory]
        [InlineData("20/30", new[] { "20", "22", "24", "26", "28", "30" })]
        [InlineData("20-30", new[] { "20", "22", "24", "26", "28", "30" })]
        [InlineData("20+30", new[] { "20", "22", "24", "26", "28", "30" })]
        [InlineData("20 30", new[] { "20", "22", "24", "26", "28", "30" })]
        [InlineData("21/27", new[] { "21", "23", "25", "27" })]
        [InlineData("21-27", new[] { "21", "23", "25", "27" })]
        [InlineData("21+27", new[] { "21", "23", "25", "27" })]
        [InlineData("21 27", new[] { "21", "23", "25", "27" })]
        public void HouseNumberWithTwoNumbersAndFirstNumberIsLowest_ThenHouseNumberRange(string input, string[] expectedHouseNumbers)
        {
            var result = _sanitizer.Sanitize("teststraat", input, null);

            result.Should().HaveCount(expectedHouseNumbers.Length);
            result.Select(x => x.HouseNumber).Should().BeEquivalentTo(expectedHouseNumbers);
        }

        [Fact]
        public void HouseNumberWithValidLetterWithRangeNotation_ThenHouseNumberWithBoxNumber()
        {
            var result = _sanitizer.Sanitize("teststraat", "30-b", null);

            result.Should().ContainSingle();
            result.Single().HouseNumber.Should().Be("30");
            result.Single().BoxNumber.Should().Be("B");
        }

        [Fact]
        // huisnummers met RR-index waarvan deel1 niet-numeriek is en waarvan deel2 numeriek is en gelijk aan 0
        // Het huisnummer + deel1 wordt een CRAB-huisnummer met een niet-numeriek bisnummer. Er is geen CRAB-subadres.
        // Als deel1 een aanduiding is van een subadres wordt enkel het RR-huisnummer als huisnummer weggeschreven.
        // Als deel1 een aanduiding is van een verdiep wordt enkel het RR-huisnummer als huisnummer weggeschreven en een subadres 0.0 met aard appartementnummer.
        // ******************************************************************************************************************************************************
        // --Opm: index moet uit 4 karakters bestaan of mag enkel niet-numerieke karakters bevatten anders worden er voorloop "0"-en aan geplakt en komen we in een ander code-path
        public void HouseNumberWithIndex_Part1NonNumeric_Part2Zero()
        {
            // // niet-numeriek bisnummer
            // Add(MockedSanitizationTest("teststraat", "10", "b")//of b000
            //    .Should().HaveCount(1)
            //    .And.First().Should().HaveHuisnummer("10b").And.HaveNoAppnummer().And.HaveNoBusnummer()
            //    .Continuation);
            //
            // // geen niet-numeriek bisnummer
            // Add(MockedSanitizationTest("teststraat", "10", "BUS")//of BUS0 //busnummer => mogelijke varianten: "BUS", "bte", "BT", "bu", "bs"
            //    .Should().HaveCount(1)
            //    .And.First().Should().HaveHuisnummer("10").And.HaveNoAppnummer().And.HaveNoBusnummer()
            //    .Continuation);
            //
            // // appartementnummer 0 => null
            // Add(MockedSanitizationTest("teststraat", "10", "RC")//of RC00 //verdiep => mogelijke varianten: "RC", "RCAR", "GV", "Rch", "RDC", "HAL", "rez", "RCH", "GV", "GVL", "GLV", "GEL", "GL"
            //    .Should().HaveCount(1)
            //    .And.First().Should().HaveHuisnummer("10").And.HaveAppnummer("0.0").And.HaveNoBusnummer()
            //    .Continuation);
            //
            // Add(MockedSanitizationTest("teststraat", "10", "gv")
            //    .Should().HaveCount(1)
            //    .And.First().Should().HaveHuisnummer("10").And.HaveAppnummer("0.0").And.HaveNoBusnummer()
            //    .Continuation);
            //
            // Add(MockedSanitizationTest("teststraat", "10", "et") //of et00 // verdiepnummer => mogelijke varianten: "et", "eta", "éta", "VER", "VDP", "VD", "Vr", "Vrd", "V", "Etg", "ét", "et", "ev", "eme", "ème", "STE", "de", "dev", "e", "E", "é", "links", "rechts"
            //    .Should().HaveCount(1)
            //    .And.First().Should().HaveHuisnummer("10").And.HaveNoAppnummer().And.HaveNoBusnummer()
            //    .Continuation);
            //
            // Add(MockedSanitizationTest("teststraat", "10", "E") //of et00 // verdiepnummer => mogelijke varianten: "et", "eta", "éta", "VER", "VDP", "VD", "Vr", "Vrd", "V", "Etg", "ét", "et", "ev", "eme", "ème", "STE", "de", "dev", "e", "E", "é", "links", "rechts"
            //     .Should().HaveCount(1)
            //     .And.First().Should().HaveHuisnummer("10").And.HaveNoAppnummer().And.HaveNoBusnummer()
            //     .Continuation);
            //
            // Add(MockedSanitizationTest("teststraat", "10", "E000")
            //    .Should().HaveCount(1)
            //    .And.First().Should().HaveHuisnummer("10").And.HaveNoAppnummer().And.HaveNoBusnummer()
            //    .Continuation);
            //
            // Add(MockedSanitizationTest("teststraat", "10", "E001")
            //    .Should().HaveCount(1)
            //    .And.First().Should().HaveHuisnummer("10").And.HaveAppnummer("E1").And.HaveNoBusnummer()
            //    .Continuation);
            //
            // Add(MockedSanitizationTest("teststraat", "10", "E01")
            //   .Should().HaveCount(1)
            //   .And.First().Should().HaveHuisnummer("10").And.HaveAppnummer("0.1").And.HaveNoBusnummer()
            //   .Continuation);
            //
            // Add(MockedSanitizationTest("teststraat", "10", "E00")
            //   .Should().HaveCount(1)
            //   .And.First().Should().HaveHuisnummer("10").And.HaveAppnummer("0.0").And.HaveNoBusnummer()
            //   .Continuation);
            //
            // Run();
        }

        [Fact]
        // huisnummers met RR-index waarvan deel1 niet-numeriek is en waarvan deel2 numeriek is en > 0
        // Het huisnummer wordt een CRAB-huisnummer, deel2 van de RR-index wordt een CRAB-subadres van type busnummer of appartementnummer.
        // Als deel1 een aanduiding is van een subadres wordt enkel het huisnummer als huisnummer weggeschreven.
        // *****************************************************************************************************
        // --Opm: index moet uit 4 karakters bestaan of anders worden er voorloop "0"-en aan geplakt en komen we in een ander code-path
        public void HouseNumberWithIndex_Part1NonNumeric_Part2NumericNonZero()
        {
            // // niet-numeriek bisnummer
            // Add(MockedSanitizationTest("teststraat", "10", "b028")
            //   .Should().HaveCount(1)
            //   .And.First().Should().HaveHuisnummer("10b").And.HaveNoAppnummer().And.HaveBusnummer("28")
            //   .Continuation);
            //
            // Add(MockedSanitizationTest("teststraat", "10", "a4")
            //   .Should().HaveCount(1)
            //   .And.First().Should().HaveHuisnummer("10a").And.HaveNoAppnummer().And.HaveBusnummer("4")
            //   .Continuation);
            //
            // Add(MockedSanitizationTest("teststraat", "10", "B  3")
            //   .Should().HaveCount(1)
            //   .And.First().Should().HaveHuisnummer("10B").And.HaveNoAppnummer().And.HaveBusnummer("3")
            //   .Continuation);
            // //geen niet-numeriek bisnummer
            // Add(MockedSanitizationTest("teststraat", "10", "BUS8")//busnummer => mogelijke varianten: "BUS", "bte", "BT", "bu", "bs"
            //   .Should().HaveCount(1)
            //   .And.First().Should().HaveHuisnummer("10").And.HaveNoAppnummer().And.HaveBusnummer("8")
            //   .Continuation);
            //
            // Add(MockedSanitizationTest("teststraat", "10", "BU8")//busnummer => mogelijke varianten: "BUS", "bte", "BT", "bu", "bs"
            //   .Should().HaveCount(1)
            //   .And.First().Should().HaveHuisnummer("10").And.HaveNoAppnummer().And.HaveBusnummer("8")
            //   .Continuation);
            //
            // Add(MockedSanitizationTest("teststraat", "10", "Bt8")//busnummer => mogelijke varianten: "BUS", "bte", "BT", "bu", "bs"
            //   .Should().HaveCount(1)
            //   .And.First().Should().HaveHuisnummer("10").And.HaveNoAppnummer().And.HaveBusnummer("8")
            //   .Continuation);
            //
            // Add(MockedSanitizationTest("teststraat", "10", "BS8")//busnummer => mogelijke varianten: "BUS", "bte", "BT", "bu", "bs"
            //   .Should().HaveCount(1)
            //   .And.First().Should().HaveHuisnummer("10").And.HaveNoAppnummer().And.HaveBusnummer("8")
            //   .Continuation);
            //
            // Add(MockedSanitizationTest("teststraat", "10", "RC08")//verdiep => mogelijke varianten: "RC", "RCAR", "GV", "Rch", "RDC", "HAL", "rez", "RCH", "GV", "GVL", "GLV", "GEL", "GL"
            //   .Should().HaveCount(1)
            //   .And.First().Should().HaveHuisnummer("10").And.HaveAppnummer("RC8").And.HaveNoBusnummer()
            //   .Continuation);
            //
            // Add(MockedSanitizationTest("teststraat", "10", "et08")//verdiepnummer => mogelijke varianten: //verdiepnummer => mogelijke varianten: "et", "eta", "éta", "VER", "VDP", "VD", "Vr", "Vrd", "V", "Etg", "ét", "et", "ev", "eme", "ème", "STE", "de", "dev", "e", "E", "é", "links", "rechts"
            //   .Should().HaveCount(1)
            //   .And.First().Should().HaveHuisnummer("10").And.HaveAppnummer("et8").And.HaveNoBusnummer()
            //   .Continuation);
            //
            // Run();
        }

        [Fact]
        // huisnummers met RR-index waarvan deel1 begint met een cijfer en zonder deel 3.
        // Het huisnummer wordt een CRAB-huisnummer, deel1 van de RR-index wordt een CRAB-subadres van type busnummer.
        // ***********************************************************************************************************
        public void HouseNumberWithIndex_Part1StartNumber_Part3Missing()
        {
            // Add(MockedSanitizationTest("teststraat", "10", "28")
            //     .Should().HaveCount(1)
            //     .And.First().Should().HaveHuisnummer("10").And.HaveNoAppnummer().And.HaveBusnummer("28")
            //     .Continuation);
            //
            // Add(MockedSanitizationTest("teststraat", "10", " 666")
            //     .Should().HaveCount(1)
            //     .And.First().Should().HaveHuisnummer("10").And.HaveNoAppnummer().And.HaveBusnummer("666")
            //     .Continuation);
            //
            // Add(MockedSanitizationTest("teststraat", "10", "1234")
            //     .Should().HaveCount(1)
            //     .And.First().Should().HaveHuisnummer("10").And.HaveNoAppnummer().And.HaveBusnummer("1234")
            //     .Continuation);
            //
            // Run();
        }

        [Fact]
        // huisnummers met RR-index waarvan deel1 begint met een cijfer en met numeriek deel 3 en zonder deel 4
        // Het huisnummer wordt een CRAB-huisnummer, de RR-index wordt een CRAB-subadres van type appartementnummer.
        // *********************************************************************************************************
        public void HouseNumberWithIndex_Part1StartNumber_Part3Numeric_Part4Missing()
        {
            // Add(MockedSanitizationTest("teststraat", "10", "2/8")
            //     .Should().HaveCount(1)
            //     .And.First().Should().HaveHuisnummer("10").And.HaveAppnummer("2/8").And.HaveNoBusnummer()
            //     .Continuation);
            //
            // Add(MockedSanitizationTest("teststraat", "10", "-8")//begint eigenlijk wel niet met cijfer (pas na toevoeging voorloop "0"-en)
            //     .Should().HaveCount(1)
            //     .And.First().Should().HaveHuisnummer("10").And.HaveAppnummer("-8").And.HaveNoBusnummer()
            //     .Continuation);
            //
            // Add(MockedSanitizationTest("teststraat", "10", "1.02")
            //     .Should().HaveCount(1)
            //     .And.First().Should().HaveHuisnummer("10").And.HaveAppnummer("1.02").And.HaveNoBusnummer()
            //     .Continuation);
            //
            // Add(MockedSanitizationTest("teststraat", "10", "0.1")
            //     .Should().HaveCount(1)
            //     .And.First().Should().HaveHuisnummer("10").And.HaveAppnummer("0.1").And.HaveNoBusnummer()
            //     .Continuation);
            //
            // Run();
        }

        [Fact]
        // huisnummers met RR-index waarvan deel1 begint met een cijfer en met niet-numeriek deel 3 en numeriek deel 4
        // Het huisnummer + deel1 wordt een CRAB-huisnummer met een numeriek bisnummer, deel4 van de RR-index wordt een CRAB-subadres van type busnummer of appartementnummer.
        // Als deel3 een aanduiding is van een verdiepnummer wordt enkel het RR-huisnummer als huisnummer weggeschreven en worden deel1 en deel4 samengevoegd tot een appartementnummer.
        // *****************************************************************************************************************************************************************************
        public void HouseNumberWithIndex_Part1StartNumber_Part3NonNumeric_Part4Numeric()
        {
            // // niet-numeriek bisnummer
            // Add(MockedSanitizationTest("teststraat", "10", "11b0")
            //               .Should().HaveCount(1)
            //               .And.First().Should().HaveHuisnummer("10b").And.HaveNoAppnummer().And.HaveBusnummer("0")
            //               .Continuation);
            //
            // Add(MockedSanitizationTest("teststraat", "10", "1b01")
            //              .Should().HaveCount(1)
            //              .And.First().Should().HaveHuisnummer("10b").And.HaveNoAppnummer().And.HaveBusnummer("1")
            //              .Continuation);
            //
            // Add(MockedSanitizationTest("teststraat", "10", "A/b7")
            //              .Should().HaveCount(1)
            //              .And.First().Should().HaveHuisnummer("10b").And.HaveNoAppnummer().And.HaveBusnummer("7")
            //              .Continuation);
            //
            // // verdiepnummer
            // Add(MockedSanitizationTest("teststraat", "10", "2et1")
            //               .Should().HaveCount(1)
            //               .And.First().Should().HaveHuisnummer("10").And.HaveAppnummer("2.1").And.HaveNoBusnummer()
            //               .Continuation);
            //
            // Add(MockedSanitizationTest("teststraat", "10", "6,E3")
            //               .Should().HaveCount(1)
            //               .And.First().Should().HaveHuisnummer("10").And.HaveAppnummer("6.3").And.HaveNoBusnummer()
            //               .Continuation);
            //
            // Add(MockedSanitizationTest("teststraat", "10", "E6")//begint eigenlijk wel niet met cijfer (pas na toevoeging voorloop "0"-en)
            //               .Should().HaveCount(1)
            //               .And.First().Should().HaveHuisnummer("10").And.HaveAppnummer("0.6").And.HaveNoBusnummer()
            //               .Continuation);
            //
            // // appartementnummer
            // Add(MockedSanitizationTest("teststraat", "10", "AP7")//begint eigenlijk wel niet met cijfer (pas na toevoeging voorloop "0"-en)
            //               .Should().HaveCount(1)
            //               .And.First().Should().HaveHuisnummer("10").And.HaveAppnummer("7").And.HaveNoBusnummer()
            //               .Continuation);
            //
            // Add(MockedSanitizationTest("teststraat", "10", "1AP7")
            //               .Should().HaveCount(1)
            //               .And.First().Should().HaveHuisnummer("101").And.HaveAppnummer("7").And.HaveNoBusnummer()//NOTE:huisnummer is 101!
            //               .Continuation);
            // // busnummer
            // Add(MockedSanitizationTest("teststraat", "10", "BT7")//begint eigenlijk wel niet met cijfer (pas na toevoeging voorloop "0"-en)
            //               .Should().HaveCount(1)
            //               .And.First().Should().HaveHuisnummer("10").And.HaveNoAppnummer().And.HaveBusnummer("7")
            //               .Continuation);
            //
            // Add(MockedSanitizationTest("teststraat", "10", "8BT7")
            //               .Should().HaveCount(1)
            //               .And.First().Should().HaveHuisnummer("108").And.HaveNoAppnummer().And.HaveBusnummer("7")//NOTE:huisnummer is 108!
            //               .Continuation);
            //
            // Run();
        }

        [Fact]
        // RR-huisnummers met RR-index waarvan deel1 begint met een cijfer en met niet-numeriek deel 3 en zonder deel 4
        // Het RR-huisnummer+deel1 wordt een CRAB-huisnummer met numeriek bisnummer, deel1 van de RR-index wordt een CRAB-subadres van type busnummer.
        // Als deel2 een aanduiding is van een verdiepnummer wordt enkel het RR-huisnummer als huisnummer weggeschreven en worden deel1 het verdiepnummer van appartementnummer 0.
        // ***********************************************************************************************************************************************************************
        public void HouseNumberWithIndex_Part1StartNumber_Part3NonNumeric_Part4Missing()
        {
            // // verdiepnummer
            // Add(MockedSanitizationTest("teststraat", "10", "4E")
            //     .Should().HaveCount(1)
            //     .And.First().Should().HaveHuisnummer("10").And.HaveAppnummer("4.0").And.HaveNoBusnummer()
            //     .Continuation);
            //
            // Add(MockedSanitizationTest("teststraat", "10", "6.V")
            //     .Should().HaveCount(1)
            //     .And.First().Should().HaveHuisnummer("10").And.HaveAppnummer("6.0").And.HaveNoBusnummer()
            //     .Continuation);
            //
            // // geen verdiepnummer
            // Add(MockedSanitizationTest("teststraat", "10", "280I")
            //     .Should().HaveCount(1)
            //     .And.First().Should().HaveHuisnummer("10_280").And.HaveNoAppnummer().And.HaveBusnummer("I")
            //     .Continuation);
            //
            // Add(MockedSanitizationTest("teststraat", "10", "1a")
            //     .Should().HaveCount(1)
            //     .And.First().Should().HaveHuisnummer("10_1").And.HaveNoAppnummer().And.HaveBusnummer("a")
            //     .Continuation);
            //
            // Run();
        }
    }
}
