namespace AddressRegistry.Tests.AggregateTests.WhenCorrectingAddressHouseNumber
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AddressRegistry.Api.BackOffice.Abstractions;
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
    using Xunit;
    using Xunit.Abstractions;
    using global::AutoFixture;

    public class GivenStreetNameExists : AddressRegistryTest
    {
        private readonly StreetNameStreamId _streamId;

        public GivenStreetNameExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedMunicipalityId());
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());
            Fixture.Customize(new WithFixedAddressPersistentLocalId());
            Fixture.Customize(new WithFixedValidHouseNumber());

            _streamId = Fixture.Create<StreetNameStreamId>();
        }

        [Fact]
        public void ThenAddressHouseNumberWasCorrected()
        {
            var expectedHouseNumber = new HouseNumber("101");

            var command = new CorrectAddressHouseNumber(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                expectedHouseNumber,
                Fixture.Create<Provenance>());

            var migrateRemovedAddressToTestFiltering = new AddressWasMigratedToStreetName(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressId>(),
                Fixture.Create<AddressStreetNameId>(),
                new AddressPersistentLocalId(456),
                AddressStatus.Current,
                new HouseNumber("404"),
                boxNumber: null,
                Fixture.Create<AddressGeometry>(),
                officiallyAssigned: true,
                postalCode: null,
                isCompleted: false,
                isRemoved: true,
                parentPersistentLocalId: null);
            ((ISetProvenance)migrateRemovedAddressToTestFiltering).SetProvenance(Fixture.Create<Provenance>());

            var addressWasProposedV2 = new AddressWasProposedV2(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                parentPersistentLocalId: null,
                Fixture.Create<PostalCode>(),
                new HouseNumber("404"),
                boxNumber: null,
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Lot,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());
            ((ISetProvenance)addressWasProposedV2).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasProposedV2,
                    migrateRemovedAddressToTestFiltering)
                .When(command)
                .Then(new Fact(_streamId,
                    new AddressHouseNumberWasCorrectedV2(
                        Fixture.Create<StreetNamePersistentLocalId>(),
                        Fixture.Create<AddressPersistentLocalId>(),
                        Array.Empty<AddressPersistentLocalId>(),
                        expectedHouseNumber))));
        }

        [Fact]
        public void WithBoxNumberAddresses_ThenBoxNumberAddressHouseNumbersWereAlsoChanged()
        {
            var parentAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var childAddressPersistentLocalId = new AddressPersistentLocalId(2);
            var parentAddressWasMigrated = CreateAddressWasMigratedToStreetName(parentAddressPersistentLocalId);
            var childAddressWasMigrated = CreateAddressWasMigratedToStreetName(
                childAddressPersistentLocalId,
                houseNumber: new HouseNumber(parentAddressWasMigrated.HouseNumber),
                parentAddressPersistentLocalId: parentAddressPersistentLocalId);

            var expectedHouseNumber = new HouseNumber("101");
            var command = new CorrectAddressHouseNumber(
                Fixture.Create<StreetNamePersistentLocalId>(),
                parentAddressPersistentLocalId,
                expectedHouseNumber,
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    parentAddressWasMigrated,
                    childAddressWasMigrated)
                .When(command)
                .Then(new Fact(
                    _streamId,
                    new AddressHouseNumberWasCorrectedV2(
                        Fixture.Create<StreetNamePersistentLocalId>(),
                        parentAddressPersistentLocalId,
                        new[] { childAddressPersistentLocalId },
                        expectedHouseNumber))));
        }

        [Fact]
        public void WithAlreadyExistingHouseNumber_ThenThrowsParentAddressAlreadyExistsException()
        {
            var houseNumberFirstAddress = new HouseNumber("1");
            var firstAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var firstAddressWasMigratedToStreetName = CreateAddressWasMigratedToStreetName(
                addressPersistentLocalId: firstAddressPersistentLocalId,
                houseNumber: houseNumberFirstAddress);

            var houseNumberSecondAddress = new HouseNumber("2");
            var secondAddressPersistentLocalId = new AddressPersistentLocalId(2);
            var secondAddressWasMigratedToStreetName = CreateAddressWasMigratedToStreetName(
                addressPersistentLocalId: secondAddressPersistentLocalId,
                houseNumber: houseNumberSecondAddress);

            var command = new CorrectAddressHouseNumber(
                Fixture.Create<StreetNamePersistentLocalId>(),
                firstAddressPersistentLocalId,
                houseNumberSecondAddress,
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    firstAddressWasMigratedToStreetName,
                    secondAddressWasMigratedToStreetName)
                .When(command)
                .Throws(new AddressAlreadyExistsException(houseNumberSecondAddress, null)));
        }

        [Fact]
        public void WithoutExistingAddress_ThenThrowsAddressNotFoundException()
        {
            var command = new CorrectAddressHouseNumber(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                new HouseNumber("101"),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>())
                .When(command)
                .Throws(new AddressIsNotFoundException(Fixture.Create<AddressPersistentLocalId>())));
        }

        [Fact]
        public void OnRemovedAddress_ThenThrowsAddressIsRemovedException()
        {
            var addressWasMigratedToStreetName = CreateAddressWasMigratedToStreetName(isRemoved: true);

            var command = new CorrectAddressHouseNumber(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                new HouseNumber("101"),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasMigratedToStreetName)
                .When(command)
                .Throws(new AddressIsRemovedException(Fixture.Create<AddressPersistentLocalId>())));
        }

        [Theory]
        [InlineData(StreetNameStatus.Rejected)]
        [InlineData(StreetNameStatus.Retired)]
        public void OnStreetNameInvalidStatus_ThenThrowsStreetNameHasInvalidStatusException(StreetNameStatus streetNameStatus)
        {
            var command = new CorrectAddressHouseNumber(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                Fixture.Create<HouseNumber>(),
                Fixture.Create<Provenance>());

            var migratedStreetNameWasImported = new MigratedStreetNameWasImported(
                Fixture.Create<StreetNameId>(),
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<MunicipalityId>(),
                Fixture.Create<NisCode>(),
                streetNameStatus);
            ((ISetProvenance)migratedStreetNameWasImported).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    migratedStreetNameWasImported,
                    CreateAddressWasMigratedToStreetName(
                        addressPersistentLocalId: Fixture.Create<AddressPersistentLocalId>()))
                .When(command)
                .Throws(new StreetNameHasInvalidStatusException()));
        }

        [Theory]
        [InlineData(AddressStatus.Rejected)]
        [InlineData(AddressStatus.Retired)]
        public void AddressWithInvalidStatuses_ThenThrowsAddressHasInvalidStatusException(AddressStatus addressStatus)
        {
            var addressWasMigratedToStreetName = CreateAddressWasMigratedToStreetName(addressStatus: addressStatus);
            ((ISetProvenance)addressWasMigratedToStreetName).SetProvenance(Fixture.Create<Provenance>());

            var command = new CorrectAddressHouseNumber(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                new HouseNumber("101"),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasMigratedToStreetName)
                .When(command)
                .Throws(new AddressHasInvalidStatusException()));
        }

        [Fact]
        public void OnChangeHouseNumberOfBoxNumberAddress_ThenThrowsAddresHasBoxNumberException()
        {
            var parentAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var childAddressPersistentLocalId = new AddressPersistentLocalId(2);
            var parentAddressWasMigrated = CreateAddressWasMigratedToStreetName(parentAddressPersistentLocalId, houseNumber:new HouseNumber("100"));
            var childAddressWasMigrated = CreateAddressWasMigratedToStreetName(
                childAddressPersistentLocalId,
                houseNumber: new HouseNumber(parentAddressWasMigrated.HouseNumber),
                parentAddressPersistentLocalId: parentAddressPersistentLocalId);

            var command = new CorrectAddressHouseNumber(
                Fixture.Create<StreetNamePersistentLocalId>(),
                childAddressPersistentLocalId,
                new HouseNumber("101"),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    parentAddressWasMigrated,
                    childAddressWasMigrated)
                .When(command)
                .Throws(new AddressHasBoxNumberException()));
        }

        [Fact]
        public void WithNoChangedHouseNumber_ThenNone()
        {
            var houseNumber = Fixture.Create<HouseNumber>();
            var addressWasMigratedToStreetName = CreateAddressWasMigratedToStreetName(houseNumber: houseNumber);

            var command = new CorrectAddressHouseNumber(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                houseNumber,
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasMigratedToStreetName)
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void StateCheck()
        {
            // Arrange
            var parentAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var childAddressPersistentLocalId = new AddressPersistentLocalId(2);
            var parentAddressWasMigrated = CreateAddressWasMigratedToStreetName(parentAddressPersistentLocalId);
            var childAddressWasMigrated = CreateAddressWasMigratedToStreetName(
                childAddressPersistentLocalId,
                houseNumber: new HouseNumber(parentAddressWasMigrated.HouseNumber),
                parentAddressPersistentLocalId: parentAddressPersistentLocalId);

            var sut = new StreetNameFactory(NoSnapshotStrategy.Instance).Create();
            sut.Initialize(new List<object>
            {
                Fixture.Create<StreetNameWasImported>(),
                parentAddressWasMigrated,
                childAddressWasMigrated
            });

            var expectedHouseNumber = new HouseNumber("101");

            // Act
            sut.CorrectAddressHouseNumber(
                parentAddressPersistentLocalId,
                expectedHouseNumber);

            // Assert
            var parentAddress =
                sut.StreetNameAddresses.First(x => x.AddressPersistentLocalId == parentAddressPersistentLocalId);
            var childAddress =
                sut.StreetNameAddresses.First(x => x.AddressPersistentLocalId == childAddressPersistentLocalId);

            parentAddress.HouseNumber.Should().Be(expectedHouseNumber);
            childAddress.HouseNumber.Should().Be(expectedHouseNumber);
        }
    }
}
