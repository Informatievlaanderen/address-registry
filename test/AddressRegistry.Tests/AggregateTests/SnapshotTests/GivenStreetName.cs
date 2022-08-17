namespace AddressRegistry.Tests.AggregateTests.SnapshotTests
{
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using FluentAssertions;
    using global::AutoFixture;
    using StreetName;
    using StreetName.Commands;
    using StreetName.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenEvents : AddressRegistryTest
    {
        private readonly StreetNameStreamId _streamId;

        public GivenEvents(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());
            Fixture.Customize(new WithFixedMunicipalityId());
            Fixture.Customize(new WithFixedValidHouseNumber());
            _streamId = Fixture.Create<StreetNameStreamId>();
        }

        [Fact]
        public async Task ThenSnapshotWasCreated()
        {
            Fixture.Register(() => (ISnapshotStrategy)IntervalStrategy.SnapshotEvery(1));

            var houseNumber = Fixture.Create<HouseNumber>();
            var boxNumber = Fixture.Create<BoxNumber>();
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
            var postalCode = Fixture.Create<PostalCode>();
            var provenance = Fixture.Create<Provenance>();

            var parentAddressWasProposed = new AddressWasProposedV2(
                streetNamePersistentLocalId,
                Fixture.Create<AddressPersistentLocalId>(),
                parentPersistentLocalId: null,
                postalCode,
                houseNumber,
                boxNumber: null);
            ((ISetProvenance)parentAddressWasProposed).SetProvenance(provenance);

            var proposeChildAddress = new ProposeAddress(
                streetNamePersistentLocalId,
                postalCode,
                Fixture.Create<MunicipalityId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                houseNumber,
                boxNumber,
                PositionGeometryMethod.DerivedFromObject,
                provenance);

            var addressWasProposedV2 = new AddressWasProposedV2(
                proposeChildAddress.StreetNamePersistentLocalId,
                proposeChildAddress.AddressPersistentLocalId,
                new AddressPersistentLocalId(parentAddressWasProposed.AddressPersistentLocalId),
                proposeChildAddress.PostalCode,
                proposeChildAddress.HouseNumber,
                proposeChildAddress.BoxNumber);
            ((ISetProvenance)addressWasProposedV2).SetProvenance(provenance);

            var migratedStreetNameWasImported = Fixture.Create<MigratedStreetNameWasImported>();
            var expectedSnapshot = SnapshotBuilder.CreateDefaultSnapshot(streetNamePersistentLocalId)
                .WithMigratedNisCode(migratedStreetNameWasImported.NisCode)
                .WithAddress(new AddressPersistentLocalId(parentAddressWasProposed.AddressPersistentLocalId),
                    AddressStatus.Proposed,
                    postalCode,
                    houseNumber,
                    null,
                    null,
                    parentAddressWasProposed.GetHash(),
                    new ProvenanceData(provenance))
                .WithAddress(proposeChildAddress.AddressPersistentLocalId,
                    AddressStatus.Proposed,
                    postalCode,
                    houseNumber,
                    boxNumber,
                    new AddressPersistentLocalId(parentAddressWasProposed.AddressPersistentLocalId),
                    addressWasProposedV2.GetHash(),
                    new ProvenanceData(provenance))
                .Build(2, EventSerializerSettings);

            Assert(new Scenario()
                .Given(_streamId,
                    migratedStreetNameWasImported,
                    parentAddressWasProposed)
                .When(proposeChildAddress)
                .Then(new Fact(_streamId, addressWasProposedV2)));

            var snapshotStore = (ISnapshotStore)Container.Resolve(typeof(ISnapshotStore));
            var latestSnapshot = await snapshotStore.FindLatestSnapshotAsync(_streamId, CancellationToken.None);

            latestSnapshot.Should().BeEquivalentTo(expectedSnapshot);
        }
    }
}
