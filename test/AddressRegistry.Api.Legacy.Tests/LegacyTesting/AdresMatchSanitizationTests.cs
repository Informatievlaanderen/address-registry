namespace AddressRegistry.Api.Legacy.Tests.LegacyTesting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AddressMatch.Requests;
    using AddressMatch.Responses;
    using Assert;
    using Generate;
    using Microsoft.AspNetCore.Mvc;
    using Mocking;
    using Projections.Legacy.AddressDetail;
    using Xunit;
    using Xunit.Abstractions;

    public class AdresMatchSanitizationTests : BehavioralTestBase
    {
        private List<Action<int>> matchActions = new List<Action<int>>();
        private List<Task> testContinuations = new List<Task>();
        private Action<string> _arrangeStraatnaam = m => { };
        private const string existingGemeentenaam = "The Shire";

        public AdresMatchSanitizationTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper, testOutputHelper.WriteLine)
        { }

        [Fact]
        public void CanSanitizeHouseNumberAndRrIndex()
        {
            string existingNisCode = Generate.Generate.NISCode.Generate(_random);

            _latest.ArrangeLatestGemeente(existingNisCode, existingGemeentenaam);

            _arrangeStraatnaam = straatnaam =>
            {
                int straatnaamId = Generate.Generate.Id.Generate(_random);
                _latest.ArrangeLatestStraatnaam(existingNisCode, existingGemeentenaam, straatnaamId.ToString(), straatnaam);
            };

            /* huisnummer in straatnaam */
            //huisnummer als prefix in straatnaam

            Add(MockedSanitizationTest("10 teststraat", null, null)
                .Should().HaveCount(1)
                .And.First().Should().HaveHuisnummer("10").And.HaveNoAppnummer().And.HaveNoBusnummer()
                .Continuation);
            Add(MockedSanitizationTest("10, teststraat", null, null)//NOT SUPPORTED
                .Should().ThrowException()
                .Continuation);
            //huisnummer met niet-numeriek bisnummer als prefix in straatnaam
            Add(MockedSanitizationTest("10b teststraat", null, null)
                .Should().HaveCount(1)
                .And.First().Should().HaveHuisnummer("10B").And.HaveNoAppnummer().And.HaveNoBusnummer()
                .Continuation);
            Add(MockedSanitizationTest("10e teststraat", null, null)//special case, "e" niet ondersteund als niet-numeriek bisnummer
                .Should().HaveCount(0)
                .Continuation);
            Add(MockedSanitizationTest("10bis teststraat", null, null)
                .Should().HaveCount(0)
                .Continuation);
            //huisnummer als suffix in straatnaam
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
            //huisnummer met niet-numeriek bisnummer als suffix in straatnaam
            Add(MockedSanitizationTest("teststraat 10b", null, null)
                .Should().HaveCount(1)
                .And.First().Should().HaveHuisnummer("10B").And.HaveNoAppnummer().And.HaveNoBusnummer()
                .Continuation);
            Add(MockedSanitizationTest("teststraat 10bis", null, null)
                .Should().HaveCount(0)
                .Continuation);

            //+ adres zonder 2de deel
            Add(MockedSanitizationTest("teststraat", "30+", null)
                .Should().HaveCount(1)
                .And.First().Should().HaveHuisnummer("30").And.HaveNoBusnummer().And.HaveNoAppnummer()
                .Continuation);

            /* huisnummer zonder RR-index */
            //volledig numerisch
            Add(MockedSanitizationTest("teststraat", "10", null)
                .Should().HaveCount(1)
                .And.First().Should().HaveHuisnummer("10").And.HaveNoAppnummer().And.HaveNoBusnummer()
                .Continuation);
            //niet-numeriek bisnummer: enkele letter
            Add(MockedSanitizationTest("teststraat", "10b", null)
                .Should().HaveCount(1)
                .And.First().Should().HaveHuisnummer("10B").And.HaveNoAppnummer().And.HaveNoBusnummer()
                .Continuation);
            Add(MockedSanitizationTest("teststraat", "10e", null)//hier is e wel ondersteund als niet-numeriek bisnummer
                .Should().HaveCount(1)
                .And.First().Should().HaveHuisnummer("10E").And.HaveNoAppnummer().And.HaveNoBusnummer()
                .Continuation);
            //niet-numeriek bisnummer: enkele letter, gescheiden door spatie
            Add(MockedSanitizationTest("teststraat", "10 b", null)
                .Should().HaveCount(1)
                .And.First().Should().HaveHuisnummer("10B").And.HaveNoAppnummer().And.HaveNoBusnummer()
                .Continuation);
            //niet-numeriek bisnummer: 'bis'
            Add(MockedSanitizationTest("teststraat", "10bis", null)
                .Should().HaveCount(1)
                .And.First().Should().HaveHuisnummer("10BIS").And.HaveNoAppnummer().And.HaveNoBusnummer()
                .Continuation);

            /* huisnummers met RR-index waarvan deel1 niet-numeriek is en waarvan deel2 numeriek is en = 0 */
            /* Het huisnummer + deel1 wordt een CRAB-huisnummer met een niet-numeriek bisnummer. Er is geen CRAB-subadres.
               Als deel1 een aanduiding is van een subadres wordt enkel het RR-huisnummer als huisnummer weggeschreven.
               Als deel1 een aanduiding is van een verdiep wordt enkel het RR-huisnummer als huisnummer weggeschreven en een subadres 0.0 met aard appartementnummer. */
            //--Opm: index moet uit 4 karakters bestaan of mag enkel niet-numerieke karakters bevatten anders worden er voorloop "0"-en aan geplakt en komen we in een ander code-path
            //niet-numeriek bisnummer
            Add(MockedSanitizationTest("teststraat", "10", "b")//of b000
               .Should().HaveCount(1)
               .And.First().Should().HaveHuisnummer("10b").And.HaveNoAppnummer().And.HaveNoBusnummer()
               .Continuation);
            //geen niet-numeriek bisnummer
            Add(MockedSanitizationTest("teststraat", "10", "BUS")//of BUS0 //busnummer => mogelijke varianten: "BUS", "bte", "BT", "bu", "bs"
               .Should().HaveCount(1)
               .And.First().Should().HaveHuisnummer("10").And.HaveNoAppnummer().And.HaveNoBusnummer()
               .Continuation);
            //appartementnummer 0 => null
            Add(MockedSanitizationTest("teststraat", "10", "RC")//of RC00 //verdiep => mogelijke varianten: "RC", "RCAR", "GV", "Rch", "RDC", "HAL", "rez", "RCH", "GV", "GVL", "GLV", "GEL", "GL"
               .Should().HaveCount(1)
               .And.First().Should().HaveHuisnummer("10").And.HaveAppnummer("0.0").And.HaveNoBusnummer()
               .Continuation);
            Add(MockedSanitizationTest("teststraat", "10", "gv")
               .Should().HaveCount(1)
               .And.First().Should().HaveHuisnummer("10").And.HaveAppnummer("0.0").And.HaveNoBusnummer()
               .Continuation);
            Add(MockedSanitizationTest("teststraat", "10", "et")//of et00 //verdiepnummer => mogelijke varianten: "et", "eta", "éta", "VER", "VDP", "VD", "Vr", "Vrd", "V", "Etg", "ét", "et", "ev", "eme", "ème", "STE", "de", "dev", "e", "E", "é", "links", "rechts"
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

            /* huisnummers met RR-index waarvan deel1 niet-numeriek is en waarvan deel2 numeriek is en > 0 */
            /* Het huisnummer wordt een CRAB-huisnummer, deel2 van de RR-index wordt een CRAB-subadres van type busnummer of appartementnummer.
               Als deel1 een aanduiding is van een subadres wordt enkel het huisnummer als huisnummer weggeschreven. */
            //--Opm: index moet uit 4 karakters bestaan of anders worden er voorloop "0"-en aan geplakt en komen we in een ander code-path
            //niet-numeriek bisnummer
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

            /* huisnummers met RR-index waarvan deel1 begint met een cijfer en zonder deel 3. */
            /* Het huisnummer wordt een CRAB-huisnummer, deel1 van de RR-index wordt een CRAB-subadres van type busnummer.*/
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

            /* huisnummers met RR-index waarvan deel1 begint met een cijfer en met numeriek deel 3 en zonder deel 4 */
            /* Het huisnummer wordt een CRAB-huisnummer, de RR-index wordt een CRAB-subadres van type appartementnummer. */
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

            /* huisnummers met RR-index waarvan deel1 begint met een cijfer en met niet-numeriek deel 3 en numeriek deel 4 */
            /* Het huisnummer + deel1 wordt een CRAB-huisnummer met een numeriek bisnummer, deel4 van de RR-index wordt een CRAB-subadres van type busnummer of appartementnummer.
               Als deel3 een aanduiding is van een verdiepnummer wordt enkel het RR-huisnummer als huisnummer weggeschreven en worden deel1 en deel4 samengevoegd tot een appartementnummer.*/
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

            /* RR-huisnummers met RR-index waarvan deel1 begint met een cijfer en met niet-numeriek deel 3 en zonder deel 4
	           Het RR-huisnummer+deel1 wordt een CRAB-huisnummer met numeriek bisnummer, deel1 van de RR-index wordt een CRAB-subadres van type busnummer.
	           Als deel2 een aanduiding is van een verdiepnummer wordt enkel het RR-huisnummer als huisnummer weggeschreven en worden deel1 het verdiepnummer van appartementnummer 0.*/
            // verdiepnummer
            Add(MockedSanitizationTest("teststraat", "10", "4E")
                          .Should().HaveCount(1)
                          .And.First().Should().HaveHuisnummer("10").And.HaveAppnummer("4.0").And.HaveNoBusnummer()
                          .Continuation);
            Add(MockedSanitizationTest("teststraat", "10", "6.V")
                          .Should().HaveCount(1)
                          .And.First().Should().HaveHuisnummer("10").And.HaveAppnummer("6.0").And.HaveNoBusnummer()
                          .Continuation);
            //geen verdiepnummer
            Add(MockedSanitizationTest("teststraat", "10", "280I")
                          .Should().HaveCount(1)
                          .And.First().Should().HaveHuisnummer("10_280").And.HaveNoAppnummer().And.HaveBusnummer("I")
                          .Continuation);
            Add(MockedSanitizationTest("teststraat", "10", "1a")
                         .Should().HaveCount(1)
                         .And.First().Should().HaveHuisnummer("10_1").And.HaveNoAppnummer().And.HaveBusnummer("a")
                         .Continuation);

            //bereiken
            Add(MockedSanitizationTest("teststraat", "30/20", "links")
                .Should().HaveCount(1)
                .And.First().Should().HaveHuisnummer("30").And.HaveBusnummer("20").And.HaveNoAppnummer()
                .Continuation);
            Add(MockedSanitizationTest("teststraat", "30/20", null)
                .Should().HaveCount(1)
                .And.First().Should().HaveHuisnummer("30").And.HaveBusnummer("20").And.HaveNoAppnummer()
                .Continuation);
            {
                var testAssertion = MockedSanitizationTest("teststraat", "20/30", null).Should().HaveCount(2);
                var testSubAssertionFirst = testAssertion.And.First().Should().HaveHuisnummer("20").And.HaveNoBusnummer().And.HaveNoAppnummer().Continuation;
                var testSubAssertionSecond = testAssertion.And.Second().Should().HaveHuisnummer("30").And.HaveNoBusnummer().And.HaveNoAppnummer().Continuation;
                Add(Task.WhenAll(testSubAssertionFirst, testSubAssertionSecond));
            }
            Add(MockedSanitizationTest("teststraat", "30-20", null)
               .Should().HaveCount(1)
               .And.First().Should().HaveHuisnummer("30").And.HaveBusnummer("20").And.HaveNoAppnummer()
               .Continuation);
            {
                var testAssertion = MockedSanitizationTest("teststraat", "20-30", null).Should().HaveCount(2);
                var testSubAssertionFirst = testAssertion.And.First().Should().HaveHuisnummer("20").And.HaveNoBusnummer().And.HaveNoAppnummer().Continuation;
                var testSubAssertionSecond = testAssertion.And.Second().Should().HaveHuisnummer("30").And.HaveNoBusnummer().And.HaveNoAppnummer().Continuation;
                Add(Task.WhenAll(testSubAssertionFirst, testSubAssertionSecond));
            }
            Add(MockedSanitizationTest("teststraat", "30+20", null)
               .Should().HaveCount(1)
               .And.First().Should().HaveHuisnummer("30").And.HaveBusnummer("20").And.HaveNoAppnummer()
               .Continuation);
            {
                var testAssertion = MockedSanitizationTest("teststraat", "20+30", null).Should().HaveCount(2);
                var testSubAssertionFirst = testAssertion.And.First().Should().HaveHuisnummer("20").And.HaveNoBusnummer().And.HaveNoAppnummer().Continuation;
                var testSubAssertionSecond = testAssertion.And.Second().Should().HaveHuisnummer("30").And.HaveNoBusnummer().And.HaveNoAppnummer().Continuation;
                Add(Task.WhenAll(testSubAssertionFirst, testSubAssertionSecond));
            }
            Add(MockedSanitizationTest("teststraat", "30 20", null)
               .Should().HaveCount(1)
               .And.First().Should().HaveHuisnummer("30").And.HaveBusnummer("20").And.HaveNoAppnummer()
               .Continuation);
            {
                var testAssertion = MockedSanitizationTest("teststraat", "20 30", null).Should().HaveCount(2);
                var testSubAssertionFirst = testAssertion.And.First().Should().HaveHuisnummer("20").And.HaveNoBusnummer().And.HaveNoAppnummer().Continuation;
                var testSubAssertionSecond = testAssertion.And.Second().Should().HaveHuisnummer("30").And.HaveNoBusnummer().And.HaveNoAppnummer().Continuation;
                Add(Task.WhenAll(testSubAssertionFirst, testSubAssertionSecond));
            }
            {
                var testAssertion = MockedSanitizationTest("teststraat20-30", null, null).Should().HaveCount(2);
                var testSubAssertionFirst = testAssertion.And.First().Should().HaveHuisnummer("20").And.HaveNoBusnummer().And.HaveNoAppnummer().Continuation;
                var testSubAssertionSecond = testAssertion.And.Second().Should().HaveHuisnummer("30").And.HaveNoBusnummer().And.HaveNoAppnummer().Continuation;
                Add(Task.WhenAll(testSubAssertionFirst, testSubAssertionSecond));
            }
            // niet-numeriek bisnummer genoteerd als bereik
            Add(MockedSanitizationTest("teststraat", "30-b", null)
               .Should().HaveCount(1)
               .And.First().Should().HaveHuisnummer("30").And.HaveBusnummer("B").And.HaveNoAppnummer()
               .Continuation);
            Add(MockedSanitizationTest("teststraat", "30-", null)
               .Should().HaveCount(1)
               .And.First().Should().HaveHuisnummer("30").And.HaveNoBusnummer().And.HaveNoAppnummer()
               .Continuation);
            //last fallback
            Add(MockedSanitizationTest("teststraat", "naast nr 30", null)
               .Should().HaveCount(1)
               .And.First().Should().HaveHuisnummer("30").And.HaveNoBusnummer().And.HaveNoAppnummer()
               .Continuation);

            //extra
            Add(MockedSanitizationTest("teststraat", "0067", "1;3")
               .Should().HaveCount(1)
               .And.First().Should().HaveHuisnummer("67").And.HaveNoBusnummer().And.HaveAppnummer("1;3")
               .Continuation);

            //additional tests
            Add(MockedSanitizationTest("teststraat", "10", "-800")
              .Should().HaveCount(0)
              .Continuation);
            Run();
        }

        private void Run(params int[] testsToExecute)
        {
            Log($"******** Test run: {DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")} ********");

            if (!testsToExecute.Any())
            {
                int counter = 1;
                foreach (var c in testContinuations)
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
                matchActions[testNumber - 1](testNumber);
                testContinuations[testNumber - 1].Wait();
            }
            catch (Exception e)
            {
                Exception inner = e;
                while (inner.InnerException != null)
                    inner = inner.InnerException;
                Log($"TEST FAILED: {inner.Message}");
                throw;
            }
        }

        private void Add(Task testExecution)
        {
            testContinuations.Add(testExecution);
        }

        private Task<List<AdresListFilterStub>> MockedSanitizationTest(string straatnaam, string huisNummer, string index)
        {
            TaskCompletionSource<List<AdresListFilterStub>> promise = new TaskCompletionSource<List<AdresListFilterStub>>();
            Action<int> sanitizeAction = i =>
            {

                try
                {
                    Log($"--------------------- TEST NR{i} ---------------------");
                    Log($"INPUT:\r\n-straatnaam : {straatnaam ?? "<null>"}\r\n-huisNummer : {huisNummer ?? "<null>"}\r\n-index : {index ?? "<null>"}");

                    _arrangeStraatnaam(straatnaam);

                    List<AdresListFilterStub> filters = new List<AdresListFilterStub>();

                    _latest.Arrange(Produce.EmptyList<AddressDetailItem>(),
                        (when, x) => when.InterceptingLatestAdresQuery((straatnaamid, huisnr, busnr) => filters.Add(new AdresListFilterStub { HuisnummerFilter = huisnr, BusnummerFilter = busnr }), x));

                    AddressMatchRequest request = new AddressMatchRequest
                    {
                        Gemeentenaam = existingGemeentenaam,
                        Straatnaam = straatnaam,
                        Huisnummer = huisNummer,
                        Index = index
                    };

                    var notImportant = (OkObjectResult)(Send(request).Result);

                    if (filters.Any())
                    {
                        Log("RESULTS:");
                        filters.ForEach(result
                            => Log($"\t***\r\n\tHuisNummer : {result.HuisnummerFilter ?? "<null>"}\r\n\tAppNummer : NOT IMPLEMENTED, WHAT TODO WITH APPNUMMER?\r\n\tBusNummer : {result.BusnummerFilter ?? "<null>"}"));

                    }
                    else
                        Log($"NO RESULTS");

                    promise.SetResult(filters);
                }
                catch (Exception e)
                {
                    Log($"Exception\r\n{e}");
                    promise.SetException(e);
                }
            };
            matchActions.Add(sanitizeAction);

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
