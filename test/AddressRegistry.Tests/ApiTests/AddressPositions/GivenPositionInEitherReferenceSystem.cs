namespace AddressRegistry.Tests.ApiTests.AddressPositions
{
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using FluentAssertions;
    using StreetName;
    using Xunit;
    using V2Mapper = Api.Oslo.Address.V2.AddressMapper;
    using V3Mapper = Api.Oslo.Address.V3.AddressMapper;

    /// <summary>
    /// Both mappers read the position in whatever reference system the event store persisted it in.
    /// Version 2 always answers in Lambert 72, version 3 answers in the persisted one preceded by its
    /// Lambert 72 equivalent. See ADR 0004.
    /// </summary>
    public class GivenPositionInEitherReferenceSystem
    {
        private const string Lambert72Point = "POINT (103671.37 192046.71)";
        private const string Lambert2008Point = "POINT (603668.87 692041.51)";

        private const string Lambert72Pos = "103671.37 192046.71";
        private const string Lambert2008Pos = "603668.87 692041.51";

        private static byte[] Lambert72Position()
            => GeometryHelpers.CreateEwkbFromWkt(Lambert72Point, SystemReferenceId.SridLambert72);

        private static byte[] Lambert2008Position()
            => GeometryHelpers.CreateEwkbFromWkt(Lambert2008Point, SystemReferenceId.SridLambert2008);

        [Fact]
        public void V2_WithLambert72Position_ReturnsLambert72Gml()
        {
            var position = V2Mapper.GetAddressPoint(Lambert72Position(), GeometryMethod.AppointedByAdministrator, GeometrySpecification.Building);

            position.Geometry.Gml.Should().Contain("EPSG/0/31370");
            position.Geometry.Gml.Should().Contain($"<gml:pos>{Lambert72Pos}</gml:pos>");
        }

        [Fact]
        public void V2_WithLambert2008Position_ReturnsTheSamePointInLambert72()
        {
            var position = V2Mapper.GetAddressPoint(Lambert2008Position(), GeometryMethod.AppointedByAdministrator, GeometrySpecification.Building);

            position.Geometry.Gml.Should().Contain("EPSG/0/31370");
            position.Geometry.Gml.Should().NotContain("3812");
            position.Geometry.Gml.Should().Contain($"<gml:pos>{Lambert72Pos}</gml:pos>");
        }

        [Fact]
        public void V2_WithPositionWithoutSrid_ReadsItAsLambert72()
        {
            var position = V2Mapper.GetAddressPoint(
                GeometryHelpers.CreateWkbWithoutSridFromWkt(Lambert72Point),
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Building);

            position.Geometry.Gml.Should().Contain("EPSG/0/31370");
            position.Geometry.Gml.Should().Contain($"<gml:pos>{Lambert72Pos}</gml:pos>");
        }

        [Fact]
        public void V2_SyndicationPoint_WithLambert2008Position_ReturnsTheSamePointInLambert72()
        {
            var point = V2Mapper.GetAddressPoint(Lambert2008Position());

            point.XmlPoint.Pos.Should().Be(Lambert72Pos);
            point.JsonPoint.Coordinates.Should().Equal(103671.37, 192046.71);
        }

        [Fact]
        public void V2_SyndicationPoint_WithLambert72Position_IsUnchanged()
        {
            var point = V2Mapper.GetAddressPoint(Lambert72Position());

            point.XmlPoint.Pos.Should().Be(Lambert72Pos);
            point.JsonPoint.Coordinates.Should().Equal(103671.37, 192046.71);
        }

        [Fact]
        public void V3_WithLambert72Position_ReturnsLambert72Only()
        {
            var position = V3Mapper.GetAddressPoint(Lambert72Position(), GeometryMethod.AppointedByAdministrator, GeometrySpecification.Building);

            position.Geometry.Should().ContainSingle();
            position.Geometry.Single().Gml.Should().Contain("EPSG/0/31370");
            position.Geometry.Single().Gml.Should().Contain($"<gml:pos>{Lambert72Pos}</gml:pos>");
        }

        [Fact]
        public void V3_WithLambert2008Position_ReturnsLambert72FollowedByLambert2008()
        {
            var position = V3Mapper.GetAddressPoint(Lambert2008Position(), GeometryMethod.AppointedByAdministrator, GeometrySpecification.Building);

            position.Geometry.Should().HaveCount(2);

            position.Geometry[0].Gml.Should().Contain("EPSG/0/31370");
            position.Geometry[0].Gml.Should().Contain($"<gml:pos>{Lambert72Pos}</gml:pos>");

            position.Geometry[1].Gml.Should().Contain("EPSG/0/3812");
            position.Geometry[1].Gml.Should().Contain($"<gml:pos>{Lambert2008Pos}</gml:pos>");
        }

        [Fact]
        public void V3_WithPositionWithoutSrid_ReadsItAsLambert72()
        {
            var position = V3Mapper.GetAddressPoint(
                GeometryHelpers.CreateWkbWithoutSridFromWkt(Lambert72Point),
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Building);

            position.Geometry.Should().ContainSingle();
            position.Geometry.Single().Gml.Should().Contain("EPSG/0/31370");
        }
    }
}
