namespace AddressRegistry.Tests
{
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

            var pointAsWgs84 = CoordinateTransformer.FromLambert72ToWgs84Text(new Point(x, y)
            {
                SRID = 31370
            });

            pointAsWgs84.Should().Be("3.277957970797176, 51.20937520963882");
        }
    }
}
