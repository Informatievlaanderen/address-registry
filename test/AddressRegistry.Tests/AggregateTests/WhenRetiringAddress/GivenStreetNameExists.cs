namespace AddressRegistry.Tests.AggregateTests.WhenRetiringAddress
{
    using System.Collections.Generic;
    using System.Linq;
    using StreetName;
    using StreetName.Commands;
    using StreetName.Events;
    using StreetName.Exceptions;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using FluentAssertions;
    using global::AutoFixture;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenStreetNameExists : AddressRegistryTest
    {
        private readonly StreetNameStreamId _streamId;

        public GivenStreetNameExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());
            Fixture.Customize(new WithFixedAddressPersistentLocalId());
            _streamId = Fixture.Create<StreetNameStreamId>();
        }

        [Fact]
        public void WithCurrentAddress_ThenAddressWasRetired()
        {
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
            var command = new RetireAddress(
                Fixture.Create<StreetNamePersistentLocalId>(),
                addressPersistentLocalId,
                Fixture.Create<Provenance>());

            var addressWasProposedV2 = new AddressWasProposedV2(
                Fixture.Create<StreetNamePersistentLocalId>(),
                addressPersistentLocalId,
                parentPersistentLocalId: null,
                Fixture.Create<PostalCode>(),
                Fixture.Create<HouseNumber>(),
                boxNumber: null);

            var addressWasApproved = new AddressWasApproved(
                Fixture.Create<StreetNamePersistentLocalId>(),
                addressPersistentLocalId);

            ((ISetProvenance)addressWasProposedV2).SetProvenance(Fixture.Create<Provenance>());
            ((ISetProvenance)addressWasApproved).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasProposedV2,
                    addressWasApproved)
                .When(command)
                .Then(new Fact(_streamId,
                    new AddressWasRetiredV2(
                        Fixture.Create<StreetNamePersistentLocalId>(),
                        addressPersistentLocalId))));
        }

        [Fact]
        public void WithCurrentChildAddresses_ThenChildAddressesWereAlsoRetired()
        {
            var parentAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var firstChildAddressPersistentLocalId = new AddressPersistentLocalId(2);
            var secondChildAddressPersistentLocalId = new AddressPersistentLocalId(3);

            var command = new RetireAddress(
                Fixture.Create<StreetNamePersistentLocalId>(),
                parentAddressPersistentLocalId,
                Fixture.Create<Provenance>());

            var parentAddressWasProposedV2 = CreateCurrentAddressWasMigrated(parentAddressPersistentLocalId);
            var firstChildAddressWasProposedV2 = CreateCurrentAddressWasMigrated(
                firstChildAddressPersistentLocalId, parentAddressPersistentLocalId);
            var secondChildAddressWasProposedV2 = CreateCurrentAddressWasMigrated(
                secondChildAddressPersistentLocalId, parentAddressPersistentLocalId);

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    parentAddressWasProposedV2,
                    firstChildAddressWasProposedV2,
                    secondChildAddressWasProposedV2)
                .When(command)
                .Then(
                    new Fact(_streamId,
                        new AddressWasRetiredV2(
                        Fixture.Create<StreetNamePersistentLocalId>(),
                        parentAddressPersistentLocalId)),
                    new Fact(_streamId,
                        new AddressWasRetiredBecauseHouseNumberWasRetired(
                            Fixture.Create<StreetNamePersistentLocalId>(),
                            firstChildAddressPersistentLocalId)),
                    new Fact(_streamId,
                        new AddressWasRetiredBecauseHouseNumberWasRetired(
                            Fixture.Create<StreetNamePersistentLocalId>(),
                            secondChildAddressPersistentLocalId))));
        }

        private AddressWasMigratedToStreetName CreateCurrentAddressWasMigrated(
            AddressPersistentLocalId addressPersistentLocalId,
            AddressPersistentLocalId? parentAddressPersistentLocalId = null)
        {
            var addressWasMigrated = new AddressWasMigratedToStreetName(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressId>(),
                Fixture.Create<AddressStreetNameId>(),
                addressPersistentLocalId,
                AddressStatus.Current,
                Fixture.Create<HouseNumber>(),
                boxNumber: null,
                Fixture.Create<AddressGeometry>(),
                officiallyAssigned: true,
                postalCode: null,
                isCompleted: false,
                isRemoved: false,
                parentPersistentLocalId: parentAddressPersistentLocalId);

            ((ISetProvenance)addressWasMigrated).SetProvenance(Fixture.Create<Provenance>());

            return addressWasMigrated;
        }

        [Theory]
        [InlineData(AddressStatus.Proposed)]
        [InlineData(AddressStatus.Rejected)]
        public void WithNotCurrentChildAddress_ThenChildAddressWasNotRetired(AddressStatus addressStatus)
        {
            var parentAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var childAddressPersistentLocalId = new AddressPersistentLocalId(2);

            var command = new RetireAddress(
                Fixture.Create<StreetNamePersistentLocalId>(),
                parentAddressPersistentLocalId,
                Fixture.Create<Provenance>());

            var parentAddressWasProposedV2 = CreateCurrentAddressWasMigrated(parentAddressPersistentLocalId);
            var childAddressWasMigratedToStreetName = new AddressWasMigratedToStreetName(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressId>(),
                Fixture.Create<AddressStreetNameId>(),
                childAddressPersistentLocalId,
                addressStatus,
                Fixture.Create<HouseNumber>(),
                boxNumber: null,
                Fixture.Create<AddressGeometry>(),
                officiallyAssigned: true,
                postalCode: null,
                isCompleted: false,
                isRemoved: false,
                parentPersistentLocalId: parentAddressPersistentLocalId);
            ((ISetProvenance)childAddressWasMigratedToStreetName).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    parentAddressWasProposedV2,
                    childAddressWasMigratedToStreetName)
                .When(command)
                .Then(
                    new Fact(_streamId,
                        new AddressWasRetiredV2(
                        Fixture.Create<StreetNamePersistentLocalId>(),
                        parentAddressPersistentLocalId))));
        }

        [Fact]
        public void WithRemovedCurrentChildAddress_ThenChildAddressWasNotRetired()
        {
            var parentAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var childAddressPersistentLocalId = new AddressPersistentLocalId(2);

            var command = new RetireAddress(
                Fixture.Create<StreetNamePersistentLocalId>(),
                parentAddressPersistentLocalId,
                Fixture.Create<Provenance>());

            var parentAddressWasProposedV2 = CreateCurrentAddressWasMigrated(parentAddressPersistentLocalId);
            var childAddressWasMigratedToStreetName = new AddressWasMigratedToStreetName(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressId>(),
                Fixture.Create<AddressStreetNameId>(),
                childAddressPersistentLocalId,
                AddressStatus.Current,
                Fixture.Create<HouseNumber>(),
                boxNumber: null,
                Fixture.Create<AddressGeometry>(),
                officiallyAssigned: true,
                postalCode: null,
                isCompleted: false,
                isRemoved: true,
                parentPersistentLocalId: parentAddressPersistentLocalId);
            ((ISetProvenance)childAddressWasMigratedToStreetName).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    parentAddressWasProposedV2,
                    childAddressWasMigratedToStreetName)
                .When(command)
                .Then(
                    new Fact(_streamId,
                        new AddressWasRetiredV2(
                        Fixture.Create<StreetNamePersistentLocalId>(),
                        parentAddressPersistentLocalId))));
        }

        [Fact]
        public void WithoutProposedAddress_ThenThrowsAddressNotFoundException()
        {
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
            var command = new RetireAddress(
                Fixture.Create<StreetNamePersistentLocalId>(),
                addressPersistentLocalId,
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>())
                .When(command)
                .Throws(new AddressNotFoundException(addressPersistentLocalId)));
        }

        [Fact]
        public void OnRemovedAddress_ThenThrowsAddressIsRemovedException()
        {
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();

            var streetNameWasImported = new StreetNameWasImported(
                streetNamePersistentLocalId,
                Fixture.Create<MunicipalityId>(),
                StreetNameStatus.Current);
            ((ISetProvenance)streetNameWasImported).SetProvenance(Fixture.Create<Provenance>());

            var migrateRemovedAddressToStreetName = new AddressWasMigratedToStreetName(
                streetNamePersistentLocalId,
                Fixture.Create<AddressId>(),
                Fixture.Create<AddressStreetNameId>(),
                addressPersistentLocalId,
                AddressStatus.Proposed,
                Fixture.Create<HouseNumber>(),
                boxNumber: null,
                Fixture.Create<AddressGeometry>(),
                officiallyAssigned: true,
                postalCode: null,
                isCompleted: false,
                isRemoved: true,
                parentPersistentLocalId: null);
            ((ISetProvenance)migrateRemovedAddressToStreetName).SetProvenance(Fixture.Create<Provenance>());

            var retireAddress = new RetireAddress(
                streetNamePersistentLocalId,
                addressPersistentLocalId,
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    streetNameWasImported,
                    migrateRemovedAddressToStreetName)
                .When(retireAddress)
                .Throws(new AddressIsRemovedException(addressPersistentLocalId)));
        }

        [Theory]
        [InlineData(AddressStatus.Rejected)]
        [InlineData(AddressStatus.Proposed)]
        public void AddressWithInvalidStatuses_ThenThrowsAddressCannotBeRetiredException(AddressStatus addressStatus)
        {
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();

            var streetNameWasImported = new StreetNameWasImported(
                streetNamePersistentLocalId,
                Fixture.Create<MunicipalityId>(),
                StreetNameStatus.Current);
            ((ISetProvenance)streetNameWasImported).SetProvenance(Fixture.Create<Provenance>());

            var migrateAddressWithStatusCurrent = new AddressWasMigratedToStreetName(
                streetNamePersistentLocalId,
                Fixture.Create<AddressId>(),
                Fixture.Create<AddressStreetNameId>(),
                addressPersistentLocalId,
                addressStatus,
                Fixture.Create<HouseNumber>(),
                boxNumber: null,
                Fixture.Create<AddressGeometry>(),
                officiallyAssigned: true,
                postalCode: null,
                isCompleted: false,
                isRemoved: false,
                parentPersistentLocalId: null);
            ((ISetProvenance)migrateAddressWithStatusCurrent).SetProvenance(Fixture.Create<Provenance>());

            var approveAddress = new RetireAddress(
                streetNamePersistentLocalId,
                addressPersistentLocalId,
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    streetNameWasImported,
                    migrateAddressWithStatusCurrent)
                .When(approveAddress)
                .Throws(new AddressCannotBeRetiredException(addressStatus)));
        }

        [Fact]
        public void WithAlreadyRetiredAddress_ThenNone()
        {
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();

            var command = new RetireAddress(
                streetNamePersistentLocalId,
                addressPersistentLocalId,
                Fixture.Create<Provenance>());

            var addressWasProposedV2 = new AddressWasProposedV2(
                streetNamePersistentLocalId,
                addressPersistentLocalId,
                parentPersistentLocalId: null,
                Fixture.Create<PostalCode>(),
                Fixture.Create<HouseNumber>(),
                boxNumber: null);

            var addressWasApproved = new AddressWasApproved(
                streetNamePersistentLocalId,
                addressPersistentLocalId);

            var addressWasRetiredV2 = new AddressWasRetiredV2(
                streetNamePersistentLocalId,
                addressPersistentLocalId);

            ((ISetProvenance)addressWasProposedV2).SetProvenance(Fixture.Create<Provenance>());
            ((ISetProvenance)addressWasApproved).SetProvenance(Fixture.Create<Provenance>());
            ((ISetProvenance)addressWasRetiredV2).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasProposedV2,
                    addressWasApproved,
                    addressWasRetiredV2)
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void StateCheck()
        {
            // Arrange
            var parentAddressPersistentLocalId = new AddressPersistentLocalId(123);
            var childAddressPersistentLocalId = new AddressPersistentLocalId(456);
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();

            var parentAddressWasMigratedToStreetName = new AddressWasMigratedToStreetName(
                streetNamePersistentLocalId,
                Fixture.Create<AddressId>(),
                Fixture.Create<AddressStreetNameId>(),
                parentAddressPersistentLocalId,
                AddressStatus.Current,
                Fixture.Create<HouseNumber>(),
                boxNumber: null,
                Fixture.Create<AddressGeometry>(),
                officiallyAssigned: true,
                postalCode: null,
                isCompleted: false,
                isRemoved: false,
                parentPersistentLocalId: null);
            ((ISetProvenance)parentAddressWasMigratedToStreetName).SetProvenance(Fixture.Create<Provenance>());

            var childAddressWasMigratedToStreetName = new AddressWasMigratedToStreetName(
                streetNamePersistentLocalId,
                Fixture.Create<AddressId>(),
                Fixture.Create<AddressStreetNameId>(),
                childAddressPersistentLocalId,
                AddressStatus.Current,
                Fixture.Create<HouseNumber>(),
                boxNumber: null,
                Fixture.Create<AddressGeometry>(),
                officiallyAssigned: true,
                postalCode: null,
                isCompleted: false,
                isRemoved: false,
                parentAddressPersistentLocalId);
            ((ISetProvenance)childAddressWasMigratedToStreetName).SetProvenance(Fixture.Create<Provenance>());

            var sut = new StreetNameFactory(NoSnapshotStrategy.Instance).Create();
            sut.Initialize(new List<object> { parentAddressWasMigratedToStreetName, childAddressWasMigratedToStreetName });

            // Act
            sut.RetireAddress(parentAddressPersistentLocalId);

            // Assert
            var parentAddress = sut.StreetNameAddresses.First(x => x.AddressPersistentLocalId == parentAddressPersistentLocalId);
            var childAddress = sut.StreetNameAddresses.First(x => x.AddressPersistentLocalId == childAddressPersistentLocalId);

            parentAddress.Status.Should().Be(AddressStatus.Retired);
            childAddress.Status.Should().Be(AddressStatus.Retired);
        }
    }
}
