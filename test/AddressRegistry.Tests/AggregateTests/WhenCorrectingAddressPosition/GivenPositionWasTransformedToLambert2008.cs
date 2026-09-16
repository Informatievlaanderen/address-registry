namespace AddressRegistry.Tests.AggregateTests.WhenCorrectingAddressPosition
{
    using AddressRegistry.Api.BackOffice.Abstractions;
    using AddressRegistry.Api.BackOffice.Infrastructure;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.GrAr.CrsTransform;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using EventExtensions;
    using FluentAssertions;
    using global::AutoFixture;
    using NetTopologySuite.Geometries;
    using StreetName;
    using StreetName.Commands;
    using StreetName.Events;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// A correction that changes nothing must apply nothing. The aggregate decides that by comparing the
    /// EWKB <em>bytes</em> (<see cref="AddressGeometry"/> over <see cref="ExtendedWkbGeometry"/>, a
    /// <c>ByteArrayValueObject</c>), so the same point serialized differently reads as a change.
    ///
    /// That is what produced <see cref="AddressPositionWasCorrectedV2"/> events whose position differed from
    /// the one before it only by a missing SRID — a byte array four bytes shorter holding the same
    /// coordinates. The lambda's <c>GmlHelpers.ToExtendedWkbGeometry</c> wrote plain WKB
    /// (<c>geometry.AsBinary()</c>) while the event store, once <see cref="AddressPositionCrsWasChanged"/>
    /// had run over the stream, held EWKB carrying SRID 3812. See ADR 0005.
    ///
    /// These tests pin the round trip that reproduced it: read the address back out of the API, post the
    /// very same position at it, and apply nothing.
    /// </summary>
    public class GivenPositionWasTransformedToLambert2008 : AddressRegistryTest
    {
        /// <summary>
        /// <see cref="GeometryHelpers.GmlPointGeometry"/> and the same physical point in Lambert 2008,
        /// the pair <c>GmlPositionNormalizerTests</c> pins the conversion of.
        /// </summary>
        private const string Lambert72PointWkt = "POINT (103671.37 192046.71)";
        private const string Lambert2008PointWkt = WithExtendedWkbGeometryLambert2008.PointWkt;

        private const GeometryMethod Method = GeometryMethod.AppointedByAdministrator;
        private const GeometrySpecification Specification = GeometrySpecification.Entry;

        private readonly StreetNameStreamId _streamId;

        public GivenPositionWasTransformedToLambert2008(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedMunicipalityId());
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());
            Fixture.Customize(new WithFixedAddressPersistentLocalId());

            _streamId = Fixture.Create<StreetNameStreamId>();
        }

        /// <summary>The position as the event store held it before it wrote EWKB: no SRID at all.</summary>
        private static ExtendedWkbGeometry LegacyPositionWithoutSrid
            => new ExtendedWkbGeometry(GeometryHelpers.CreateWkbWithoutSridFromWkt(Lambert72PointWkt));

        /// <summary>The position as the Lambert 2008 migrator leaves it: EWKB carrying SRID 3812.</summary>
        private static ExtendedWkbGeometry MigratedPosition
            => GeometryHelpers.CreateEwkbFromWkt(Lambert2008PointWkt, SystemReferenceId.SridLambert2008);

        /// <summary>
        /// What the BackOffice does with an incoming GML position: normalize it to the event store's
        /// reference system (<see cref="GmlPositionNormalizer"/>, in the controller) and then serialize it
        /// to EWKB (<c>GmlHelpers</c>, in the lambda). Going through both rather than through a handcrafted
        /// <see cref="ExtendedWkbGeometry"/> is the point of these tests.
        /// </summary>
        private static ExtendedWkbGeometry AsPostedToTheBackOffice(string gml)
            => new GmlPositionNormalizer(new UseLambert2008EventStoreToggle(true))
                .ToEventStoreSrs(gml)
                .ToExtendedWkbGeometry();

        private CorrectAddressPosition CorrectWith(ExtendedWkbGeometry position)
            => new CorrectAddressPosition(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                Method,
                Specification,
                position,
                Fixture.Create<Provenance>());

        private IScenarioGivenStateBuilder MigratedAddress()
        {
            var addressWasProposedV2 = Fixture.Create<AddressWasProposedV2>()
                .AsHouseNumberAddress()
                .WithGeometryMethod(Method)
                .WithGeometrySpecification(Specification)
                .WithExtendedWkbGeometry(LegacyPositionWithoutSrid);

            var addressPositionCrsWasChanged = new AddressPositionCrsWasChanged(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                Method,
                Specification,
                MigratedPosition);
            ((ISetProvenance)addressPositionCrsWasChanged).SetProvenance(Fixture.Create<Provenance>());

            return new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasProposedV2,
                    addressPositionCrsWasChanged);
        }

        /// <summary>
        /// Version 3 of the API answers in the reference system the position is persisted in, so a caller
        /// that edits nothing posts back Lambert 2008. The normalizer passes it through untouched.
        /// </summary>
        [Fact]
        public void WhenCorrectingWithTheLambert2008PositionTheApiServes_ThenNone()
        {
            Assert(MigratedAddress()
                .When(CorrectWith(AsPostedToTheBackOffice(GeometryHelpers.GmlPointGeometryLambert2008)))
                .ThenNone());
        }

        /// <summary>
        /// Version 2 of the API answers in Lambert 72 whatever the event store holds, so a caller that edits
        /// nothing posts back Lambert 72 and the normalizer converts it. The position therefore makes a full
        /// 08 -> 72 -> 08 round trip before it is compared, and still has to come out as the stored bytes.
        /// </summary>
        [Fact]
        public void WhenCorrectingWithTheLambert72PositionTheApiServes_ThenNone()
        {
            Assert(MigratedAddress()
                .When(CorrectWith(AsPostedToTheBackOffice(GeometryHelpers.GmlPointGeometry)))
                .ThenNone());
        }

        /// <summary>
        /// The same two round trips, stated as the byte equality the aggregate actually performs. This is the
        /// assertion the old <c>geometry.AsBinary()</c> failed: the coordinates matched and the bytes did not.
        /// </summary>
        [Theory]
        [InlineData(GeometryHelpers.GmlPointGeometry)]
        [InlineData(GeometryHelpers.GmlPointGeometryLambert2008)]
        public void ThenThePostedPositionIsByteIdenticalToTheStoredPosition(string gml)
        {
            var posted = AsPostedToTheBackOffice(gml);

            posted.ToByteArray().TryReadSrid(out var srid).Should().BeTrue();
            srid.Should().Be(SystemReferenceId.SridLambert2008);
            posted.Should().Be(MigratedPosition);
        }

        /// <summary>
        /// The one way a no-op correction still applies an event, and it is not the SRID.
        ///
        /// Version 2 answers in Lambert 72, so a caller who edits nothing sends the position through
        /// 08 -> 72 -> 08 with a rounding to the centimetre at each end. That is not quite the identity:
        /// for roughly one position in 25 000 the two roundings fall either side of a half-centimetre and
        /// the position comes back one centimetre off, which the aggregate can only read as a change.
        ///
        /// Nothing here is serializable away — the position really is different — so this is a
        /// characterization test, not a guard. If a transform library upgrade makes this point survive the
        /// round trip, this test failing is the news, and the residual is worth re-measuring over a grid
        /// rather than assuming it is gone.
        /// </summary>
        [Fact]
        public void WhenCorrectingWithALambert72PositionThatDoesNotSurviveTheRoundTrip_ThenAddressPositionWasCorrectedV2()
        {
            // A position in East Flanders, picked from a grid sweep as one that drifts.
            var stored = ((Geometry)new Point(new Coordinate(106513.37, 167137.71)) { SRID = SystemReferenceId.SridLambert72 })
                .TransformFromLambert72To08(ExtendedWkbGeometry.CoordinateDecimals);
            var storedPosition = ExtendedWkbGeometry.Create(stored);

            // What version 2 serves for it, and what comes back when that is posted unchanged. Api.Oslo
            // rounds to its own PositionCoordinateDecimals, which is the same centimetre as the event
            // store's — it has to be, or a position could never be posted back as it was stored.
            var served = GeometryExtensions.ConvertToGml(
                ((Geometry)stored.Copy()).EnsureLambert72().RoundCoordinates(ExtendedWkbGeometry.CoordinateDecimals),
                false);
            var posted = AsPostedToTheBackOffice(served);

            // One centimetre off, plus the slack to compare two doubles that are a centimetre apart.
            var postedPoint = (Point)WKBReaderFactory.CreateForEwkb(posted.ToByteArray()).Read(posted.ToByteArray());
            postedPoint.X.Should().BeApproximately(((Point)stored).X, 0.0101);
            postedPoint.Y.Should().BeApproximately(((Point)stored).Y, 0.0101);
            posted.Should().NotBe(storedPosition);

            var addressWasProposedV2 = Fixture.Create<AddressWasProposedV2>()
                .AsHouseNumberAddress()
                .WithGeometryMethod(Method)
                .WithGeometrySpecification(Specification)
                .WithExtendedWkbGeometry(storedPosition);

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasProposedV2)
                .When(CorrectWith(posted))
                .Then(new Fact(_streamId,
                    new AddressPositionWasCorrectedV2(
                        Fixture.Create<StreetNamePersistentLocalId>(),
                        Fixture.Create<AddressPersistentLocalId>(),
                        Method,
                        Specification,
                        posted))));
        }

        /// <summary>
        /// The bug itself, kept as a test: the same point without its SRID is a different byte array, and the
        /// aggregate has no way to see it is the same position. Nothing in the BackOffice can produce this
        /// any more — <c>GmlHelpers</c> writes EWKB and an unsupported or missing srsName throws — but the
        /// comparison it trips over is still a byte comparison, so this is what any future writer that drops
        /// the SRID would do.
        /// </summary>
        [Fact]
        public void WhenCorrectingWithTheSamePointWithoutItsSrid_ThenAddressPositionWasCorrectedV2()
        {
            var positionWithoutSrid = new ExtendedWkbGeometry(
                GeometryHelpers.CreateWkbWithoutSridFromWkt(Lambert2008PointWkt));

            positionWithoutSrid.ToByteArray().Length
                .Should().BeLessThan(MigratedPosition.ToByteArray().Length);

            Assert(MigratedAddress()
                .When(CorrectWith(positionWithoutSrid))
                .Then(new Fact(_streamId,
                    new AddressPositionWasCorrectedV2(
                        Fixture.Create<StreetNamePersistentLocalId>(),
                        Fixture.Create<AddressPersistentLocalId>(),
                        Method,
                        Specification,
                        positionWithoutSrid))));
        }
    }
}
