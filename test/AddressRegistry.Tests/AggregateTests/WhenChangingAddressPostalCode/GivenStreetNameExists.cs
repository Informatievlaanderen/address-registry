namespace AddressRegistry.Tests.AggregateTests.WhenChangingAddressPostalCode
{
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
    using global::AutoFixture;
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

            _streamId = Fixture.Create<StreetNameStreamId>();
        }

        [Fact]
        public void ThenAddressPostalCodeWasChanged()
        {
            var expectedPostalCode = Fixture.Create<PostalCode>();
            var command = new ChangeAddressPostalCode(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                expectedPostalCode,
                Fixture.Create<Provenance>());

            var addressWasProposedV2 = new AddressWasProposedV2(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                parentPersistentLocalId: null,
                Fixture.Create<PostalCode>(),
                Fixture.Create<HouseNumber>(),
                boxNumber: null,
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Lot,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());
            ((ISetProvenance)addressWasProposedV2).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasProposedV2)
                .When(command)
                .Then(new Fact(_streamId,
                    new AddressPostalCodeWasChangedV2(
                        Fixture.Create<StreetNamePersistentLocalId>(),
                        Fixture.Create<AddressPersistentLocalId>(),
                        expectedPostalCode))));
        }

        [Fact]
        public void WithBoxNumberAddresses_ThenBoxNumberAddressPostalCodesWereAlsoChanged()
        {
            var parentAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var childAddressPersistentLocalId = new AddressPersistentLocalId(2);
            var parentAddressWasMigrated = CreateAddressWasMigratedToStreetName(parentAddressPersistentLocalId);
            var childAddressWasMigrated = CreateAddressWasMigratedToStreetName(
                childAddressPersistentLocalId,
                houseNumber: new HouseNumber(parentAddressWasMigrated.HouseNumber),
                parentAddressPersistentLocalId: parentAddressPersistentLocalId);

            var expectedPostalCode = Fixture.Create<PostalCode>();
            var command = new ChangeAddressPostalCode(
                Fixture.Create<StreetNamePersistentLocalId>(),
                parentAddressPersistentLocalId,
                expectedPostalCode,
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    parentAddressWasMigrated,
                    childAddressWasMigrated)
                .When(command)
                .Then(new Fact(_streamId,
                    new AddressPostalCodeWasChangedV2(
                        Fixture.Create<StreetNamePersistentLocalId>(),
                        childAddressPersistentLocalId,
                        expectedPostalCode)),
                    new Fact(_streamId,
                        new AddressPostalCodeWasChangedV2(
                            Fixture.Create<StreetNamePersistentLocalId>(),
                            parentAddressPersistentLocalId,
                            expectedPostalCode))));
        }

        [Fact]
        public void WithoutExistingAddress_ThenThrowsAddressNotFoundException()
        {
            var command = new ChangeAddressPostalCode(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                Fixture.Create<PostalCode>(),
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
            var migrateRemovedAddressToStreetName = CreateAddressWasMigratedToStreetName(isRemoved: true);

           var command = new ChangeAddressPostalCode(
               Fixture.Create<StreetNamePersistentLocalId>(),
               Fixture.Create<AddressPersistentLocalId>(),
               Fixture.Create<PostalCode>(),
               Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    migrateRemovedAddressToStreetName)
                .When(command)
                .Throws(new AddressIsRemovedException(Fixture.Create<AddressPersistentLocalId>())));
        }

        [Theory]
        [InlineData(AddressStatus.Rejected)]
        [InlineData(AddressStatus.Retired)]
        public void AddressWithInvalidStatuses_ThenThrowsAddressHasInvalidStatusException(AddressStatus addressStatus)
        {
            var addressWasMigratedToStreetName = CreateAddressWasMigratedToStreetName(addressStatus: addressStatus);
            ((ISetProvenance)addressWasMigratedToStreetName).SetProvenance(Fixture.Create<Provenance>());

            var command = new ChangeAddressPostalCode(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                Fixture.Create<PostalCode>(),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    addressWasMigratedToStreetName)
                .When(command)
                .Throws(new AddressHasInvalidStatusException()));
        }

        [Fact]
        public void WithNoChangedPostalCode_ThenNone()
        {
            var postalCode = Fixture.Create<PostalCode>();
            var addressWasMigratedToStreetName = CreateAddressWasMigratedToStreetName(postalCode: postalCode);

            var command = new ChangeAddressPostalCode(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                postalCode,
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
            sut.Initialize(new List<object> { parentAddressWasMigrated, childAddressWasMigrated });

            var expectedPostalCode = Fixture.Create<PostalCode>();

            // Act
            sut.ChangeAddressPostalCode(
                parentAddressPersistentLocalId,
                expectedPostalCode);

            // Assert
            var parentAddress = sut.StreetNameAddresses.First(x => x.AddressPersistentLocalId == parentAddressPersistentLocalId);
            var childAddress = sut.StreetNameAddresses.First(x => x.AddressPersistentLocalId == childAddressPersistentLocalId);

            parentAddress.PostalCode.Should().Be(expectedPostalCode);
            childAddress.PostalCode.Should().Be(expectedPostalCode);
        }
    }
}
