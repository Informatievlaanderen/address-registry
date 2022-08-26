namespace AddressRegistry.Tests.AggregateTests.SnapshotTests
{
    using System.Collections.Generic;
    using System.Linq;
    using Api.BackOffice.Abstractions;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using FluentAssertions;
    using global::AutoFixture;
    using StreetName;
    using StreetName.DataStructures;
    using StreetName.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class TakeStreetNameSnapshotTests : AddressRegistryTest
    {
        public TakeStreetNameSnapshotTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());
        }

        [Theory]
        [InlineData(StreetNameStatus.Proposed)]
        [InlineData(StreetNameStatus.Current)]
        [InlineData(StreetNameStatus.Rejected)]
        [InlineData(StreetNameStatus.Retired)]
        public void StreetNameWasImportedIsSavedInSnapshot(StreetNameStatus expectedStatus)
        {
            Fixture.Register(() => expectedStatus);

            var aggregate = new StreetNameFactory(IntervalStrategy.Default).Create();

            aggregate.Initialize(new List<object>
            {
                Fixture.Create<StreetNameWasImported>()
            });

            var snapshot = aggregate.TakeSnapshot();

            snapshot.Should().BeOfType<StreetNameSnapshot>();
            var streetNameSnapshot = (StreetNameSnapshot)snapshot;

            streetNameSnapshot.StreetNamePersistentLocalId.Should().Be(aggregate.PersistentLocalId);
            streetNameSnapshot.MigratedNisCode.Should().BeNull();
            streetNameSnapshot.IsRemoved.Should().BeFalse();
            streetNameSnapshot.StreetNameStatus.Should().Be(expectedStatus);
            streetNameSnapshot.Addresses.Should().BeEmpty();
        }

        [Theory]
        [InlineData(StreetNameStatus.Proposed)]
        [InlineData(StreetNameStatus.Current)]
        [InlineData(StreetNameStatus.Rejected)]
        [InlineData(StreetNameStatus.Retired)]
        public void MigratedStreetNameWasImportedIsSavedInSnapshot(StreetNameStatus expectedStatus)
        {
            Fixture.Register(() => expectedStatus);

            var aggregate = new StreetNameFactory(IntervalStrategy.Default).Create();

            aggregate.Initialize(new List<object>
            {
                Fixture.Create<MigratedStreetNameWasImported>()
            });

            var snapshot = aggregate.TakeSnapshot();

            snapshot.Should().BeOfType<StreetNameSnapshot>();
            var streetNameSnapshot = (StreetNameSnapshot)snapshot;

            streetNameSnapshot.StreetNamePersistentLocalId.Should().Be(aggregate.PersistentLocalId);
            streetNameSnapshot.MigratedNisCode.Should().Be(aggregate.MigratedNisCode);
            streetNameSnapshot.IsRemoved.Should().BeFalse();
            streetNameSnapshot.StreetNameStatus.Should().Be(expectedStatus);
            streetNameSnapshot.Addresses.Should().BeEmpty();
        }

        [Fact]
        public void StreetNameWasRemovedIsSavedInSnapshot()
        {
            var aggregate = new StreetNameFactory(IntervalStrategy.Default).Create();

            aggregate.Initialize(new List<object>
            {
                Fixture.Create<StreetNameWasImported>(),
                Fixture.Create<StreetNameWasRemoved>()
            });

            var snapshot = aggregate.TakeSnapshot();

            snapshot.Should().BeOfType<StreetNameSnapshot>();
            var streetNameSnapshot = (StreetNameSnapshot)snapshot;

            streetNameSnapshot.IsRemoved.Should().BeTrue();
        }

        [Fact]
        public void StreetNameWasApprovedIsSavedInSnapshot()
        {
            Fixture.Register(() => StreetNameStatus.Proposed);

            var aggregate = new StreetNameFactory(IntervalStrategy.Default).Create();

            aggregate.Initialize(new List<object>
            {
                Fixture.Create<StreetNameWasImported>(),
                Fixture.Create<StreetNameWasApproved>()
            });

            var snapshot = aggregate.TakeSnapshot();

            snapshot.Should().BeOfType<StreetNameSnapshot>();
            var streetNameSnapshot = (StreetNameSnapshot)snapshot;

            streetNameSnapshot.StreetNameStatus.Should().Be(StreetNameStatus.Current);
        }

        [Fact]
        public void AddressWasMigratedIsSavedInSnapshot()
        {
            var aggregate = new StreetNameFactory(IntervalStrategy.Default).Create();
            var addressHouseNumberWasMigratedToStreetName = new AddressWasMigratedToStreetName(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressId>(),
                Fixture.Create<AddressStreetNameId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                Fixture.Create<AddressStatus>(),
                Fixture.Create<HouseNumber>(),
                null,
                Fixture.Create<AddressGeometry>(),
                Fixture.Create<bool>(),
                Fixture.Create<PostalCode>(),
                true,
                Fixture.Create<bool>(),
                null);

            ((ISetProvenance)addressHouseNumberWasMigratedToStreetName).SetProvenance(Fixture.Create<Provenance>());

            var addressBoxNumberWasMigratedToStreetName = new AddressWasMigratedToStreetName(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressId>(),
                Fixture.Create<AddressStreetNameId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                Fixture.Create<AddressStatus>(),
                Fixture.Create<HouseNumber>(),
                Fixture.Create<BoxNumber>(),
                Fixture.Create<AddressGeometry>(),
                Fixture.Create<bool>(),
                Fixture.Create<PostalCode>(),
                true,
                Fixture.Create<bool>(),
                new AddressPersistentLocalId(addressHouseNumberWasMigratedToStreetName.AddressPersistentLocalId));

            ((ISetProvenance)addressBoxNumberWasMigratedToStreetName).SetProvenance(Fixture.Create<Provenance>());


            aggregate.Initialize(new List<object>
            {
                Fixture.Create<StreetNameWasImported>(),
                addressHouseNumberWasMigratedToStreetName,
                addressBoxNumberWasMigratedToStreetName
            });

            var snapshot = aggregate.TakeSnapshot();

            snapshot.Should().BeOfType<StreetNameSnapshot>();
            var streetNameSnapshot = (StreetNameSnapshot)snapshot;

            streetNameSnapshot.Addresses.Should().NotBeEmpty();

            AssertExpectedAddressDataFromAddressWasMigrated(streetNameSnapshot.Addresses.First(x => x.ParentId is null), addressHouseNumberWasMigratedToStreetName);
            AssertExpectedAddressDataFromAddressWasMigrated(streetNameSnapshot.Addresses.First(x => x.ParentId.HasValue), addressBoxNumberWasMigratedToStreetName);
        }

        private static void AssertExpectedAddressDataFromAddressWasMigrated(
            AddressData address,
            AddressWasMigratedToStreetName addressHouseNumberWasMigratedToStreetName)
        {
            address.AddressPersistentLocalId.Should().Be(addressHouseNumberWasMigratedToStreetName.AddressPersistentLocalId);
            address.Status.Should().Be(addressHouseNumberWasMigratedToStreetName.Status);
            address.PostalCode.Should().Be(addressHouseNumberWasMigratedToStreetName.PostalCode);
            address.HouseNumber.Should().Be(addressHouseNumberWasMigratedToStreetName.HouseNumber);
            address.BoxNumber.Should().Be(addressHouseNumberWasMigratedToStreetName.BoxNumber);
            address.IsOfficiallyAssigned.Should().Be(addressHouseNumberWasMigratedToStreetName.OfficiallyAssigned);
            address.IsRemoved.Should().Be(addressHouseNumberWasMigratedToStreetName.IsRemoved);
            address.ParentId.Should().Be(addressHouseNumberWasMigratedToStreetName.ParentPersistentLocalId);
            address.LastEventHash.Should().Be(addressHouseNumberWasMigratedToStreetName.GetHash());
            address.LastProvenanceData.Should().Be(addressHouseNumberWasMigratedToStreetName.Provenance);
        }

        [Fact]
        public void AddressWasProposedIsSavedInSnapshot()
        {
            var aggregate = new StreetNameFactory(IntervalStrategy.Default).Create();
            var addressWasProposedV2 = ProposeHouseNumber();

            aggregate.Initialize(new List<object>
            {
                Fixture.Create<StreetNameWasImported>(),
                addressWasProposedV2
            });

            var snapshot = aggregate.TakeSnapshot();

            snapshot.Should().BeOfType<StreetNameSnapshot>();
            var streetNameSnapshot = (StreetNameSnapshot)snapshot;

            streetNameSnapshot.Addresses.Should().NotBeEmpty();

            var address = streetNameSnapshot.Addresses.First();
            address.AddressPersistentLocalId.Should().Be(addressWasProposedV2.AddressPersistentLocalId);
            address.Status.Should().Be(AddressStatus.Proposed);
            address.PostalCode.Should().Be(addressWasProposedV2.PostalCode);
            address.HouseNumber.Should().Be(addressWasProposedV2.HouseNumber);
            address.BoxNumber.Should().BeNullOrEmpty();
            address.IsOfficiallyAssigned.Should().BeTrue();
            address.IsRemoved.Should().BeFalse();
            address.ParentId.Should().BeNull();
            address.LastEventHash.Should().Be(addressWasProposedV2.GetHash());
            address.LastProvenanceData.Should().Be(addressWasProposedV2.Provenance);
        }

        [Fact]
        public void AddressWasApprovedIsSavedInSnapshot()
        {
            var aggregate = new StreetNameFactory(IntervalStrategy.Default).Create();
            var addressWasProposedV2 = ProposeHouseNumber();
            var addressWasApproved = ApproveProposedHouseNumber(addressWasProposedV2);

            aggregate.Initialize(new List<object>
            {
                Fixture.Create<StreetNameWasImported>(),
                addressWasProposedV2,
                addressWasApproved
            });

            var snapshot = aggregate.TakeSnapshot();

            snapshot.Should().BeOfType<StreetNameSnapshot>();
            var streetNameSnapshot = (StreetNameSnapshot)snapshot;

            streetNameSnapshot.Addresses.Should().NotBeEmpty();

            var address = streetNameSnapshot.Addresses.First();
            address.AddressPersistentLocalId.Should().Be(addressWasProposedV2.AddressPersistentLocalId);
            address.Status.Should().Be(AddressStatus.Current);
            address.PostalCode.Should().Be(addressWasProposedV2.PostalCode);
            address.HouseNumber.Should().Be(addressWasProposedV2.HouseNumber);
            address.BoxNumber.Should().BeNullOrEmpty();
            address.ParentId.Should().BeNull();
            address.IsOfficiallyAssigned.Should().BeTrue();
            address.IsRemoved.Should().BeFalse();
            address.LastEventHash.Should().Be(addressWasApproved.GetHash());
            address.LastProvenanceData.Should().Be(addressWasApproved.Provenance);
        }

        private AddressWasApproved ApproveProposedHouseNumber(AddressWasProposedV2 addressWasProposedV2)
        {
            var addressWasApproved = new AddressWasApproved(
                Fixture.Create<StreetNamePersistentLocalId>(),
                new AddressPersistentLocalId(addressWasProposedV2.AddressPersistentLocalId));

            ((ISetProvenance)addressWasApproved).SetProvenance(Fixture.Create<Provenance>());
            return addressWasApproved;
        }

        private AddressWasProposedV2 ProposeHouseNumber()
        {
            var addressWasProposedV2 = new AddressWasProposedV2(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                null,
                Fixture.Create<PostalCode>(),
                Fixture.Create<HouseNumber>(),
                null,
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Entry,
                GeometryHelpers.PointGeometry.ToExtendedWkbGeometry());
            ((ISetProvenance)addressWasProposedV2).SetProvenance(Fixture.Create<Provenance>());
            return addressWasProposedV2;
        }
    }
}
