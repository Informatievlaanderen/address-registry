namespace AddressRegistry.Tests.ProjectionTests.WfsV3
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using EventExtensions;
    using FluentAssertions;
    using global::AutoFixture;
    using Moq;
    using Projections.Wfs.AddressWfsV3;
    using AddressRegistry.StreetName;
    using AddressRegistry.StreetName.Events;
    using Xunit;
    using Envelope = Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope;

    /// <summary>
    /// Version 3 stores Lambert 2008 whatever the event store persists, so its table, spatial index and
    /// views stay single-SRID through the conversion. See ADR 0004.
    /// </summary>
    public class GivenPositionInEitherReferenceSystem : AddressWfsItemV3ProjectionTest
    {
        /// <summary>The same physical point, in both reference systems.</summary>
        private const string Lambert72Point = "POINT (103671.37 192046.71)";
        private const string Lambert2008Point = "POINT (603668.87 692041.51)";

        private readonly Fixture _fixture;
        private readonly Mock<IHouseNumberLabelUpdater> _houseNumberLabelUpdaterMock = new Mock<IHouseNumberLabelUpdater>();

        public GivenPositionInEitherReferenceSystem()
        {
            _fixture = new Fixture();
            _fixture.Customize(new WithFixedAddressPersistentLocalId());
            _fixture.Customize(new WithFixedStreetNamePersistentLocalId());
            _fixture.Customize<AddressStatus>(_ => new WithoutUnknownStreetNameAddressStatus());
            _fixture.Customize(new WithValidHouseNumber());
            _fixture.Customize(new WithValidBoxNumber());
            _fixture.Customize(new WithExtendedWkbGeometry());
            _fixture.Customize(new InfrastructureCustomization());
        }

        [Theory]
        [InlineData(SystemReferenceId.SridLambert72, Lambert72Point)]
        [InlineData(SystemReferenceId.SridLambert2008, Lambert2008Point)]
        public async Task WhenAddressWasProposed_ThenThePositionIsStoredInLambert2008(int eventSrid, string eventPoint)
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithExtendedWkbGeometry(GeometryHelpers.CreateEwkbFromWkt(eventPoint, eventSrid));

            await Sut
                .Given(new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var addressWfsItem = await ct.AddressWfsV3Items.FindAsync(addressWasProposedV2.AddressPersistentLocalId);

                    addressWfsItem.Should().NotBeNull();
                    addressWfsItem!.Position.SRID.Should().Be(SystemReferenceId.SridLambert2008);

                    // Rounded back to the centimetre positions are persisted at, so a position that came
                    // in as Lambert 72 is indistinguishable from one that came in as Lambert 2008.
                    addressWfsItem.Position.X.Should().Be(603668.87);
                    addressWfsItem.Position.Y.Should().Be(692041.51);
                    addressWfsItem.PositionX.Should().Be(603668.87);
                    addressWfsItem.PositionY.Should().Be(692041.51);
                });
        }

        [Theory]
        [InlineData(SystemReferenceId.SridLambert72, Lambert72Point)]
        [InlineData(SystemReferenceId.SridLambert2008, Lambert2008Point)]
        public async Task WhenAddressPositionWasChanged_ThenThePositionIsStoredInLambert2008(int eventSrid, string eventPoint)
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var addressPositionWasChanged = _fixture.Create<AddressPositionWasChanged>()
                .WithExtendedWkbGeometry(GeometryHelpers.CreateEwkbFromWkt(eventPoint, eventSrid));

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, new Dictionary<string, object>())),
                    new Envelope<AddressPositionWasChanged>(new Envelope(addressPositionWasChanged, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var addressWfsItem = await ct.AddressWfsV3Items.FindAsync(addressWasProposedV2.AddressPersistentLocalId);

                    addressWfsItem.Should().NotBeNull();
                    addressWfsItem!.Position.SRID.Should().Be(SystemReferenceId.SridLambert2008);
                    addressWfsItem.Position.X.Should().Be(603668.87);
                    addressWfsItem.Position.Y.Should().Be(692041.51);
                });
        }

        [Theory]
        [InlineData(SystemReferenceId.SridLambert72, Lambert72Point)]
        [InlineData(SystemReferenceId.SridLambert2008, Lambert2008Point)]
        public async Task WhenAddressPositionCrsWasChanged_ThenThePositionIsStoredInLambert2008(int eventSrid, string eventPoint)
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var addressPositionCrsWasChanged = _fixture.Create<AddressPositionCrsWasChanged>()
                .WithExtendedWkbGeometry(GeometryHelpers.CreateEwkbFromWkt(eventPoint, eventSrid));

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, new Dictionary<string, object>())),
                    new Envelope<AddressPositionCrsWasChanged>(new Envelope(addressPositionCrsWasChanged, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var addressWfsItem = await ct.AddressWfsV3Items.FindAsync(addressWasProposedV2.AddressPersistentLocalId);

                    addressWfsItem.Should().NotBeNull();
                    addressWfsItem!.Position.SRID.Should().Be(SystemReferenceId.SridLambert2008);
                    addressWfsItem.Position.X.Should().Be(603668.87);
                    addressWfsItem.Position.Y.Should().Be(692041.51);
                });
        }


        /// <summary>
        /// Positions written before the event store recorded an SRID carry none at all. They are Lambert 72
        /// by definition (ADR 0004), and this is the path that would break first if the projection read them
        /// through a reader that rejects SRID-less EWKB rather than falling back.
        /// </summary>
        [Fact]
        public async Task WhenPositionHasNoSrid_ThenItIsReadAsLambert72()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithExtendedWkbGeometry(new ExtendedWkbGeometry(
                    GeometryHelpers.CreateWkbWithoutSridFromWkt(Lambert72Point)));

            await Sut
                .Given(new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var addressWfsItem = await ct.AddressWfsV3Items.FindAsync(addressWasProposedV2.AddressPersistentLocalId);

                    addressWfsItem.Should().NotBeNull();
                    addressWfsItem!.Position.SRID.Should().Be(SystemReferenceId.SridLambert2008);
                    addressWfsItem.Position.X.Should().Be(603668.87);
                    addressWfsItem.Position.Y.Should().Be(692041.51);
                });
        }

        protected override AddressWfsV3Projections CreateProjection()
            => new AddressWfsV3Projections(_houseNumberLabelUpdaterMock.Object);
    }
}
