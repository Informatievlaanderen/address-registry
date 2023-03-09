namespace AddressRegistry.Tests.ApiTests.Oslo_Legacy.AddressMatch
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AddressRegistry.Api.Oslo.AddressMatch.Requests;
    using AddressRegistry.Projections.Legacy.AddressDetail;
    using Framework;
    using Framework.Assert;
    using Framework.Generate;
    using Framework.Mocking;
    using Xunit;
    using Xunit.Abstractions;

    public class AdresMatchSanitizationTests : BehavioralTestBase
    {
        private const string ExistingGemeentenaam = "The Shire";

        private readonly List<Action<int>> _matchActions = new List<Action<int>>();
        private readonly List<Task> _testContinuations = new List<Task>();
        private readonly Action<string> _arrangeStraatnaam;

        public AdresMatchSanitizationTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper, testOutputHelper.WriteLine)
        {
            string existingNisCode = Generate.NisCode.Generate(Random);

            Latest.ArrangeLatestGemeente(existingNisCode, ExistingGemeentenaam);

            _arrangeStraatnaam = straatnaam =>
            {
                var straatnaamId = Generate.Id.Generate(Random);
                Latest.ArrangeLatestStraatnaam(existingNisCode, ExistingGemeentenaam, straatnaamId.ToString(), straatnaam, Guid.NewGuid());
            };
        }

        [Fact]
        // huisnummer in straatnaam
        public void ParseHouseNumberFromStreetName_HouseNumberAsPrefix()
        {
            // huisnummer als prefix in straatnaam
            Add(MockedSanitizationTest("5 teststraat", null, null)
                .Should().HaveCount(1)
                .And.First().Should().HaveHuisnummer("5").And.HaveNoAppnummer().And.HaveNoBusnummer()
                .Continuation);

            Add(MockedSanitizationTest("10 teststraat", null, null)
                .Should().HaveCount(1)
                .And.First().Should().HaveHuisnummer("10").And.HaveNoAppnummer().And.HaveNoBusnummer()
                .Continuation);

            Add(MockedSanitizationTest("10, teststraat", null, null) // NOT SUPPORTED
                .Should().HaveCount(0).Continuation);

            Run();
        }

        [Fact]
        // huisnummer in straatnaam
        public void ParseHouseNumberFromStreetName_HouseNumberNonNumericBisNumberAsPrefix()
        {
            //huisnummer met niet - numeriek bisnummer als prefix in straatnaam
            var testAssertion1 = MockedSanitizationTest("4b teststraat", null, null).Should().HaveCount(2);
            Add(Task.WhenAll(
                testAssertion1.And.First().Should().HaveHuisnummer("4B").And.HaveNoBusnummer().And.HaveNoAppnummer().Continuation,
                testAssertion1.And.Second().Should().HaveHuisnummer("4").And.HaveNoBusnummer().And.HaveNoAppnummer().Continuation));

            var testAssertion2 = MockedSanitizationTest("10b teststraat", null, null).Should().HaveCount(2);
            Add(Task.WhenAll(
                testAssertion2.And.First().Should().HaveHuisnummer("10B").And.HaveNoBusnummer().And.HaveNoAppnummer().Continuation,
                testAssertion2.And.Second().Should().HaveHuisnummer("10").And.HaveNoBusnummer().And.HaveNoAppnummer().Continuation));

            var testAssertion3 = MockedSanitizationTest("10e teststraat", null, null).Should().HaveCount(2);
            Add(Task.WhenAll(
                testAssertion3.And.First().Should().HaveHuisnummer("10E").And.HaveNoBusnummer().And.HaveNoAppnummer().Continuation,
                testAssertion3.And.Second().Should().HaveHuisnummer("10").And.HaveNoBusnummer().And.HaveNoAppnummer().Continuation));

            var testAssertion4 = MockedSanitizationTest("10E teststraat", null, null).Should().HaveCount(2);
            Add(Task.WhenAll(
                testAssertion4.And.First().Should().HaveHuisnummer("10E").And.HaveNoBusnummer().And.HaveNoAppnummer().Continuation,
                testAssertion4.And.Second().Should().HaveHuisnummer("10").And.HaveNoBusnummer().And.HaveNoAppnummer().Continuation));

            Add(MockedSanitizationTest("10bis teststraat", null, null)
                .Should().HaveCount(0)
                .Continuation);

            Run();
        }

        [Fact]
        // huisnummer in straatnaam
        public void ParseHouseNumberFromStreetName_HouseNumberAsSuffix()
        {
            //huisnummer als suffix in straatnaam
            Add(MockedSanitizationTest("teststraat 5", null, null)
                .Should().HaveCount(1)
                .And.First().Should().HaveHuisnummer("5").And.HaveNoAppnummer().And.HaveNoBusnummer()
                .Continuation);

            Add(MockedSanitizationTest("teststraat 10", null, null)
                .Should().HaveCount(1)
                .And.First().Should().HaveHuisnummer("10").And.HaveNoAppnummer().And.HaveNoBusnummer()
                .Continuation);

            Add(MockedSanitizationTest("teststraat, 10", null, null)
                .Should().HaveCount(1)
                .And.First().Should().HaveHuisnummer("10").And.HaveNoAppnummer().And.HaveNoBusnummer()
                .Continuation);

            Add(MockedSanitizationTest("teststr. 10", null, null)
                .Should().HaveCount(1)
                .And.First().Should().HaveHuisnummer("10").And.HaveNoAppnummer().And.HaveNoBusnummer()
                .Continuation);

            Run();
        }

        [Fact]
        // huisnummer in straatnaam
        public void ParseHouseNumberFromStreetName_HouseNumberNonNumericBisNumberAsSuffix()
        {
            //huisnummer met niet-numeriek bisnummer als suffix in straatnaam
            var testAssertion1 = MockedSanitizationTest("teststraat 5b", null, null).Should().HaveCount(2);
            Add(Task.WhenAll(
                testAssertion1.And.First().Should().HaveHuisnummer("5B").And.HaveNoBusnummer().And.HaveNoAppnummer().Continuation,
                testAssertion1.And.Second().Should().HaveHuisnummer("5").And.HaveNoBusnummer().And.HaveNoAppnummer().Continuation));

            var testAssertion2 = MockedSanitizationTest("teststraat 5B", null, null).Should().HaveCount(2);
            Add(Task.WhenAll(
                testAssertion2.And.First().Should().HaveHuisnummer("5B").And.HaveNoBusnummer().And.HaveNoAppnummer().Continuation,
                testAssertion2.And.Second().Should().HaveHuisnummer("5").And.HaveNoBusnummer().And.HaveNoAppnummer().Continuation));

            var testAssertion3 = MockedSanitizationTest("teststraat 10b", null, null).Should().HaveCount(2);
            Add(Task.WhenAll(
                testAssertion3.And.First().Should().HaveHuisnummer("10B").And.HaveNoBusnummer().And.HaveNoAppnummer().Continuation,
                testAssertion3.And.Second().Should().HaveHuisnummer("10").And.HaveNoBusnummer().And.HaveNoAppnummer().Continuation));

            var testAssertion4 = MockedSanitizationTest("teststraat 10e", null, null).Should().HaveCount(2);
            Add(Task.WhenAll(
                testAssertion4.And.First().Should().HaveHuisnummer("10E").And.HaveNoBusnummer().And.HaveNoAppnummer().Continuation,
                testAssertion4.And.Second().Should().HaveHuisnummer("10").And.HaveNoBusnummer().And.HaveNoAppnummer().Continuation));

            var testAssertion5 = MockedSanitizationTest("teststraat 10E", null, null).Should().HaveCount(2);
            Add(Task.WhenAll(
                testAssertion5.And.First().Should().HaveHuisnummer("10E").And.HaveNoBusnummer().And.HaveNoAppnummer().Continuation,
                testAssertion5.And.Second().Should().HaveHuisnummer("10").And.HaveNoBusnummer().And.HaveNoAppnummer().Continuation));

            Add(MockedSanitizationTest("teststraat 10bis", null, null)
                .Should().HaveCount(0)
                .Continuation);

            //+ adres zonder 2de deel
            Add(MockedSanitizationTest("teststraat", "30+", null)
                .Should().HaveCount(1)
                .And.First().Should().HaveHuisnummer("30").And.HaveNoBusnummer().And.HaveNoAppnummer()
                .Continuation);

            Run();
        }

        [Fact]
        // huisnummer zonder RR-index
        // Het huisnummer wordt een CRAB-huisnummer. Er is geen CRAB-subadres.
        // *******************************************************************
        public void HouseNumberWithoutIndex_AllNumeric()
        {
            // volledig numerisch
            Add(MockedSanitizationTest("teststraat", "10", null)
                .Should().HaveCount(1)
                .And.First().Should().HaveHuisnummer("10").And.HaveNoAppnummer().And.HaveNoBusnummer()
                .Continuation);

            Run();
        }

        [Fact]
        // huisnummer zonder RR-index
        // Het huisnummer wordt een CRAB-huisnummer. Er is geen CRAB-subadres.
        // *******************************************************************
        public void HouseNumberWithoutIndex_SingleCharacter()
        {
            // niet-numeriek bisnummer: enkele letter
            var testAssertion1 = MockedSanitizationTest("teststraat", "10b", null).Should().HaveCount(2);
            Add(Task.WhenAll(
                testAssertion1.And.First().Should().HaveHuisnummer("10B").And.HaveNoBusnummer().And.HaveNoAppnummer().Continuation,
                testAssertion1.And.Second().Should().HaveHuisnummer("10").And.HaveNoBusnummer().And.HaveNoAppnummer().Continuation));

            var testAssertion2 = MockedSanitizationTest("teststraat", "10e", null).Should().HaveCount(2);
            Add(Task.WhenAll(
                testAssertion2.And.First().Should().HaveHuisnummer("10E").And.HaveNoBusnummer().And.HaveNoAppnummer().Continuation,
                testAssertion2.And.Second().Should().HaveHuisnummer("10").And.HaveNoBusnummer().And.HaveNoAppnummer().Continuation));

            //niet-numeriek bisnummer: enkele letter, gescheiden door spatie
            var testAssertion3 = MockedSanitizationTest("teststraat", "10 b", null).Should().HaveCount(2);
            Add(Task.WhenAll(
                testAssertion3.And.First().Should().HaveHuisnummer("10B").And.HaveNoBusnummer().And.HaveNoAppnummer().Continuation,
                testAssertion3.And.Second().Should().HaveHuisnummer("10").And.HaveNoBusnummer().And.HaveNoAppnummer().Continuation));

            Run();
        }

        [Fact]
        // huisnummer zonder RR-index
        // Het huisnummer wordt een CRAB-huisnummer. Er is geen CRAB-subadres.
        // *******************************************************************
        public void HouseNumberWithoutIndex_Bis()
        {
            // niet-numeriek bisnummer: 'bis'
            Add(MockedSanitizationTest("teststraat", "10bis", null)
                .Should().HaveCount(1)
                .And.First().Should().HaveHuisnummer("10BIS").And.HaveNoAppnummer().And.HaveNoBusnummer()
                .Continuation);

            Run();
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
            // niet-numeriek bisnummer
            Add(MockedSanitizationTest("teststraat", "10", "b")//of b000
               .Should().HaveCount(1)
               .And.First().Should().HaveHuisnummer("10b").And.HaveNoAppnummer().And.HaveNoBusnummer()
               .Continuation);

            // geen niet-numeriek bisnummer
            Add(MockedSanitizationTest("teststraat", "10", "BUS")//of BUS0 //busnummer => mogelijke varianten: "BUS", "bte", "BT", "bu", "bs"
               .Should().HaveCount(1)
               .And.First().Should().HaveHuisnummer("10").And.HaveNoAppnummer().And.HaveNoBusnummer()
               .Continuation);

            // appartementnummer 0 => null
            Add(MockedSanitizationTest("teststraat", "10", "RC")//of RC00 //verdiep => mogelijke varianten: "RC", "RCAR", "GV", "Rch", "RDC", "HAL", "rez", "RCH", "GV", "GVL", "GLV", "GEL", "GL"
               .Should().HaveCount(1)
               .And.First().Should().HaveHuisnummer("10").And.HaveAppnummer("0.0").And.HaveNoBusnummer()
               .Continuation);

            Add(MockedSanitizationTest("teststraat", "10", "gv")
               .Should().HaveCount(1)
               .And.First().Should().HaveHuisnummer("10").And.HaveAppnummer("0.0").And.HaveNoBusnummer()
               .Continuation);

            Add(MockedSanitizationTest("teststraat", "10", "et") //of et00 // verdiepnummer => mogelijke varianten: "et", "eta", "éta", "VER", "VDP", "VD", "Vr", "Vrd", "V", "Etg", "ét", "et", "ev", "eme", "ème", "STE", "de", "dev", "e", "E", "é", "links", "rechts"
               .Should().HaveCount(1)
               .And.First().Should().HaveHuisnummer("10").And.HaveNoAppnummer().And.HaveNoBusnummer()
               .Continuation);

            Add(MockedSanitizationTest("teststraat", "10", "E") //of et00 // verdiepnummer => mogelijke varianten: "et", "eta", "éta", "VER", "VDP", "VD", "Vr", "Vrd", "V", "Etg", "ét", "et", "ev", "eme", "ème", "STE", "de", "dev", "e", "E", "é", "links", "rechts"
                .Should().HaveCount(1)
                .And.First().Should().HaveHuisnummer("10").And.HaveNoAppnummer().And.HaveNoBusnummer()
                .Continuation);

            Add(MockedSanitizationTest("teststraat", "10", "E000")
               .Should().HaveCount(1)
               .And.First().Should().HaveHuisnummer("10").And.HaveNoAppnummer().And.HaveNoBusnummer()
               .Continuation);

            Add(MockedSanitizationTest("teststraat", "10", "E001")
               .Should().HaveCount(1)
               .And.First().Should().HaveHuisnummer("10").And.HaveAppnummer("E1").And.HaveNoBusnummer()
               .Continuation);

            Add(MockedSanitizationTest("teststraat", "10", "E01")
              .Should().HaveCount(1)
              .And.First().Should().HaveHuisnummer("10").And.HaveAppnummer("0.1").And.HaveNoBusnummer()
              .Continuation);

            Add(MockedSanitizationTest("teststraat", "10", "E00")
              .Should().HaveCount(1)
              .And.First().Should().HaveHuisnummer("10").And.HaveAppnummer("0.0").And.HaveNoBusnummer()
              .Continuation);

            Run();
        }

        [Fact]
        // huisnummers met RR-index waarvan deel1 niet-numeriek is en waarvan deel2 numeriek is en > 0
        // Het huisnummer wordt een CRAB-huisnummer, deel2 van de RR-index wordt een CRAB-subadres van type busnummer of appartementnummer.
        // Als deel1 een aanduiding is van een subadres wordt enkel het huisnummer als huisnummer weggeschreven.
        // *****************************************************************************************************
        // --Opm: index moet uit 4 karakters bestaan of anders worden er voorloop "0"-en aan geplakt en komen we in een ander code-path
        public void HouseNumberWithIndex_Part1NonNumeric_Part2NumericNonZero()
        {
            // niet-numeriek bisnummer
            Add(MockedSanitizationTest("teststraat", "10", "b028")
              .Should().HaveCount(1)
              .And.First().Should().HaveHuisnummer("10b").And.HaveNoAppnummer().And.HaveBusnummer("28")
              .Continuation);

            Add(MockedSanitizationTest("teststraat", "10", "a4")
              .Should().HaveCount(1)
              .And.First().Should().HaveHuisnummer("10a").And.HaveNoAppnummer().And.HaveBusnummer("4")
              .Continuation);

            Add(MockedSanitizationTest("teststraat", "10", "B  3")
              .Should().HaveCount(1)
              .And.First().Should().HaveHuisnummer("10B").And.HaveNoAppnummer().And.HaveBusnummer("3")
              .Continuation);
            //geen niet-numeriek bisnummer
            Add(MockedSanitizationTest("teststraat", "10", "BUS8")//busnummer => mogelijke varianten: "BUS", "bte", "BT", "bu", "bs"
              .Should().HaveCount(1)
              .And.First().Should().HaveHuisnummer("10").And.HaveNoAppnummer().And.HaveBusnummer("8")
              .Continuation);

            Add(MockedSanitizationTest("teststraat", "10", "BU8")//busnummer => mogelijke varianten: "BUS", "bte", "BT", "bu", "bs"
              .Should().HaveCount(1)
              .And.First().Should().HaveHuisnummer("10").And.HaveNoAppnummer().And.HaveBusnummer("8")
              .Continuation);

            Add(MockedSanitizationTest("teststraat", "10", "Bt8")//busnummer => mogelijke varianten: "BUS", "bte", "BT", "bu", "bs"
              .Should().HaveCount(1)
              .And.First().Should().HaveHuisnummer("10").And.HaveNoAppnummer().And.HaveBusnummer("8")
              .Continuation);

            Add(MockedSanitizationTest("teststraat", "10", "BS8")//busnummer => mogelijke varianten: "BUS", "bte", "BT", "bu", "bs"
              .Should().HaveCount(1)
              .And.First().Should().HaveHuisnummer("10").And.HaveNoAppnummer().And.HaveBusnummer("8")
              .Continuation);

            Add(MockedSanitizationTest("teststraat", "10", "RC08")//verdiep => mogelijke varianten: "RC", "RCAR", "GV", "Rch", "RDC", "HAL", "rez", "RCH", "GV", "GVL", "GLV", "GEL", "GL"
              .Should().HaveCount(1)
              .And.First().Should().HaveHuisnummer("10").And.HaveAppnummer("RC8").And.HaveNoBusnummer()
              .Continuation);

            Add(MockedSanitizationTest("teststraat", "10", "et08")//verdiepnummer => mogelijke varianten: //verdiepnummer => mogelijke varianten: "et", "eta", "éta", "VER", "VDP", "VD", "Vr", "Vrd", "V", "Etg", "ét", "et", "ev", "eme", "ème", "STE", "de", "dev", "e", "E", "é", "links", "rechts"
              .Should().HaveCount(1)
              .And.First().Should().HaveHuisnummer("10").And.HaveAppnummer("et8").And.HaveNoBusnummer()
              .Continuation);

            Run();
        }

        [Fact]
        // huisnummers met RR-index waarvan deel1 begint met een cijfer en zonder deel 3.
        // Het huisnummer wordt een CRAB-huisnummer, deel1 van de RR-index wordt een CRAB-subadres van type busnummer.
        // ***********************************************************************************************************
        public void HouseNumberWithIndex_Part1StartNumber_Part3Missing()
        {
            Add(MockedSanitizationTest("teststraat", "10", "28")
                .Should().HaveCount(1)
                .And.First().Should().HaveHuisnummer("10").And.HaveNoAppnummer().And.HaveBusnummer("28")
                .Continuation);

            Add(MockedSanitizationTest("teststraat", "10", " 666")
                .Should().HaveCount(1)
                .And.First().Should().HaveHuisnummer("10").And.HaveNoAppnummer().And.HaveBusnummer("666")
                .Continuation);

            Add(MockedSanitizationTest("teststraat", "10", "1234")
                .Should().HaveCount(1)
                .And.First().Should().HaveHuisnummer("10").And.HaveNoAppnummer().And.HaveBusnummer("1234")
                .Continuation);

            Run();
        }

        [Fact]
        // huisnummers met RR-index waarvan deel1 begint met een cijfer en met numeriek deel 3 en zonder deel 4
        // Het huisnummer wordt een CRAB-huisnummer, de RR-index wordt een CRAB-subadres van type appartementnummer.
        // *********************************************************************************************************
        public void HouseNumberWithIndex_Part1StartNumber_Part3Numeric_Part4Missing()
        {
            Add(MockedSanitizationTest("teststraat", "10", "2/8")
                .Should().HaveCount(1)
                .And.First().Should().HaveHuisnummer("10").And.HaveAppnummer("2/8").And.HaveNoBusnummer()
                .Continuation);

            Add(MockedSanitizationTest("teststraat", "10", "-8")//begint eigenlijk wel niet met cijfer (pas na toevoeging voorloop "0"-en)
                .Should().HaveCount(1)
                .And.First().Should().HaveHuisnummer("10").And.HaveAppnummer("-8").And.HaveNoBusnummer()
                .Continuation);

            Add(MockedSanitizationTest("teststraat", "10", "1.02")
                .Should().HaveCount(1)
                .And.First().Should().HaveHuisnummer("10").And.HaveAppnummer("1.02").And.HaveNoBusnummer()
                .Continuation);

            Add(MockedSanitizationTest("teststraat", "10", "0.1")
                .Should().HaveCount(1)
                .And.First().Should().HaveHuisnummer("10").And.HaveAppnummer("0.1").And.HaveNoBusnummer()
                .Continuation);

            Run();
        }

        [Fact]
        // huisnummers met RR-index waarvan deel1 begint met een cijfer en met niet-numeriek deel 3 en numeriek deel 4
        // Het huisnummer + deel1 wordt een CRAB-huisnummer met een numeriek bisnummer, deel4 van de RR-index wordt een CRAB-subadres van type busnummer of appartementnummer.
        // Als deel3 een aanduiding is van een verdiepnummer wordt enkel het RR-huisnummer als huisnummer weggeschreven en worden deel1 en deel4 samengevoegd tot een appartementnummer.
        // *****************************************************************************************************************************************************************************
        public void HouseNumberWithIndex_Part1StartNumber_Part3NonNumeric_Part4Numeric()
        {
            // niet-numeriek bisnummer
            Add(MockedSanitizationTest("teststraat", "10", "11b0")
                          .Should().HaveCount(1)
                          .And.First().Should().HaveHuisnummer("10b").And.HaveNoAppnummer().And.HaveBusnummer("0")
                          .Continuation);

            Add(MockedSanitizationTest("teststraat", "10", "1b01")
                         .Should().HaveCount(1)
                         .And.First().Should().HaveHuisnummer("10b").And.HaveNoAppnummer().And.HaveBusnummer("1")
                         .Continuation);

            Add(MockedSanitizationTest("teststraat", "10", "A/b7")
                         .Should().HaveCount(1)
                         .And.First().Should().HaveHuisnummer("10b").And.HaveNoAppnummer().And.HaveBusnummer("7")
                         .Continuation);

            // verdiepnummer
            Add(MockedSanitizationTest("teststraat", "10", "2et1")
                          .Should().HaveCount(1)
                          .And.First().Should().HaveHuisnummer("10").And.HaveAppnummer("2.1").And.HaveNoBusnummer()
                          .Continuation);

            Add(MockedSanitizationTest("teststraat", "10", "6,E3")
                          .Should().HaveCount(1)
                          .And.First().Should().HaveHuisnummer("10").And.HaveAppnummer("6.3").And.HaveNoBusnummer()
                          .Continuation);

            Add(MockedSanitizationTest("teststraat", "10", "E6")//begint eigenlijk wel niet met cijfer (pas na toevoeging voorloop "0"-en)
                          .Should().HaveCount(1)
                          .And.First().Should().HaveHuisnummer("10").And.HaveAppnummer("0.6").And.HaveNoBusnummer()
                          .Continuation);

            // appartementnummer
            Add(MockedSanitizationTest("teststraat", "10", "AP7")//begint eigenlijk wel niet met cijfer (pas na toevoeging voorloop "0"-en)
                          .Should().HaveCount(1)
                          .And.First().Should().HaveHuisnummer("10").And.HaveAppnummer("7").And.HaveNoBusnummer()
                          .Continuation);

            Add(MockedSanitizationTest("teststraat", "10", "1AP7")
                          .Should().HaveCount(1)
                          .And.First().Should().HaveHuisnummer("101").And.HaveAppnummer("7").And.HaveNoBusnummer()//NOTE:huisnummer is 101!
                          .Continuation);
            // busnummer
            Add(MockedSanitizationTest("teststraat", "10", "BT7")//begint eigenlijk wel niet met cijfer (pas na toevoeging voorloop "0"-en)
                          .Should().HaveCount(1)
                          .And.First().Should().HaveHuisnummer("10").And.HaveNoAppnummer().And.HaveBusnummer("7")
                          .Continuation);

            Add(MockedSanitizationTest("teststraat", "10", "8BT7")
                          .Should().HaveCount(1)
                          .And.First().Should().HaveHuisnummer("108").And.HaveNoAppnummer().And.HaveBusnummer("7")//NOTE:huisnummer is 108!
                          .Continuation);

            Run();
        }

        [Fact]
        // RR-huisnummers met RR-index waarvan deel1 begint met een cijfer en met niet-numeriek deel 3 en zonder deel 4
        // Het RR-huisnummer+deel1 wordt een CRAB-huisnummer met numeriek bisnummer, deel1 van de RR-index wordt een CRAB-subadres van type busnummer.
        // Als deel2 een aanduiding is van een verdiepnummer wordt enkel het RR-huisnummer als huisnummer weggeschreven en worden deel1 het verdiepnummer van appartementnummer 0.
        // ***********************************************************************************************************************************************************************
        public void HouseNumberWithIndex_Part1StartNumber_Part3NonNumeric_Part4Missing()
        {
            // verdiepnummer
            Add(MockedSanitizationTest("teststraat", "10", "4E")
                .Should().HaveCount(1)
                .And.First().Should().HaveHuisnummer("10").And.HaveAppnummer("4.0").And.HaveNoBusnummer()
                .Continuation);

            Add(MockedSanitizationTest("teststraat", "10", "6.V")
                .Should().HaveCount(1)
                .And.First().Should().HaveHuisnummer("10").And.HaveAppnummer("6.0").And.HaveNoBusnummer()
                .Continuation);

            // geen verdiepnummer
            Add(MockedSanitizationTest("teststraat", "10", "280I")
                .Should().HaveCount(1)
                .And.First().Should().HaveHuisnummer("10_280").And.HaveNoAppnummer().And.HaveBusnummer("I")
                .Continuation);

            Add(MockedSanitizationTest("teststraat", "10", "1a")
                .Should().HaveCount(1)
                .And.First().Should().HaveHuisnummer("10_1").And.HaveNoAppnummer().And.HaveBusnummer("a")
                .Continuation);

            Run();
        }

        [Fact]
        // detecteer bereikachtige records
        // *******************************
        public void HouseNumberRanges()
        {
            Add(MockedSanitizationTest("teststraat", "30/20", "links")
                .Should().HaveCount(1)
                .And.First().Should().HaveHuisnummer("30").And.HaveBusnummer("20").And.HaveNoAppnummer()
                .Continuation);

            Add(MockedSanitizationTest("teststraat", "30/20", null)
                .Should().HaveCount(1)
                .And.First().Should().HaveHuisnummer("30").And.HaveBusnummer("20").And.HaveNoAppnummer()
                .Continuation);

            Add(MockedSanitizationTest("teststraat", "30-20", null)
               .Should().HaveCount(1)
               .And.First().Should().HaveHuisnummer("30").And.HaveBusnummer("20").And.HaveNoAppnummer()
               .Continuation);

            Add(MockedSanitizationTest("teststraat", "30+20", null)
                .Should().HaveCount(1)
                .And.First().Should().HaveHuisnummer("30").And.HaveBusnummer("20").And.HaveNoAppnummer()
                .Continuation);

            Add(MockedSanitizationTest("teststraat", "30 20", null)
                .Should().HaveCount(1)
                .And.First().Should().HaveHuisnummer("30").And.HaveBusnummer("20").And.HaveNoAppnummer()
                .Continuation);

            AddHouseNumberRangeCheck(MockedSanitizationTest("teststraat", "20/30", null), new[] { "20", "22", "24", "26", "28", "30" });
            AddHouseNumberRangeCheck(MockedSanitizationTest("teststraat", "20+30", null), new[] { "20", "22", "24", "26", "28", "30" });
            AddHouseNumberRangeCheck(MockedSanitizationTest("teststraat", "20-30", null), new[] { "20", "22", "24", "26", "28", "30" });
            AddHouseNumberRangeCheck(MockedSanitizationTest("teststraat", "20 30", null), new[] { "20", "22", "24", "26", "28", "30" });
            AddHouseNumberRangeCheck(MockedSanitizationTest("teststraat20-30", null, null), new[] { "20", "22", "24", "26", "28", "30" });

            AddHouseNumberRangeCheck(MockedSanitizationTest("teststraat", "20-26", null), new[] { "20", "22", "24", "26" });
            AddHouseNumberRangeCheck(MockedSanitizationTest("teststraat", "21-27", null), new[] { "21", "23", "25", "27" });
            AddHouseNumberRangeCheck(MockedSanitizationTest("teststraat", "20-25", null), new[] { "20", "21", "22", "23", "24", "25" });
            AddHouseNumberRangeCheck(MockedSanitizationTest("teststraat", "21-24", null), new[] { "21", "22", "23", "24" });

            // niet-numeriek bisnummer genoteerd als bereik
            Add(MockedSanitizationTest("teststraat", "30-b", null)
               .Should().HaveCount(1)
               .And.First().Should().HaveHuisnummer("30").And.HaveBusnummer("B").And.HaveNoAppnummer()
               .Continuation);

            Add(MockedSanitizationTest("teststraat", "30-", null)
               .Should().HaveCount(1)
               .And.First().Should().HaveHuisnummer("30").And.HaveNoBusnummer().And.HaveNoAppnummer()
               .Continuation);

            Run();
        }

        private void AddHouseNumberRangeCheck(Task<List<AdresListFilterStub>> test, IReadOnlyCollection<string> expectedHouseNumbers)
        {
            var testAssertion = test.Should().HaveCount(expectedHouseNumbers.Count);

            Add(Task.WhenAll(
                expectedHouseNumbers
                    .Select((t, i) =>
                        testAssertion.And.Element(i).Should()
                            .HaveHuisnummer(t).And
                            .HaveNoBusnummer().And
                            .HaveNoAppnummer().Continuation)
                    .Cast<Task>()
                    .ToList()));
        }

        [Fact]
        public void ExtraSanitizeCases()
        {
            // last fallback
            Add(MockedSanitizationTest("teststraat", "naast nr 30", null)
               .Should().HaveCount(1)
               .And.First().Should().HaveHuisnummer("30").And.HaveNoBusnummer().And.HaveNoAppnummer()
               .Continuation);

            // extra
            Add(MockedSanitizationTest("teststraat", "0067", "1;3")
               .Should().HaveCount(1)
               .And.First().Should().HaveHuisnummer("67").And.HaveNoBusnummer().And.HaveAppnummer("1;3")
               .Continuation);

            // additional tests
            Add(MockedSanitizationTest("teststraat", "10", "-800")
              .Should().HaveCount(0)
              .Continuation);

            // fix gawr-3884, empty streetname after StripStreetName
            Add(MockedSanitizationTest("00", null, null)
                .Should().HaveCount(0)
                .Continuation);

            Run();
        }

        private void Run(params int[] testsToExecute)
        {
            Log($"******** Test run: {DateTime.Now:yyyy-MM-dd hh:mm:ss} ********");

            if (!testsToExecute.Any())
            {
                var counter = 1;
                foreach (var c in _testContinuations)
                    RunTestAndWaitForAssertionContinuation(counter++);
            }
            else
            {
                testsToExecute.ToList().ForEach(RunTestAndWaitForAssertionContinuation);
            }
        }

        private void RunTestAndWaitForAssertionContinuation(int testNumber)
        {
            try
            {
                _matchActions[testNumber - 1](testNumber);
                _testContinuations[testNumber - 1].GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                var inner = e;
                while (inner.InnerException != null)
                    inner = inner.InnerException;

                Log($"TEST FAILED: {inner.Message}");
                throw;
            }
        }

        private void Add(Task testExecution)
            => _testContinuations.Add(testExecution);

        private Task<List<AdresListFilterStub>> MockedSanitizationTest(string straatnaam, string? huisNummer, string? index)
        {
            var promise = new TaskCompletionSource<List<AdresListFilterStub>>();

            void SanitizeAction(int i)
            {
                var request = new AddressMatchRequest
                {
                    Gemeentenaam = ExistingGemeentenaam,
                    Straatnaam = straatnaam,
                    Huisnummer = huisNummer,
                    Index = index
                };

                try
                {
                    Log($"--------------------- TEST NR{i} ---------------------");
                    Log($"INPUT:\r\n-straatnaam : {straatnaam ?? "<null>"}\r\n-huisNummer : {huisNummer ?? "<null>"}\r\n-index : {index ?? "<null>"}");

                    _arrangeStraatnaam(straatnaam);

                    var filters = new List<AdresListFilterStub>();

                    Latest.Arrange(
                        Produce.EmptyList<AddressDetailItem>(),
                        (when, x) =>
                            when.InterceptingLatestAdresQuery(
                                (straatnaamid, huisnr, busnr) => filters.Add(new AdresListFilterStub
                                {
                                    HuisnummerFilter = huisnr,
                                    BusnummerFilter = busnr

                                }), x));

                    var _ = Send(request).GetAwaiter().GetResult();

                    if (filters.Any())
                    {
                        Log("RESULTS:");
                        filters.ForEach(result => Log($"\t***\r\n\tHuisNummer : {result.HuisnummerFilter ?? "<null>"}\r\n\tAppNummer : NOT IMPLEMENTED, WHAT TODO WITH APPNUMMER?\r\n\tBusNummer : {result.BusnummerFilter ?? "<null>"}"));
                    }
                    else
                    {
                        Log("NO RESULTS");
                    }

                    promise.SetResult(filters);
                }
                catch (Exception e)
                {
                    Log($"Exception\r\n{e}");
                    promise.SetException(e);
                }
            }

            _matchActions.Add(SanitizeAction);

            return promise.Task;
        }
    }

    public class AdresListFilterStub
    {
        public string BusnummerFilter { get; set; }

        public string GemeentenaamFilter { get; set; }

        public string HomoniemToevoegingFilter { get; set; }

        public string HuisnummerFilter { get; set; }

        public string PostcodeFilter { get; set; }

        public string StraatnaamFilter { get; set; }
    }
}
