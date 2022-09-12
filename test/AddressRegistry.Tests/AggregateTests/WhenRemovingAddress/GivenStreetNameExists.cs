namespace AddressRegistry.Tests.AggregateTests.WhenRemovingAddress
{
    using System.Collections.Generic;
    using System.Linq;
    using Api.BackOffice.Abstractions;
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
        public void WithProposedAddress_ThenAddressWasRemoved()
        {
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
            var command = new RemoveAddress(
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

            ((ISetProvenance)addressWasProposedV2).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasProposedV2)
                .When(command)
                .Then(new Fact(_streamId,
                    new AddressWasRemovedV2(
                        Fixture.Create<StreetNamePersistentLocalId>(),
                        addressPersistentLocalId))));
        }

        [Fact]
        public void WithProposedChildAddresses_ThenChildAddressesWereAlsoRemoved()
        {
            var parentAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var firstChildAddressPersistentLocalId = new AddressPersistentLocalId(2);
            var secondChildAddressPersistentLocalId = new AddressPersistentLocalId(3);

            var command = new RemoveAddress(
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
                        new AddressWasRemovedV2(
                            Fixture.Create<StreetNamePersistentLocalId>(),
                            firstChildAddressPersistentLocalId)),
                    new Fact(_streamId,
                        new AddressWasRemovedV2(
                            Fixture.Create<StreetNamePersistentLocalId>(),
                            secondChildAddressPersistentLocalId)),
                    new Fact(_streamId,
                        new AddressWasRemovedV2(
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
                boxNumber: null,
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Entry,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());

            ((ISetProvenance)addressWasProposedV2).SetProvenance(Fixture.Create<Provenance>());

            return addressWasProposedV2;
        }

        [Fact]
        public void WithoutProposedAddress_ThenThrowsAddressNotFoundException()
        {
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
            var command = new RemoveAddress(
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
        public void OnRemovedAddress_ThenNothing()
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

            var rejectAddress = new RemoveAddress(
                streetNamePersistentLocalId,
                addressPersistentLocalId,
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    streetNameWasImported,
                    migrateRemovedAddressToStreetName)
                .When(rejectAddress)
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
            sut.RemoveAddress(parentAddressPersistentLocalId);

            // Assert
            var parentAddress = sut.StreetNameAddresses.First(x => x.AddressPersistentLocalId == parentAddressPersistentLocalId);
            var childAddress = sut.StreetNameAddresses.First(x => x.AddressPersistentLocalId == childAddressPersistentLocalId);

            parentAddress.IsRemoved.Should().BeTrue();
            childAddress.IsRemoved.Should().BeTrue();
        }
    }
}
