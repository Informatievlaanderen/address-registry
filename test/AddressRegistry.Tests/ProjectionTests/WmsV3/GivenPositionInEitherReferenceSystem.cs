namespace AddressRegistry.Tests.ProjectionTests.WmsV3
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AddressRegistry.StreetName;
    using AddressRegistry.StreetName.Events;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using EventExtensions;
    using FluentAssertions;
    using global::AutoFixture;
    using Moq;
    using Projections.Wms.AddressWmsItemV3;
    using Xunit;
    using Envelope = Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope;

    /// <summary>
    /// Version 3 stores Lambert 72 whatever the event store persists, so its table, spatial index
    /// and views stay single-SRID through the conversion. See ADR 0004.
    /// </summary>
    public class GivenPositionInEitherReferenceSystem : AddressWmsItemV3ProjectionTest
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
        public async Task WhenAddressWasProposed_ThenThePositionIsStoredInLambert72(int eventSrid, string eventPoint)
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>()
                .WithExtendedWkbGeometry(GeometryHelpers.CreateEwkbFromWkt(eventPoint, eventSrid));

            await Sut
                .Given(new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var addressWmsItem = await ct.AddressWmsItemsV3.FindAsync(addressWasProposedV2.AddressPersistentLocalId);

                    addressWmsItem.Should().NotBeNull();
                    addressWmsItem!.Position.SRID.Should().Be(SystemReferenceId.SridLambert72);

                    // Rounded back to the centimetre positions are persisted at, so both inputs land on
                    // exactly the same stored coordinates.
                    addressWmsItem.Position.X.Should().Be(103671.37);
                    addressWmsItem.Position.Y.Should().Be(192046.71);
                    addressWmsItem.PositionX.Should().Be(103671.37);
                    addressWmsItem.PositionY.Should().Be(192046.71);
                });
        }

        [Theory]
        [InlineData(SystemReferenceId.SridLambert72, Lambert72Point)]
        [InlineData(SystemReferenceId.SridLambert2008, Lambert2008Point)]
        public async Task WhenAddressPositionWasChanged_ThenThePositionIsStoredInLambert72(int eventSrid, string eventPoint)
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
                    var addressWmsItem = await ct.AddressWmsItemsV3.FindAsync(addressWasProposedV2.AddressPersistentLocalId);

                    addressWmsItem.Should().NotBeNull();
                    addressWmsItem!.Position.SRID.Should().Be(SystemReferenceId.SridLambert72);
                    addressWmsItem.Position.X.Should().Be(103671.37);
                    addressWmsItem.Position.Y.Should().Be(192046.71);
                });
        }

        [Theory]
        [InlineData(SystemReferenceId.SridLambert72, Lambert72Point)]
        [InlineData(SystemReferenceId.SridLambert2008, Lambert2008Point)]
        public async Task WhenAddressPositionCrsWasChanged_ThenThePositionIsStoredInLambert72(int eventSrid, string eventPoint)
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
                    var addressWmsItem = await ct.AddressWmsItemsV3.FindAsync(addressWasProposedV2.AddressPersistentLocalId);

                    addressWmsItem.Should().NotBeNull();
                    addressWmsItem!.Position.SRID.Should().Be(SystemReferenceId.SridLambert72);
                    addressWmsItem.Position.X.Should().Be(103671.37);
                    addressWmsItem.Position.Y.Should().Be(192046.71);
                });
        }

        protected override AddressWmsItemV3Projections CreateProjection()
            => new AddressWmsItemV3Projections(_houseNumberLabelUpdaterMock.Object);
    }
}
