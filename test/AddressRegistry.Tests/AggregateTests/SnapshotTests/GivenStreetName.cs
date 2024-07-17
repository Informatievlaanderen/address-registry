namespace AddressRegistry.Tests.AggregateTests.SnapshotTests
{
    using System.Threading;
    using System.Threading.Tasks;
    using Api.BackOffice.Abstractions;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using FluentAssertions;
    using global::AutoFixture;
    using Newtonsoft.Json;
    using NodaTime;
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
            Fixture.Customize(new WithExtendedWkbGeometry());
            _streamId = Fixture.Create<StreetNameStreamId>();
        }

        [Fact]
        public async Task ThenSnapshotWasCreated()
        {
            Fixture.Register(() => (ISnapshotStrategy)IntervalStrategy.SnapshotEvery(1));

            var houseNumber = new HouseNumber("1");
            var boxNumber = Fixture.Create<BoxNumber>();
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
            var postalCode = Fixture.Create<PostalCode>();
            var provenance = Fixture.Create<Provenance>();

            var migratedStreetNameWasImported = Fixture.Create<MigratedStreetNameWasImported>();

            var parentAddressWasProposed = new AddressWasProposedV2(
                streetNamePersistentLocalId,
                Fixture.Create<AddressPersistentLocalId>(),
                parentPersistentLocalId: null,
                postalCode,
                houseNumber,
                boxNumber: null,
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Entry,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());
            ((ISetProvenance)parentAddressWasProposed).SetProvenance(provenance);

            var proposeChildAddress = new ProposeAddress(
                streetNamePersistentLocalId,
                postalCode,
                Fixture.Create<MunicipalityId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                houseNumber,
                boxNumber,
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Entry,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry(),
                provenance);

            var addressWasProposedV2 = new AddressWasProposedV2(
                proposeChildAddress.StreetNamePersistentLocalId,
                proposeChildAddress.AddressPersistentLocalId,
                new AddressPersistentLocalId(parentAddressWasProposed.AddressPersistentLocalId),
                proposeChildAddress.PostalCode,
                proposeChildAddress.HouseNumber,
                proposeChildAddress.BoxNumber,
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Entry,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());
            ((ISetProvenance)addressWasProposedV2).SetProvenance(provenance);

            var addressWasProposedForMunicipalityMerger = new AddressWasProposedForMunicipalityMerger(
                streetNamePersistentLocalId,
                Fixture.Create<AddressPersistentLocalId>(),
                null,
                postalCode,
                new HouseNumber("2"),
                null,
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Entry,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry(),
                true,
                Fixture.Create<AddressPersistentLocalId>());
            ((ISetProvenance)addressWasProposedForMunicipalityMerger).SetProvenance(provenance);

            Assert(new Scenario()
                .Given(_streamId,
                    migratedStreetNameWasImported,
                    parentAddressWasProposed,
                    addressWasProposedForMunicipalityMerger)
                .When(proposeChildAddress)
                .Then(new Fact(_streamId, addressWasProposedV2)));

            var expectedSnapshot = SnapshotBuilder.CreateDefaultSnapshot(streetNamePersistentLocalId)
                .WithMunicipalityId(Fixture.Create<MunicipalityId>())
                .WithMigratedNisCode(migratedStreetNameWasImported.NisCode)
                .WithAddress(new AddressPersistentLocalId(parentAddressWasProposed.AddressPersistentLocalId),
                    AddressStatus.Proposed,
                    postalCode,
                    houseNumber,
                    null,
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry(),
                    null,
                    null,
                    parentAddressWasProposed.GetHash(),
                    new ProvenanceData(provenance))
                .WithAddress(
                    new AddressPersistentLocalId(addressWasProposedForMunicipalityMerger.AddressPersistentLocalId),
                    AddressStatus.Proposed,
                    postalCode,
                    new HouseNumber(addressWasProposedForMunicipalityMerger.HouseNumber),
                    null,
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry(),
                    new AddressPersistentLocalId(parentAddressWasProposed.AddressPersistentLocalId),
                    new AddressPersistentLocalId(addressWasProposedForMunicipalityMerger.MergedAddressPersistentLocalId),
                    addressWasProposedV2.GetHash(),
                    new ProvenanceData(provenance))
                .WithAddress(proposeChildAddress.AddressPersistentLocalId,
                    AddressStatus.Proposed,
                    postalCode,
                    houseNumber,
                    boxNumber,
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Entry,
                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry(),
                    new AddressPersistentLocalId(parentAddressWasProposed.AddressPersistentLocalId),
                    null,
                    addressWasProposedV2.GetHash(),
                    new ProvenanceData(provenance));

            var snapshotStore = (ISnapshotStore)Container.Resolve(typeof(ISnapshotStore));
            var latestSnapshot = await snapshotStore.FindLatestSnapshotAsync(_streamId, CancellationToken.None);

            latestSnapshot.Should().NotBeNull();
            var snapshot = JsonConvert.DeserializeObject<StreetNameSnapshot>(latestSnapshot!.Data, EventSerializerSettings);

            snapshot.Should().BeEquivalentTo(expectedSnapshot, options =>
            {
                options.Excluding(x => x.Path.EndsWith("LastEventHash"));
                options.Excluding(x => x.Type == typeof(Instant));
                return options;
            });
        }
    }
}
