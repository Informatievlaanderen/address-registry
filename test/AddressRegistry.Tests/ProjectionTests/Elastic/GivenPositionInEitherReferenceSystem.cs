namespace AddressRegistry.Tests.ProjectionTests.Elastic
{
    using System;
    using System.Globalization;
    using System.Linq;
    using AddressRegistry.StreetName;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using FluentAssertions;
    using global::AutoFixture;
    using NetTopologySuite.Geometries;
    using Projections.Elastic.AddressSearch;
    using Xunit;

    /// <summary>
    /// The indexed position keeps the reference system the event store persisted it in and says which
    /// one that is, and the WGS84 geo point is projected from that same system rather than from an
    /// assumed one. See ADR 0004.
    /// </summary>
    public class GivenPositionInEitherReferenceSystem
    {
        /// <summary>The same physical point, in both reference systems.</summary>
        private static readonly Point Lambert72Point = new Point(73862.07, 211634.58) { SRID = SystemReferenceId.SridLambert72 };
        private static readonly Point Lambert2008Point = new Point(573857.26, 711625.49) { SRID = SystemReferenceId.SridLambert2008 };

        private readonly Fixture _fixture = new Fixture();

        private AddressPosition Position(Point point) =>
            new AddressPosition(point, _fixture.Create<GeometryMethod>(), _fixture.Create<GeometrySpecification>());

        [Fact]
        public void Lambert72PositionIsIndexedAsLambert72Ewkt()
        {
            Position(Lambert72Point).GeometryAsWkt.Should().Be("SRID=31370;POINT (73862.07 211634.58)");
        }

        [Fact]
        public void Lambert2008PositionIsIndexedAsLambert2008Ewkt()
        {
            Position(Lambert2008Point).GeometryAsWkt.Should().Be("SRID=3812;POINT (573857.26 711625.49)");
        }

        [Fact]
        public void PositionWithoutSridIsIndexedAsLambert72Ewkt()
        {
            Position(new Point(73862.07, 211634.58)).GeometryAsWkt.Should().Be("SRID=31370;POINT (73862.07 211634.58)");
        }

        /// <summary>
        /// Asserted to a tolerance rather than against an exact string: the projection's trigonometry
        /// lands on a different last ULP on Linux than on Windows, and since the coordinates are rendered
        /// with the shortest round-trippable form, one bit of difference changes the string. A tenth of a
        /// millimetre is many orders of magnitude tighter than anything this value is used for.
        /// </summary>
        private const double Wgs84Tolerance = 1e-9;

        [Fact]
        public void Lambert2008PositionIsProjectedToWgs84WithoutGoingThroughLambert72()
        {
            var (longitude, latitude) = Wgs84Of(Lambert2008Point);

            longitude.Should().BeApproximately(3.2791958613886547, Wgs84Tolerance);
            latitude.Should().BeApproximately(51.209648908280634, Wgs84Tolerance);
        }

        [Fact]
        public void Lambert72PositionIsProjectedToWgs84AsBefore()
        {
            var (longitude, latitude) = Wgs84Of(Lambert72Point);

            longitude.Should().BeApproximately(3.277957970797176, Wgs84Tolerance);
            latitude.Should().BeApproximately(51.20937520963882, Wgs84Tolerance);
        }

        /// <summary>
        /// A tripwire, not a requirement. The two reference systems describe the same physical point, yet
        /// land about 90 m apart in WGS84, because <c>Lambert72Wkt</c> carries no <c>TOWGS84</c> and so
        /// projects BD72 coordinates as if they were already on the WGS84 datum. The Lambert 2008 result
        /// is the accurate one — it agrees to within 15 cm with Lambert 72 projected through the official
        /// BD72 shift. Adding that shift to <c>Lambert72Wkt</c> would move every existing geo point by
        /// the same 90 m, which is a separate decision. See ADR 0004.
        /// </summary>
        [Fact]
        public void Lambert72AndLambert2008DisagreeByTheDatumShiftMissingFromTheLambert72Definition()
        {
            var (fromLambert72Lon, fromLambert72Lat) = Wgs84Of(Lambert72Point);
            var (fromLambert2008Lon, fromLambert2008Lat) = Wgs84Of(Lambert2008Point);

            MetresApart(fromLambert72Lon, fromLambert72Lat, fromLambert2008Lon, fromLambert2008Lat)
                .Should().BeInRange(50, 150);
        }

        private static double MetresApart(double lon1, double lat1, double lon2, double lat2)
        {
            var eastWest = (lon2 - lon1) * Math.Cos(lat1 * Math.PI / 180) * 111320;
            var northSouth = (lat2 - lat1) * 110540;

            return Math.Sqrt(eastWest * eastWest + northSouth * northSouth);
        }

        private (double Longitude, double Latitude) Wgs84Of(Point point)
        {
            var parts = Position(point).GeometryAsWgs84.Split(',', StringSplitOptions.TrimEntries);

            return (
                double.Parse(parts[0], CultureInfo.InvariantCulture),
                double.Parse(parts[1], CultureInfo.InvariantCulture));
        }
    }
}
