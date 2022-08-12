namespace AddressRegistry.Tests.AggregateTests.WhenRejectingAddress
{
    using System.Collections.Generic;
    using System.Linq;
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
    using StreetName.Exceptions;
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
        public void WithProposedAddress_ThenAddressWasRejected()
        {
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
            var command = new RejectAddress(
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

            ((ISetProvenance)addressWasProposedV2).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasProposedV2)
                .When(command)
                .Then(new Fact(_streamId,
                    new AddressWasRejected(
                        Fixture.Create<StreetNamePersistentLocalId>(),
                        addressPersistentLocalId))));
        }

        [Fact]
        public void WithProposedChildAddresses_ThenChildAddressesWereAlsoRejected()
        {
            var parentAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var firstChildAddressPersistentLocalId = new AddressPersistentLocalId(2);
            var secondChildAddressPersistentLocalId = new AddressPersistentLocalId(3);

            var command = new RejectAddress(
                Fixture.Create<StreetNamePersistentLocalId>(),
                parentAddressPersistentLocalId,
                Fixture.Create<Provenance>());

            var parentAddressWasProposedV2 = CreateAddressWasProposed(parentAddressPersistentLocalId);
            var firstChildAddressWasProposedV2 = CreateAddressWasProposed(
                firstChildAddressPersistentLocalId, parentAddressPersistentLocalId);
            var secondChildAddressWasProposedV2 = CreateAddressWasProposed(
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
                        new AddressWasRejectedBecauseHouseNumberWasRejected(
                            Fixture.Create<StreetNamePersistentLocalId>(),
                            firstChildAddressPersistentLocalId)),
                    new Fact(_streamId,
                        new AddressWasRejectedBecauseHouseNumberWasRejected(
                            Fixture.Create<StreetNamePersistentLocalId>(),
                            secondChildAddressPersistentLocalId)),
                    new Fact(_streamId,
                        new AddressWasRejected(
                            Fixture.Create<StreetNamePersistentLocalId>(),
                            parentAddressPersistentLocalId))));
        }

        [Theory]
        [InlineData(AddressStatus.Current)]
        [InlineData(AddressStatus.Retired)]
        [InlineData(AddressStatus.Rejected)]
        public void WithNotProposedChildAddress_ThenChildAddressWasNotRejected(AddressStatus addressStatus)
        {
            var parentAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var childAddressPersistentLocalId = new AddressPersistentLocalId(2);

            var command = new RejectAddress(
                Fixture.Create<StreetNamePersistentLocalId>(),
                parentAddressPersistentLocalId,
                Fixture.Create<Provenance>());

            var parentAddressWasProposedV2 = CreateAddressWasProposed(parentAddressPersistentLocalId);
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
                        new AddressWasRejected(
                        Fixture.Create<StreetNamePersistentLocalId>(),
                        parentAddressPersistentLocalId))));
        }

        [Fact]
        public void WithRemovedProposedChildAddress_ThenChildAddressWasNotRejected()
        {
            var parentAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var childAddressPersistentLocalId = new AddressPersistentLocalId(2);

            var command = new RejectAddress(
                Fixture.Create<StreetNamePersistentLocalId>(),
                parentAddressPersistentLocalId,
                Fixture.Create<Provenance>());

            var parentAddressWasProposedV2 = CreateAddressWasProposed(parentAddressPersistentLocalId);
            var childAddressWasMigratedToStreetName = new AddressWasMigratedToStreetName(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressId>(),
                Fixture.Create<AddressStreetNameId>(),
                childAddressPersistentLocalId,
                AddressStatus.Proposed,
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
                        new AddressWasRejected(
                        Fixture.Create<StreetNamePersistentLocalId>(),
                        parentAddressPersistentLocalId))));
        }

        private AddressWasProposedV2 CreateAddressWasProposed(
            AddressPersistentLocalId addressPersistentLocalId,
            AddressPersistentLocalId? parentAddressPersistentLocalId = null)
        {
            var addressWasProposedV2 = new AddressWasProposedV2(
                Fixture.Create<StreetNamePersistentLocalId>(),
                addressPersistentLocalId,
                parentPersistentLocalId: parentAddressPersistentLocalId,
                Fixture.Create<PostalCode>(),
                Fixture.Create<HouseNumber>(),
                boxNumber: null);

            ((ISetProvenance)addressWasProposedV2).SetProvenance(Fixture.Create<Provenance>());

            return addressWasProposedV2;
        }

        [Fact]
        public void WithoutProposedAddress_ThenThrowsAddressNotFoundException()
        {
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
            var command = new RejectAddress(
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
                StreetNameStatus.Proposed);
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

            var rejectAddress = new RejectAddress(
                streetNamePersistentLocalId,
                addressPersistentLocalId,
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    streetNameWasImported,
                    migrateRemovedAddressToStreetName)
                .When(rejectAddress)
                .Throws(new AddressIsRemovedException(addressPersistentLocalId)));
        }

        [Theory]
        [InlineData(AddressStatus.Current)]
        [InlineData(AddressStatus.Retired)]
        public void AddressWithInvalidStatuses_ThenThrowsAddressCannotBeRejectedException(AddressStatus addressStatus)
        {
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();

            var streetNameWasImported = new StreetNameWasImported(
                streetNamePersistentLocalId,
                Fixture.Create<MunicipalityId>(),
                StreetNameStatus.Current);
            ((ISetProvenance)streetNameWasImported).SetProvenance(Fixture.Create<Provenance>());

            var addressWasMigratedToStreetName = new AddressWasMigratedToStreetName(
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
            ((ISetProvenance)addressWasMigratedToStreetName).SetProvenance(Fixture.Create<Provenance>());

            var rejectAddress = new RejectAddress(
                streetNamePersistentLocalId,
                addressPersistentLocalId,
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    streetNameWasImported,
                    addressWasMigratedToStreetName)
                .When(rejectAddress)
                .Throws(new AddressCannotBeRejectedException(addressStatus)));
        }

        [Fact]
        public void WithAlreadyRejectedAddress_ThenNone()
        {
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();

            var command = new RejectAddress(
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
            ((ISetProvenance)addressWasProposedV2).SetProvenance(Fixture.Create<Provenance>());

            var addressWasRejected = new AddressWasRejected(streetNamePersistentLocalId, addressPersistentLocalId);
            ((ISetProvenance)addressWasRejected).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasProposedV2,
                    addressWasRejected)
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
                AddressStatus.Proposed,
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
                AddressStatus.Proposed,
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
            sut.RejectAddress(parentAddressPersistentLocalId);

            // Assert
            var parentAddress = sut.StreetNameAddresses.First(x => x.AddressPersistentLocalId == parentAddressPersistentLocalId);
            var childAddress = sut.StreetNameAddresses.First(x => x.AddressPersistentLocalId == childAddressPersistentLocalId);

            parentAddress.Status.Should().Be(AddressStatus.Rejected);
            childAddress.Status.Should().Be(AddressStatus.Rejected);
        }
    }
}
