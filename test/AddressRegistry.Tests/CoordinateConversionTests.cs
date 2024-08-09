namespace AddressRegistry.Tests
{
    using System.Globalization;
    using FluentAssertions;
    using NetTopologySuite.Geometries;
    using Projections.Elastic;
    using Xunit;

    public class CoordinateConversionTests
    {
        [Fact]
        public void FromLambert72ToWgs84()
        {
            const double x = 73862.07;
            const double y = 211634.58;

            var wgs84Point = CoordinateTransformer.FromLambert72ToWgs84(new Point(x, y)
            {
                SRID = 31370
            });

            var pointAsWgs84 = string.Format(CultureInfo.InvariantCulture, "{0}, {1}", wgs84Point.X, wgs84Point.Y);

            pointAsWgs84.Should().Be("3.277957970797176, 51.20937520963882");
        }
    }
}
