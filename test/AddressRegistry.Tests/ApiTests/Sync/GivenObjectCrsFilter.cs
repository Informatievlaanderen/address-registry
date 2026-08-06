namespace AddressRegistry.Tests.ApiTests.Sync
{
    using System;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Xml;
    using Api.Oslo.Address.V2.Sync;
    using Api.Oslo.Infrastructure.Options;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using FluentAssertions;
    using Microsoft.Extensions.Options;
    using Microsoft.SyndicationFeed.Atom;
    using NodaTime;
    using StreetName;
    using Xunit;

    /// <summary>
    /// The objectCrs filter selects the reference system of the embedded object only. The embedded event is
    /// always emitted exactly as the event store held it at that position. See ADR 0004.
    /// </summary>
    public class GivenObjectCrsFilter
    {
        private const string Lambert72Point = "POINT (103671.37 192046.71)";
        private const string Lambert2008Point = "POINT (603668.87 692041.51)";

        private const string Lambert72Pos = "103671.37 192046.71";
        private const string Lambert2008Pos = "603668.87 692041.51";

        [Theory]
        [InlineData("3812", SystemReferenceId.SridLambert2008)]
        [InlineData(" 3812 ", SystemReferenceId.SridLambert2008)]
        [InlineData("31370", SystemReferenceId.SridLambert72)]
        [InlineData("EPSG:3812", SystemReferenceId.SridLambert72)]
        [InlineData("nonsense", SystemReferenceId.SridLambert72)]
        [InlineData("", SystemReferenceId.SridLambert72)]
        [InlineData(null, SystemReferenceId.SridLambert72)]
        public void ThenOnlyTheExactValue3812SelectsLambert2008(string? objectCrs, int expectedSrid)
            => ObjectCrs.ToSrid(objectCrs).Should().Be(expectedSrid);

        [Fact]
        public async Task WhenNotRequested_ThenLambert72PositionIsUnchanged()
        {
            var feed = await WriteFeed(Lambert72Position(), objectCrs: null);

            PosOfObjectPosition(feed).Should().Be(Lambert72Pos);
        }

        [Fact]
        public async Task WhenRequesting3812_ThenLambert72PositionIsConverted()
        {
            var feed = await WriteFeed(Lambert72Position(), objectCrs: "3812");

            PosOfObjectPosition(feed).Should().Be(Lambert2008Pos);
        }

        [Fact]
        public async Task WhenRequesting3812_ThenLambert2008PositionStaysAsIs()
        {
            var feed = await WriteFeed(Lambert2008Position(), objectCrs: "3812");

            PosOfObjectPosition(feed).Should().Be(Lambert2008Pos);
        }

        /// <summary>
        /// The default direction, and the one that only starts mattering once the event store is converted:
        /// a caller that does not ask keeps getting Lambert 72, so the feed's existing contract holds.
        /// </summary>
        [Fact]
        public async Task WhenNotRequested_ThenLambert2008PositionIsConvertedBackToLambert72()
        {
            var feed = await WriteFeed(Lambert2008Position(), objectCrs: null);

            PosOfObjectPosition(feed).Should().Be(Lambert72Pos);
        }

        [Fact]
        public async Task WhenUnrecognisedValue_ThenLambert2008PositionIsConvertedBackToLambert72()
        {
            var feed = await WriteFeed(Lambert2008Position(), objectCrs: "nonsense");

            PosOfObjectPosition(feed).Should().Be(Lambert72Pos);
        }

        [Fact]
        public async Task WhenPositionHasNoSrid_ThenItIsReadAsLambert72()
        {
            var feed = await WriteFeed(GeometryHelpers.CreateWkbWithoutSridFromWkt(Lambert72Point), objectCrs: null);

            PosOfObjectPosition(feed).Should().Be(Lambert72Pos);
        }

        /// <summary>
        /// The embedded event is the event store's own payload and is never reprojected, even when the object
        /// beside it is.
        /// </summary>
        [Fact]
        public async Task WhenRequesting3812_ThenTheEmbeddedEventIsStillTheStoredPosition()
        {
            var stored = Lambert72Position();

            var feed = await WriteFeed(stored, objectCrs: "3812");

            feed.Should().Contain(Convert.ToHexString(stored));
            PosOfObjectPosition(feed).Should().Be(Lambert2008Pos);
        }

        private static byte[] Lambert72Position()
            => GeometryHelpers.CreateEwkbFromWkt(Lambert72Point, SystemReferenceId.SridLambert72);

        private static byte[] Lambert2008Position()
            => GeometryHelpers.CreateEwkbFromWkt(Lambert2008Point, SystemReferenceId.SridLambert2008);

        /// <summary>
        /// The object's position is a bare <c>&lt;pos&gt;</c> — <c>GmlPoint</c> has no srsName member — so the
        /// coordinates are the only evidence of which reference system it came back in.
        /// </summary>
        private static string PosOfObjectPosition(string feed)
        {
            var pos = Regex.Match(feed, "<pos>([^<]+)</pos>");
            pos.Success.Should().BeTrue("the feed should contain an object position");

            return pos.Groups[1].Value;
        }

        private static async Task<string> WriteFeed(byte[] position, string? objectCrs)
        {
            var address = new AddressSyndicationQueryResult(
                Guid.NewGuid(),
                1,
                1,
                1,
                "11",
                null,
                Guid.NewGuid(),
                "9000",
                position,
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Building,
                "AddressWasProposedV2",
                Instant.FromUtc(2026, 1, 1, 0, 0),
                Instant.FromUtc(2026, 1, 1, 0, 0),
                true,
                true,
                AddressStatus.Proposed,
                null,
                "reason",
                // The event payload carries the store's own hex, and must come out untouched whatever objectCrs says.
                $"<AddressWasProposedV2><ExtendedWkbGeometry>{Convert.ToHexString(position)}</ExtendedWkbGeometry></AddressWasProposedV2>");

            var sw = new StringWriterWithEncoding(Encoding.UTF8);
            await using (var xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings { Async = true, Indent = true, Encoding = sw.Encoding }))
            {
                var formatter = new AtomFormatter(null, xmlWriter.Settings) { UseCDATA = true };
                var writer = new AtomFeedWriter(xmlWriter, null, formatter);

                await writer.WriteAddress(
                    new OptionsWrapper<ResponseOptionsV2>(new ResponseOptionsV2 { Naamruimte = "https://data.vlaanderen.be/id/adres" }),
                    formatter,
                    "category",
                    address,
                    ObjectCrs.ToSrid(objectCrs));

                xmlWriter.Flush();
            }

            return sw.ToString();
        }
    }
}
