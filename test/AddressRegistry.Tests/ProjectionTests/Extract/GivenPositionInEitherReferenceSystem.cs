namespace AddressRegistry.Tests.ProjectionTests.Extract
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using AddressRegistry.StreetName;
    using AddressRegistry.StreetName.Events;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Testing;
    using EventExtensions;
    using FluentAssertions;
    using global::AutoFixture;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Moq;
    using Projections.Extract;
    using Projections.Extract.AddressExtract;
    using SqlStreamStore;
    using Xunit;
    using Envelope = Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope;

    /// <summary>
    /// The extract is always Lambert 72, whichever reference system the event store persists, because the
    /// shapefile it feeds is accompanied by a `Belge_Lambert_1972` projection file and the shape record
    /// carries no SRID of its own. See ADR 0004.
    /// </summary>
    public sealed class GivenPositionInEitherReferenceSystem
    {
        /// <summary>The same physical point, in both reference systems.</summary>
        private const string Lambert72Point = "POINT (103671.37 192046.71)";
        private const string Lambert2008Point = "POINT (603668.87 692041.51)";

        private readonly ConnectedProjectionTest<ExtractContext, AddressExtractProjectionsV2> _sut;
        private readonly Fixture _fixture;

        public GivenPositionInEitherReferenceSystem()
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedAddressPersistentLocalId());
            _fixture.Customize(new WithFixedStreetNamePersistentLocalId());
            _fixture.Customize(new WithFixedPostalCode());
            _fixture.Customize(new WithValidHouseNumber());
            _fixture.Customize(new WithValidBoxNumber());
            _fixture.Customize(new WithExtendedWkbGeometry());

            _sut = new ConnectedProjectionTest<ExtractContext, AddressExtractProjectionsV2>(
                CreateContext,
                () => new AddressExtractProjectionsV2(
                    Mock.Of<IReadonlyStreamStore>(),
                    new EventDeserializer((_, _) => new object()),
                    new OptionsWrapper<ExtractConfig>(new ExtractConfig()),
                    Encoding.UTF8));
        }

        private static ExtractContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ExtractContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ExtractContext(options);
        }

        [Theory]
        [InlineData(SystemReferenceId.SridLambert72, Lambert72Point)]
        [InlineData(SystemReferenceId.SridLambert2008, Lambert2008Point)]
        public async Task WhenAddressWasProposed_ThenTheShapeHoldsLambert72Coordinates(int eventSrid, string eventPoint)
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithExtendedWkbGeometry(GeometryHelpers.CreateEwkbFromWkt(eventPoint, eventSrid));

            await _sut
                .Given(new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var item = await ct.AddressExtractV2.FindAsync(addressWasProposedV2.AddressPersistentLocalId);

                    item.Should().NotBeNull();

                    // These feed the shapefile's bounding box, so they have to be in the same reference
                    // system as the .prj that Api.Extract writes alongside it.
                    item!.MinimumX.Should().Be(103671.37);
                    item.MinimumY.Should().Be(192046.71);
                    item.MaximumX.Should().Be(103671.37);
                    item.MaximumY.Should().Be(192046.71);
                });
        }

        [Theory]
        [InlineData(SystemReferenceId.SridLambert72, Lambert72Point)]
        [InlineData(SystemReferenceId.SridLambert2008, Lambert2008Point)]
        public async Task WhenAddressPositionWasChanged_ThenTheShapeHoldsLambert72Coordinates(int eventSrid, string eventPoint)
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var addressPositionWasChanged = _fixture.Create<AddressPositionWasChanged>()
                .WithExtendedWkbGeometry(GeometryHelpers.CreateEwkbFromWkt(eventPoint, eventSrid));

            await _sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, new Dictionary<string, object>())),
                    new Envelope<AddressPositionWasChanged>(new Envelope(addressPositionWasChanged, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var item = await ct.AddressExtractV2.FindAsync(addressWasProposedV2.AddressPersistentLocalId);

                    item.Should().NotBeNull();
                    item!.MinimumX.Should().Be(103671.37);
                    item.MinimumY.Should().Be(192046.71);
                });
        }

        [Theory]
        [InlineData(SystemReferenceId.SridLambert72, Lambert72Point)]
        [InlineData(SystemReferenceId.SridLambert2008, Lambert2008Point)]
        public async Task WhenAddressPositionCrsWasChanged_ThenTheShapeHoldsLambert72Coordinates(int eventSrid, string eventPoint)
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var addressPositionCrsWasChanged = _fixture.Create<AddressPositionCrsWasChanged>()
                .WithExtendedWkbGeometry(GeometryHelpers.CreateEwkbFromWkt(eventPoint, eventSrid));

            await _sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, new Dictionary<string, object>())),
                    new Envelope<AddressPositionCrsWasChanged>(new Envelope(addressPositionCrsWasChanged, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var item = await ct.AddressExtractV2.FindAsync(addressWasProposedV2.AddressPersistentLocalId);

                    item.Should().NotBeNull();
                    item!.MinimumX.Should().Be(103671.37);
                    item.MinimumY.Should().Be(192046.71);
                });
        }

        /// <summary>
        /// The transformation reaches removed addresses, unlike every other position event — and a removed
        /// address has no extract record, because <c>AddressWasRemovedV2</c> deletes it. There is nothing to
        /// reproject, and nothing to throw over. See ADR 0005.
        /// </summary>
        [Fact]
        public async Task WhenAddressPositionCrsWasChangedForARemovedAddress_ThenNothingHappens()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var addressWasRemoved = _fixture.Create<AddressWasRemovedV2>();
            var addressPositionCrsWasChanged = _fixture.Create<AddressPositionCrsWasChanged>()
                .WithExtendedWkbGeometry(GeometryHelpers.CreateEwkbFromWkt(
                    Lambert2008Point, SystemReferenceId.SridLambert2008));

            await _sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, new Dictionary<string, object>())),
                    new Envelope<AddressWasRemovedV2>(new Envelope(addressWasRemoved, new Dictionary<string, object>())),
                    new Envelope<AddressPositionCrsWasChanged>(new Envelope(addressPositionCrsWasChanged, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var item = await ct.AddressExtractV2.FindAsync(addressWasProposedV2.AddressPersistentLocalId);

                    item.Should().BeNull();
                });
        }

        /// <summary>
        /// Positions written before the event store recorded an SRID carry none at all. They are Lambert 72
        /// by definition (ADR 0004), and this is the path that would break first if the projection read them
        /// through a reader that rejects SRID-less EWKB rather than falling back.
        /// </summary>
        [Fact]
        public async Task WhenPositionHasNoSrid_ThenTheShapeHoldsLambert72Coordinates()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithExtendedWkbGeometry(new ExtendedWkbGeometry(
                    GeometryHelpers.CreateWkbWithoutSridFromWkt(Lambert72Point)));

            await _sut
                .Given(new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var item = await ct.AddressExtractV2.FindAsync(addressWasProposedV2.AddressPersistentLocalId);

                    item.Should().NotBeNull();
                    item!.MinimumX.Should().Be(103671.37);
                    item.MinimumY.Should().Be(192046.71);
                });
        }

    }
}
