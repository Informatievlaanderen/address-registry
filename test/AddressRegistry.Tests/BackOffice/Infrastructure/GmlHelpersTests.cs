namespace AddressRegistry.Tests.BackOffice.Infrastructure
{
    using System;
    using AddressRegistry.Api.BackOffice.Abstractions;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using FluentAssertions;
    using NetTopologySuite.Geometries;
    using Xunit;
    // GrAr's reader rather than AddressRegistry's, deliberately: this one throws on EWKB without an SRID
    // instead of falling back to Lambert 72, so reading the bytes back also proves the SRID is in them.
    using StrictEwkbReaderFactory = Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology.WKBReaderFactory;

    /// <summary>
    /// What the lambda persists. Positions reaching it have already been normalized to the event store's
    /// reference system by the API (ADR 0003), so this reads the srsName rather than assuming one, and
    /// records the SRID in the EWKB it writes. See ADR 0005.
    /// </summary>
    public class GmlHelpersTests
    {
        private const string GmlPointWithoutSrsName =
            "<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\">" +
            "<gml:pos>103671.37 192046.71</gml:pos></gml:Point>";

        private const string GmlPointInWgs84 =
            "<gml:Point srsName=\"http://www.opengis.net/def/crs/EPSG/0/4326\" xmlns:gml=\"http://www.opengis.net/gml/3.2\">" +
            "<gml:pos>4.35 51.21</gml:pos></gml:Point>";

        [Fact]
        public void GivenLambert72Position_ThenTheEwkbCarriesLambert72()
        {
            var position = ReadBack(GeometryHelpers.GmlPointGeometry);

            position.SRID.Should().Be(SystemReferenceId.SridLambert72);
            position.X.Should().Be(103671.37);
            position.Y.Should().Be(192046.71);
        }

        /// <summary>
        /// The regression this exists for: the SRID used to be force-set to Lambert 72, which relabelled a
        /// Lambert 2008 position instead of keeping it, leaving the coordinates ~500 km from the address.
        /// </summary>
        [Fact]
        public void GivenLambert2008Position_ThenTheEwkbCarriesLambert2008()
        {
            var position = ReadBack(GeometryHelpers.GmlPointGeometryLambert2008);

            position.SRID.Should().Be(SystemReferenceId.SridLambert2008);
            position.X.Should().Be(603668.87);
            position.Y.Should().Be(692041.51);
        }

        /// <summary>
        /// The SRID has to be in the bytes: without it every reader falls back to Lambert 72
        /// (see ADR 0004), which is the same silent 500 km error by another route.
        /// </summary>
        [Theory]
        [InlineData(GeometryHelpers.GmlPointGeometry)]
        [InlineData(GeometryHelpers.GmlPointGeometryLambert2008)]
        public void ThenTheSridIsPersisted(string gml)
        {
            gml.ToExtendedWkbGeometry()
                .ToString()
                .ToByteArray()
                .TryReadSrid(out _)
                .Should().BeTrue();
        }

        [Theory]
        [InlineData(GmlPointWithoutSrsName)]
        [InlineData(GmlPointInWgs84)]
        public void GivenAnUnsupportedOrMissingSrsName_ThenThrows(string gml)
        {
            var act = () => gml.ToExtendedWkbGeometry();

            act.Should().Throw<InvalidOperationException>();
        }

        private static Point ReadBack(string gml)
        {
            var ewkb = gml.ToExtendedWkbGeometry().ToString().ToByteArray();
            return (Point)StrictEwkbReaderFactory.CreateForEwkb(ewkb).Read(ewkb);
        }
    }
}
