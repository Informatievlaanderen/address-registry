namespace AddressRegistry.Tests.ProjectionTests.Legacy
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AddressRegistry.StreetName;
    using AddressRegistry.StreetName.Events;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using EventExtensions;
    using FluentAssertions;
    using global::AutoFixture;
    using Microsoft.EntityFrameworkCore;
    using Projections.Legacy.AddressSyndication;
    using Xunit;

    /// <summary>
    /// The street name is part of the address, so a change to its names or homonym additions is published as a
    /// new version of every address on that street name, house numbers and box numbers alike.
    /// </summary>
    public class AddressSyndicationStreetNameTests : AddressLegacyProjectionTest<AddressSyndicationProjections>
    {
        private readonly Fixture _fixture;
        private readonly AddressPersistentLocalId _houseNumberPersistentLocalId = new(1);
        private readonly AddressPersistentLocalId _boxNumberPersistentLocalId = new(2);

        public AddressSyndicationStreetNameTests()
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedStreetNamePersistentLocalId());
            _fixture.Customize(new WithValidHouseNumber());
            _fixture.Customize(new WithValidBoxNumber());
            _fixture.Customize(new WithExtendedWkbGeometry());
        }

        [Fact]
        public Task WhenStreetNameNamesWereChanged_ThenEveryAddressGetsANewVersion()
            => AssertEveryAddressGetsANewVersion(new StreetNameNamesWereChanged(
                _fixture.Create<StreetNamePersistentLocalId>(),
                new Dictionary<string, string> { { "Dutch", "Kerkstraat" } },
                [_houseNumberPersistentLocalId, _boxNumberPersistentLocalId]));

        [Fact]
        public Task WhenStreetNameNamesWereCorrected_ThenEveryAddressGetsANewVersion()
            => AssertEveryAddressGetsANewVersion(new StreetNameNamesWereCorrected(
                _fixture.Create<StreetNamePersistentLocalId>(),
                new Dictionary<string, string> { { "Dutch", "Kerkstraat" } },
                [_houseNumberPersistentLocalId, _boxNumberPersistentLocalId]));

        [Fact]
        public Task WhenStreetNameHomonymAdditionsWereCorrected_ThenEveryAddressGetsANewVersion()
            => AssertEveryAddressGetsANewVersion(new StreetNameHomonymAdditionsWereCorrected(
                _fixture.Create<StreetNamePersistentLocalId>(),
                new Dictionary<string, string> { { "Dutch", "KS" } },
                [_houseNumberPersistentLocalId, _boxNumberPersistentLocalId]));

        [Fact]
        public Task WhenStreetNameHomonymAdditionsWereRemoved_ThenEveryAddressGetsANewVersion()
            => AssertEveryAddressGetsANewVersion(new StreetNameHomonymAdditionsWereRemoved(
                _fixture.Create<StreetNamePersistentLocalId>(),
                ["Dutch"],
                [_houseNumberPersistentLocalId, _boxNumberPersistentLocalId]));

        private async Task AssertEveryAddressGetsANewVersion<T>(T streetNameEvent)
            where T : IStreetNameEvent
        {
            streetNameEvent.SetProvenance(_fixture.Create<Provenance>());

            var houseNumberWasMigrated = _fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress(addressStatus: AddressStatus.Current)
                .WithAddressPersistentLocalId(_houseNumberPersistentLocalId);

            var boxNumberWasMigrated = _fixture.Create<AddressWasMigratedToStreetName>()
                .WithAddressPersistentLocalId(_boxNumberPersistentLocalId)
                .AsBoxNumberAddress(_houseNumberPersistentLocalId);

            await Sut
                .Given(
                    CreateEnvelope(houseNumberWasMigrated, 1L),
                    CreateEnvelope(boxNumberWasMigrated, 2L),
                    CreateEnvelope(streetNameEvent, 3L))
                .Then(async context =>
                {
                    var syndicationItems = await context.AddressSyndication
                        .Where(x => x.Position == 3L)
                        .ToListAsync();

                    syndicationItems.Select(x => x.PersistentLocalId).Should().BeEquivalentTo(
                        [(int)_houseNumberPersistentLocalId, (int)_boxNumberPersistentLocalId]);

                    foreach (var syndicationItem in syndicationItems)
                    {
                        syndicationItem.ChangeType.Should().Be(typeof(T).Name);
                        syndicationItem.LastChangedOn.Should().Be(streetNameEvent.Provenance.Timestamp);
                        syndicationItem.EventDataAsXml.Should().NotBeEmpty();
                    }

                    var houseNumberItem = syndicationItems.Single(x => x.PersistentLocalId == _houseNumberPersistentLocalId);
                    houseNumberItem.HouseNumber.Should().Be(houseNumberWasMigrated.HouseNumber);
                    houseNumberItem.BoxNumber.Should().BeNull();

                    var boxNumberItem = syndicationItems.Single(x => x.PersistentLocalId == _boxNumberPersistentLocalId);
                    boxNumberItem.HouseNumber.Should().Be(boxNumberWasMigrated.HouseNumber);
                    boxNumberItem.BoxNumber.Should().Be(boxNumberWasMigrated.BoxNumber);
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
