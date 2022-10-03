namespace AddressRegistry.Tests.AggregateTests.WhenCorrectingRetirementAddress
{
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
        public void WithRetiredAddress_ThenAddressWasCorrected()
        {
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
            var correctAddressRetirement = new CorrectAddressRetirement(
                Fixture.Create<StreetNamePersistentLocalId>(),
                addressPersistentLocalId,
                Fixture.Create<Provenance>());

            var addressWasMigrated = CreateAddressWasMigratedToStreetName(
                addressPersistentLocalId,
                addressStatus: AddressStatus.Retired);

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasMigrated)
                .When(correctAddressRetirement)
                .Then(new Fact(_streamId,
                    new AddressWasCorrectedFromRetiredToCurrent(
                        Fixture.Create<StreetNamePersistentLocalId>(),
                        addressPersistentLocalId))));
        }

        [Fact]
        public void WithoutProposedAddress_ThenThrowsAddressNotFoundException()
        {
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
            var command = new CorrectAddressRetirement(
                Fixture.Create<StreetNamePersistentLocalId>(),
                addressPersistentLocalId,
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId, Fixture.Create<StreetNameWasImported>())
                .When(command)
                .Throws(new AddressIsNotFoundException(addressPersistentLocalId)));
        }

        [Fact]
        public void OnRemovedAddress_ThenThrowsAddressIsRemovedException()
        {
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();

            var correctAddressRetirement = new CorrectAddressRetirement(
                Fixture.Create<StreetNamePersistentLocalId>(),
                addressPersistentLocalId,
                Fixture.Create<Provenance>());

            var addressWasMigrated = CreateAddressWasMigratedToStreetName(
                addressPersistentLocalId,
                addressStatus: AddressStatus.Retired,
                isRemoved: true);

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasMigrated)
                .When(correctAddressRetirement)
                .Throws(new AddressIsRemovedException(addressPersistentLocalId)));
        }

        [Theory]
        [InlineData(AddressStatus.Rejected)]
        [InlineData(AddressStatus.Proposed)]
        public void AddressWithInvalidStatuses_ThenThrowsAddressHasInvalidStatusException(AddressStatus addressStatus)
        {
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();

            var migrateAddressWithStatusCurrent = CreateAddressWasMigratedToStreetName(
                addressPersistentLocalId,
                addressStatus: addressStatus);

            var correctAddressRetirement = new CorrectAddressRetirement(
                Fixture.Create<StreetNamePersistentLocalId>(),
                addressPersistentLocalId,
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    migrateAddressWithStatusCurrent)
                .When(correctAddressRetirement)
                .Throws(new AddressHasInvalidStatusException()));
        }

        [Fact]
        public void WithAlreadyCurrentAddress_ThenNone()
        {
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();

            var command = new CorrectAddressRetirement(
                Fixture.Create<StreetNamePersistentLocalId>(),
                addressPersistentLocalId,
                Fixture.Create<Provenance>());

            var addressWasMigrated = CreateAddressWasMigratedToStreetName(
                addressPersistentLocalId,
                addressStatus: AddressStatus.Current);

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasMigrated)
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void StateCheck()
        {
            // Arrange
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();

            var addressWasMigrated = CreateAddressWasMigratedToStreetName(
                addressPersistentLocalId,
                addressStatus: AddressStatus.Retired);

            var sut = new StreetNameFactory(NoSnapshotStrategy.Instance).Create();
            sut.Initialize(new[] { addressWasMigrated });

            // Act
            sut.CorrectAddressRetirement(addressPersistentLocalId);

            // Assert
            var address = sut.StreetNameAddresses
                .First(x => x.AddressPersistentLocalId == addressPersistentLocalId);

            address.Status.Should().Be(AddressStatus.Current);
        }
    }
}
