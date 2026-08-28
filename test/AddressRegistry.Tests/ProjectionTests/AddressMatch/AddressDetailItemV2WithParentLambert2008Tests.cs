namespace AddressRegistry.Tests.ProjectionTests.AddressMatch
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AddressRegistry.StreetName.Events;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using EventExtensions;
    using FluentAssertions;
    using global::AutoFixture;
    using Projections.AddressMatch.AddressDetailV2WithParent;
    using Xunit;

    /// <summary>
    /// The address match projection is the legacy detail projection minus a few columns, and stores the
    /// position the same way: as the EWKB it was given, whichever reference system that is in. These
    /// tests pin that down for Lambert 2008, the same way
    /// <see cref="Legacy.AddressDetailItemV2WithParentLambert2008Tests"/> does for the legacy projection.
    /// See ADR 0004.
    /// </summary>
    public class AddressDetailItemV2WithParentLambert2008Tests
        : AddressMatchProjectionTest<AddressDetailProjectionsV2WithParent>
    {
        private readonly Fixture _fixture;

        public AddressDetailItemV2WithParentLambert2008Tests()
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedAddressPersistentLocalId());
            _fixture.Customize(new WithFixedStreetNamePersistentLocalId());
            _fixture.Customize(new WithExtendedWkbGeometryLambert2008());
            _fixture.Customize(new WithValidHouseNumber());
            _fixture.Customize(new WithValidBoxNumber());
        }

        [Fact]
        public async Task WhenAddressWasProposedV2_ThenPositionKeepsItsLambert2008Srid()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            await Sut
                .Given(new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, metadata)))
                .Then(async ct =>
                {
                    var addressDetailItemV2 =
                        await ct.AddressDetailV2WithParent.FindAsync(addressWasProposedV2.AddressPersistentLocalId);

                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2!.Position.Should()
                        .BeEquivalentTo(addressWasProposedV2.ExtendedWkbGeometry.ToByteArray());

                    addressDetailItemV2.Position.TryReadSrid(out var srid).Should().BeTrue();
                    srid.Should().Be(SystemReferenceId.SridLambert2008);
                });
        }

        [Fact]
        public async Task WhenAddressPositionWasChanged_ThenPositionKeepsItsLambert2008Srid()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var addressPositionWasChanged = _fixture.Create<AddressPositionWasChanged>();
            var positionChangedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressPositionWasChanged.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressPositionWasChanged>(new Envelope(addressPositionWasChanged, positionChangedMetadata)))
                .Then(async ct =>
                {
                    var addressDetailItemV2 =
                        await ct.AddressDetailV2WithParent.FindAsync(addressWasProposedV2.AddressPersistentLocalId);

                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2!.Position.Should()
                        .BeEquivalentTo(addressPositionWasChanged.ExtendedWkbGeometry.ToByteArray());

                    addressDetailItemV2.Position.TryReadSrid(out var srid).Should().BeTrue();
                    srid.Should().Be(SystemReferenceId.SridLambert2008);
                });
        }

        [Fact]
        public async Task WhenAddressPositionCrsWasChanged_ThenPositionKeepsItsLambert2008Srid()
        {
            var addressWasProposedV2 = _fixture.Create<AddressWasProposedV2>();
            var proposedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasProposedV2.GetHash() }
            };

            var addressPositionCrsWasChanged = _fixture.Create<AddressPositionCrsWasChanged>();
            var positionChangedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressPositionCrsWasChanged.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(addressWasProposedV2, proposedMetadata)),
                    new Envelope<AddressPositionCrsWasChanged>(new Envelope(addressPositionCrsWasChanged, positionChangedMetadata)))
                .Then(async ct =>
                {
                    var addressDetailItemV2 =
                        await ct.AddressDetailV2WithParent.FindAsync(addressWasProposedV2.AddressPersistentLocalId);

                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2!.Position.Should()
                        .BeEquivalentTo(addressPositionCrsWasChanged.ExtendedWkbGeometry.ToByteArray());

                    addressDetailItemV2.Position.TryReadSrid(out var srid).Should().BeTrue();
                    srid.Should().Be(SystemReferenceId.SridLambert2008);
                });
        }

        protected override AddressDetailProjectionsV2WithParent CreateProjection()
            => new AddressDetailProjectionsV2WithParent();
    }
}
