namespace AddressRegistry.Tests.ValueObjectTests
{
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using FluentAssertions;
    using NetTopologySuite.IO;
    using StreetName;
    using Xunit;

    public class ExtendedWkbGeometryTests
    {
        private const int SridLambert72 = 31370;
        private const int SridLambert2008 = 3812;

        private static byte[] CreateEwkbPoint(double x, double y, int srid)
        {
            var wkbWriter = new WKBWriter { Strict = false, HandleSRID = true };
            var reader = new WKTReader { DefaultSRID = srid };
            var geometry = reader.Read($"POINT ({x} {y})");
            geometry.SRID = srid;
            return wkbWriter.Write(geometry);
        }

        [Fact]
        public void CreateEWkb_WithLambert72Srid_PreservesLambert72Srid()
        {
            var ewkb = CreateEwkbPoint(141299, 185188, SridLambert72);

            var result = ExtendedWkbGeometry.CreateEWkb(ewkb);

            result.Should().NotBeNull();
            result!.ToString().TryReadSrid(out var srid);
            srid.Should().Be(SridLambert72);
        }

        [Fact]
        public void CreateEWkb_WithLambert2008Srid_PreservesLambert2008Srid()
        {
            var ewkb = CreateEwkbPoint(150000, 200000, SridLambert2008);

            var result = ExtendedWkbGeometry.CreateEWkb(ewkb);

            result.Should().NotBeNull();
            result!.ToString().TryReadSrid(out var srid);
            srid.Should().Be(SridLambert2008);
        }

        [Fact]
        public void CreateEWkb_WithNullInput_ReturnsNull()
        {
            var result = ExtendedWkbGeometry.CreateEWkb(null);

            result.Should().BeNull();
        }

        [Fact]
        public void CreateEWkb_WithInvalidBytes_ReturnsNull()
        {
            var invalidBytes = new byte[] { 0x00, 0x01, 0x02, 0x03 };

            var result = ExtendedWkbGeometry.CreateEWkb(invalidBytes);

            result.Should().BeNull();
        }
    }
}
