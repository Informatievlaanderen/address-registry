namespace AddressRegistry.Tests.AggregateTests.WhenRetiringAddress
{
    using System.Linq;
    using Api.BackOffice.Abstractions;
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
                boxNumber: null,
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Entry,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());

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
        public void WithProposedChildAddresses_ThenChildAddressesWereRejected()
        {
            var parentAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var firstChildAddressPersistentLocalId = new AddressPersistentLocalId(2);
            var secondChildAddressPersistentLocalId = new AddressPersistentLocalId(3);

            var command = new RetireAddress(
                Fixture.Create<StreetNamePersistentLocalId>(),
                parentAddressPersistentLocalId,
                Fixture.Create<Provenance>());

            var parentAddressWasMigrated = CreateAddressWasMigratedToStreetName(
                parentAddressPersistentLocalId, AddressStatus.Current);
            var firstChildAddressWasMigrated = CreateAddressWasMigratedToStreetName(
                firstChildAddressPersistentLocalId, AddressStatus.Proposed, parentAddressPersistentLocalId);
            var secondChildAddressWasMigrated = CreateAddressWasMigratedToStreetName(
                secondChildAddressPersistentLocalId, AddressStatus.Proposed, parentAddressPersistentLocalId);

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    parentAddressWasMigrated,
                    firstChildAddressWasMigrated,
                    secondChildAddressWasMigrated)
                .When(command)
                .Then(
                    new Fact(_streamId,
                        new AddressWasRetiredV2(
                        Fixture.Create<StreetNamePersistentLocalId>(),
                        parentAddressPersistentLocalId)),
                    new Fact(_streamId,
                        new AddressWasRejectedBecauseHouseNumberWasRetired(
                            Fixture.Create<StreetNamePersistentLocalId>(),
                            firstChildAddressPersistentLocalId)),
                    new Fact(_streamId,
                        new AddressWasRejectedBecauseHouseNumberWasRetired(
                            Fixture.Create<StreetNamePersistentLocalId>(),
                            secondChildAddressPersistentLocalId))));
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

            var parentAddressWasMigrated = CreateAddressWasMigratedToStreetName(
                parentAddressPersistentLocalId, AddressStatus.Current);
            var firstChildAddressWasMigrated = CreateAddressWasMigratedToStreetName(
                firstChildAddressPersistentLocalId, AddressStatus.Current, parentAddressPersistentLocalId);
            var secondChildAddressWasMigrated = CreateAddressWasMigratedToStreetName(
                secondChildAddressPersistentLocalId, AddressStatus.Current, parentAddressPersistentLocalId);

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    parentAddressWasMigrated,
                    firstChildAddressWasMigrated,
                    secondChildAddressWasMigrated)
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

        private AddressWasMigratedToStreetName CreateAddressWasMigratedToStreetName(
            AddressPersistentLocalId addressPersistentLocalId,
            AddressStatus addressStatus,
            AddressPersistentLocalId? parentAddressPersistentLocalId = null)
        {
            var addressWasMigrated = new AddressWasMigratedToStreetName(
                Fixture.Create<StreetNamePersistentLocalId>(),
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
                parentPersistentLocalId: parentAddressPersistentLocalId);

            ((ISetProvenance)addressWasMigrated).SetProvenance(Fixture.Create<Provenance>());

            return addressWasMigrated;
        }

        [Fact]
        public void WithRejectedChildAddress_ThenChildAddressWasNotRetired()
        {
            var parentAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var childAddressPersistentLocalId = new AddressPersistentLocalId(2);

            var command = new RetireAddress(
                Fixture.Create<StreetNamePersistentLocalId>(),
                parentAddressPersistentLocalId,
                Fixture.Create<Provenance>());

            var parentAddressWasMigratedToStreetName = CreateAddressWasMigratedToStreetName(
                parentAddressPersistentLocalId, AddressStatus.Current);
            var childAddressWasMigratedToStreetName = new AddressWasMigratedToStreetName(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressId>(),
                Fixture.Create<AddressStreetNameId>(),
                childAddressPersistentLocalId,
                AddressStatus.Rejected,
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
                    parentAddressWasMigratedToStreetName,
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

            var parentAddressWasMigratedToStreetName = CreateAddressWasMigratedToStreetName(
                parentAddressPersistentLocalId, AddressStatus.Current);
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
                    parentAddressWasMigratedToStreetName,
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
                .Throws(new AddressIsNotFoundException(addressPersistentLocalId)));
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
        public void AddressWithInvalidStatuses_ThenThrowsAddressHasInvalidStatusException(AddressStatus addressStatus)
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
                .Throws(new AddressHasInvalidStatusException()));
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
                boxNumber: null,
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Entry,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());

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
            var proposedChildAddressPersistentLocalId = new AddressPersistentLocalId(456);
            var currentChildAddressPersistentLocalId = new AddressPersistentLocalId(789);

            var parentAddressWasMigratedToStreetName = CreateAddressWasMigratedToStreetName(
                parentAddressPersistentLocalId, AddressStatus.Current);
            var proposedChildAddressWasMigratedToStreetName = CreateAddressWasMigratedToStreetName(
                proposedChildAddressPersistentLocalId, AddressStatus.Proposed, parentAddressPersistentLocalId);
            var currentChildAddressWasMigratedToStreetName = CreateAddressWasMigratedToStreetName(
                currentChildAddressPersistentLocalId, AddressStatus.Current, parentAddressPersistentLocalId);

            var sut = new StreetNameFactory(NoSnapshotStrategy.Instance).Create();
            sut.Initialize(new[]
            {
                parentAddressWasMigratedToStreetName,
                proposedChildAddressWasMigratedToStreetName,
                currentChildAddressWasMigratedToStreetName
            });

            // Act
            sut.RetireAddress(parentAddressPersistentLocalId);

            // Assert
            var parentAddress = sut.StreetNameAddresses
                .First(x => x.AddressPersistentLocalId == parentAddressPersistentLocalId);
            var previouslyProposedChildAddress = sut.StreetNameAddresses
                .First(x => x.AddressPersistentLocalId == proposedChildAddressPersistentLocalId);
            var previouslyCurrentChildAddress = sut.StreetNameAddresses
                .First(x => x.AddressPersistentLocalId == currentChildAddressPersistentLocalId);

            parentAddress.Status.Should().Be(AddressStatus.Retired);
            previouslyProposedChildAddress.Status.Should().Be(AddressStatus.Rejected);
            previouslyCurrentChildAddress.Status.Should().Be(AddressStatus.Retired);
        }
    }
}
