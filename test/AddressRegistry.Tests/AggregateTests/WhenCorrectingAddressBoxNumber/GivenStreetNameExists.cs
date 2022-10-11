namespace AddressRegistry.Tests.AggregateTests.WhenCorrectingAddressBoxNumber
{
    using System;
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
            Fixture.Customize(new WithFixedMunicipalityId());
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());
            Fixture.Customize(new WithFixedAddressPersistentLocalId());
            Fixture.Customize(new WithFixedValidHouseNumber());

            _streamId = Fixture.Create<StreetNameStreamId>();
        }

        [Fact]
        public void ThenAddressBoxNumberWasCorrected()
        {
            var expectedBoxNumber = new BoxNumber("1B");
            var addressToCorrectPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();

            var command = new CorrectAddressBoxNumber(
                Fixture.Create<StreetNamePersistentLocalId>(),
                addressToCorrectPersistentLocalId,
                expectedBoxNumber,
                Fixture.Create<Provenance>());

            var proposeAddressToTestFiltering = new AddressWasProposedV2(
                Fixture.Create<StreetNamePersistentLocalId>(),
                new AddressPersistentLocalId(456),
                parentPersistentLocalId: null,
                Fixture.Create<PostalCode>(),
                new HouseNumber("1"),
                new BoxNumber("1B"),
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Lot,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());
            ((ISetProvenance)proposeAddressToTestFiltering).SetProvenance(Fixture.Create<Provenance>());

            var proposeAddressToCorrect = new AddressWasProposedV2(
                Fixture.Create<StreetNamePersistentLocalId>(),
                addressToCorrectPersistentLocalId,
                parentPersistentLocalId: null,
                Fixture.Create<PostalCode>(),
                new HouseNumber("404"),
                new BoxNumber("1XYZ"),
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Lot,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());
            ((ISetProvenance)proposeAddressToCorrect).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    proposeAddressToTestFiltering,
                    proposeAddressToCorrect)
                .When(command)
                .Then(new Fact(_streamId,
                    new AddressBoxNumberWasCorrectedV2(
                        Fixture.Create<StreetNamePersistentLocalId>(),
                        Fixture.Create<AddressPersistentLocalId>(),
                        expectedBoxNumber))));
        }

        [Fact]
        public void WithDuplicateBoxNumberForHouseNumber_ThenThrowsAddressNotFoundException()
        {
            var houseNumber = new HouseNumber("404");
            var newBoxNumber = new BoxNumber("101");

            var command = new CorrectAddressBoxNumber(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                newBoxNumber,
                Fixture.Create<Provenance>());

            var proposeAddressToCorrect = new AddressWasProposedV2(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                parentPersistentLocalId: null,
                Fixture.Create<PostalCode>(),
                houseNumber,
                new BoxNumber("1XYZ"),
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Lot,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());
            ((ISetProvenance)proposeAddressToCorrect).SetProvenance(Fixture.Create<Provenance>());

            var addressToTestFiltering = new AddressWasProposedV2(
                Fixture.Create<StreetNamePersistentLocalId>(),
                new AddressPersistentLocalId(456), // DIFFERENT PERSISTENTID
                parentPersistentLocalId: null,
                Fixture.Create<PostalCode>(),
                houseNumber, // SAME HOUSENUMBER
                newBoxNumber, // SAME BOXNUMBER
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Lot,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());
            ((ISetProvenance)addressToTestFiltering).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    proposeAddressToCorrect,
                    addressToTestFiltering)
                .When(command)
                .Throws(new AddressAlreadyExistsException(houseNumber, newBoxNumber)));
        }

        [Fact]
        public void OnRemovedAddress_ThenThrowsAddressIsRemovedException()
        {
            var addressWasMigratedToStreetName = CreateAddressWasMigratedToStreetName(isRemoved: true);

            var command = new CorrectAddressBoxNumber(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                new BoxNumber("101"),
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
            var command = new CorrectAddressBoxNumber(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                Fixture.Create<BoxNumber>(),
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
            var parentAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var parentAddressWasMigrated = CreateAddressWasMigratedToStreetName(parentAddressPersistentLocalId);

            var childAddressWasMigrated = CreateAddressWasMigratedToStreetName(
                addressStatus: addressStatus,
                parentAddressPersistentLocalId: parentAddressPersistentLocalId);
            ((ISetProvenance)childAddressWasMigrated).SetProvenance(Fixture.Create<Provenance>());

            var command = new CorrectAddressBoxNumber(
                Fixture.Create<StreetNamePersistentLocalId>(),
                new AddressPersistentLocalId(childAddressWasMigrated.AddressPersistentLocalId),
                new BoxNumber("101"),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    parentAddressWasMigrated,
                    childAddressWasMigrated)
                .When(command)
                .Throws(new AddressHasInvalidStatusException()));
        }

        [Fact]
        public void AddressWithoutBoxNumber_ThenThrowsAddressHasNoBoxNumberException()
        {
            var parentAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var parentAddressWasMigrated = CreateAddressWasMigratedToStreetName(parentAddressPersistentLocalId);

            var command = new CorrectAddressBoxNumber(
                Fixture.Create<StreetNamePersistentLocalId>(),
                new AddressPersistentLocalId(parentAddressPersistentLocalId),
                new BoxNumber("101"),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    parentAddressWasMigrated)
                .When(command)
                .Throws(new AddressHasNoBoxNumberException()));
        }

        [Fact]
        public void WithNoChangedBoxNumber_ThenNone()
        {
            var boxNumber = Fixture.Create<BoxNumber>();
            var addressWasMigratedToStreetName = CreateAddressWasMigratedToStreetName(boxNumber: boxNumber);

            var command = new CorrectAddressBoxNumber(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                boxNumber,
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
                parentAddressPersistentLocalId: parentAddressPersistentLocalId,
                boxNumber: Fixture.Create<BoxNumber>());

            var sut = new StreetNameFactory(NoSnapshotStrategy.Instance).Create();
            sut.Initialize(new List<object>
            {
                Fixture.Create<StreetNameWasImported>(),
                parentAddressWasMigrated,
                childAddressWasMigrated
            });

            var expectedBoxNumber = new BoxNumber("101");

            // Act
            sut.CorrectAddressBoxNumber(
                childAddressPersistentLocalId,
                expectedBoxNumber);

            // Assert
            var childAddress = sut.StreetNameAddresses.First(x => x.AddressPersistentLocalId == childAddressPersistentLocalId);

            childAddress.BoxNumber.Should().Be(expectedBoxNumber);
        }
    }
}
