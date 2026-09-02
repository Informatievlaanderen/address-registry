namespace AddressRegistry.Tests.ProjectionTests.Legacy
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AddressRegistry.StreetName;
    using AddressRegistry.StreetName.Events;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using EventExtensions;
    using FluentAssertions;
    using global::AutoFixture;
    using Projections.Legacy.AddressSyndication;
    using Xunit;

    /// <summary>
    /// The transformation is published in the syndication feed — consumers see the event and the position in
    /// the reference system the event store now holds — but it is not a change to the address, so the item's
    /// version does not move. See ADR 0005.
    /// </summary>
    public class AddressSyndicationLambert2008Tests : AddressLegacyProjectionTest<AddressSyndicationProjections>
    {
        private readonly Fixture _fixture;

        public AddressSyndicationLambert2008Tests()
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedAddressPersistentLocalId());
            _fixture.Customize(new WithFixedStreetNamePersistentLocalId());
            _fixture.Customize(new WithValidHouseNumber());
            _fixture.Customize(new WithValidBoxNumber());
            _fixture.Customize(new WithExtendedWkbGeometry());
        }

        [Fact]
        public async Task WhenAddressPositionCrsWasChanged_ThenTheEventIsPublishedWithoutANewVersion()
        {
            var lambert2008Position = GeometryHelpers.CreateEwkbFromWkt(
                WithExtendedWkbGeometryLambert2008.PointWkt, SystemReferenceId.SridLambert2008);

            var addressWasMigrated = _fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress(addressStatus: AddressStatus.Current);

            var addressPositionCrsWasChanged = new AddressPositionCrsWasChanged(
                _fixture.Create<StreetNamePersistentLocalId>(),
                _fixture.Create<AddressPersistentLocalId>(),
                addressWasMigrated.GeometryMethod,
                addressWasMigrated.GeometrySpecification,
                lambert2008Position);
            ((ISetProvenance)addressPositionCrsWasChanged).SetProvenance(_fixture.Create<Provenance>());

            await Sut
                .Given(
                    CreateEnvelope(addressWasMigrated, 1L),
                    CreateEnvelope(addressPositionCrsWasChanged, 2L))
                .Then(async context =>
                {
                    var syndicationItem = await context.AddressSyndication.FindAsync(2L);
                    syndicationItem.Should().NotBeNull();
                    syndicationItem!.ChangeType.Should().Be(nameof(AddressPositionCrsWasChanged));
                    syndicationItem.PointPosition.Should().BeEquivalentTo(lambert2008Position.ToByteArray());
                    syndicationItem.EventDataAsXml.Should().NotBeEmpty();
                    syndicationItem.RecordCreatedAt.Should().Be(addressWasMigrated.Provenance.Timestamp);

                    syndicationItem.LastChangedOn.Should().Be(addressWasMigrated.Provenance.Timestamp);
                    syndicationItem.LastChangedOn.Should()
                        .NotBe(addressPositionCrsWasChanged.Provenance.Timestamp);
                });
        }

        /// <summary>
        /// A removed address stays out of the feed. Consumers were told it was removed; the transformation is
        /// not something to tell them about, least of all for that address. See ADR 0005.
        /// </summary>
        [Fact]
        public async Task WhenAddressPositionCrsWasChangedForARemovedAddress_ThenNothingIsPublished()
        {
            var addressWasMigrated = _fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress(addressStatus: AddressStatus.Current);

            var addressWasRemoved = _fixture.Create<AddressWasRemovedV2>();

            var addressPositionCrsWasChanged = new AddressPositionCrsWasChanged(
                _fixture.Create<StreetNamePersistentLocalId>(),
                _fixture.Create<AddressPersistentLocalId>(),
                addressWasMigrated.GeometryMethod,
                addressWasMigrated.GeometrySpecification,
                GeometryHelpers.CreateEwkbFromWkt(
                    WithExtendedWkbGeometryLambert2008.PointWkt, SystemReferenceId.SridLambert2008));
            ((ISetProvenance)addressPositionCrsWasChanged).SetProvenance(_fixture.Create<Provenance>());

            await Sut
                .Given(
                    CreateEnvelope(addressWasMigrated, 1L),
                    CreateEnvelope(addressWasRemoved, 2L),
                    CreateEnvelope(addressPositionCrsWasChanged, 3L))
                .Then(async context =>
                {
                    (await context.AddressSyndication.FindAsync(3L)).Should().BeNull();

                    var latest = await context.AddressSyndication.FindAsync(2L);
                    latest.Should().NotBeNull();
                    latest!.ChangeType.Should().Be(nameof(AddressWasRemovedV2));
                });
        }

        private static Envelope<T> CreateEnvelope<T>(T message, long position)
            where T : IStreetNameEvent
            => new Envelope<T>(new Envelope(message, new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, message.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, message.GetType().Name }
            }));

        protected override AddressSyndicationProjections CreateProjection() => new();
    }
}
