namespace AddressRegistry.Tests.AggregateTests.WhenCorrectingAddressRetirement
{
    using System.Linq;
    using AddressRegistry.StreetName;
    using AddressRegistry.StreetName.Commands;
    using AddressRegistry.StreetName.Events;
    using AddressRegistry.StreetName.Exceptions;
    using AddressRegistry.Tests.AutoFixture;
    using AddressRegistry.Tests.EventExtensions;
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
            Fixture.Customize(new WithFixedValidHouseNumber());

            _streamId = Fixture.Create<StreetNameStreamId>();
        }

        [Fact]
        public void WithRetiredAddress_ThenAddressWasCorrected()
        {
            var addressWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress()
                .WithStatus(AddressStatus.Retired);

            var command = Fixture.Create<CorrectAddressRetirement>();

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasMigrated)
                .When(command)
                .Then(new Fact(_streamId, Fixture.Create<AddressWasCorrectedFromRetiredToCurrent>())));
        }

        [Fact]
        public void WithoutProposedAddress_ThenThrowsAddressNotFoundException()
        {
            var command = Fixture.Create<CorrectAddressRetirement>();

            Assert(new Scenario()
                .Given(_streamId, Fixture.Create<StreetNameWasImported>())
                .When(command)
                .Throws(new AddressIsNotFoundException(Fixture.Create<AddressPersistentLocalId>())));
        }

        [Fact]
        public void WithRemovedAddress_ThenThrowsAddressIsRemovedException()
        {
            var addressWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress()
                .WithStatus(AddressStatus.Retired)
                .WithRemoved();

            var command = Fixture.Create<CorrectAddressRetirement>();

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasMigrated)
                .When(command)
                .Throws(new AddressIsRemovedException(Fixture.Create<AddressPersistentLocalId>())));
        }

        [Fact]
        public void WithParentHouseNumberDiffersFromChildHouseNumberAddress_ThenThrowsBoxNumberHouseNumberDoesNotMatchParentHouseNumberException()
        {
            var houseNumberAddressWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress()
                .WithStatus(AddressStatus.Current)
                .WithHouseNumber(new HouseNumber("1"));

            var boxNumberAddressWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsBoxNumberAddress(new AddressPersistentLocalId(houseNumberAddressWasMigrated.AddressPersistentLocalId))
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(houseNumberAddressWasMigrated.AddressPersistentLocalId + 1))
                .WithStatus(AddressStatus.Retired)
                .WithHouseNumber(new HouseNumber("2"));

            var command = new CorrectAddressRetirement(
                Fixture.Create<StreetNamePersistentLocalId>(),
                new AddressPersistentLocalId(boxNumberAddressWasMigrated.AddressPersistentLocalId),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    houseNumberAddressWasMigrated,
                    boxNumberAddressWasMigrated)
                .When(command)
                .Throws(new BoxNumberHouseNumberDoesNotMatchParentHouseNumberException()));
        }

        [Fact]
        public void WhenParentPostalCodeDiffersFromChildHouseNumberAddress_ThenThrowsBoxNumberPostalCodeDoesNotMatchHouseNumberPostalCodeException()
        {
            var houseNumberAddressWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress()
                .WithStatus(AddressStatus.Current)
                .WithPostalCode(new PostalCode("1000"));

            var boxNumberAddressWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsBoxNumberAddress(new AddressPersistentLocalId(houseNumberAddressWasMigrated.AddressPersistentLocalId))
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(houseNumberAddressWasMigrated.AddressPersistentLocalId + 1))
                .WithStatus(AddressStatus.Retired)
                .WithPostalCode(new PostalCode("2000"));

            var command = new CorrectAddressRetirement(
                Fixture.Create<StreetNamePersistentLocalId>(),
                new AddressPersistentLocalId(boxNumberAddressWasMigrated.AddressPersistentLocalId),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    houseNumberAddressWasMigrated,
                    boxNumberAddressWasMigrated)
                .When(command)
                .Throws(new BoxNumberPostalCodeDoesNotMatchHouseNumberPostalCodeException()));
        }

        [Fact]
        public void WithRemovedParentAddress_ThenThrowsParentAddressIsRemovedException()
        {
            var houseNumberAddressWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress()
                .WithRemoved();

            var boxNumberAddressPersistentLocalId = new AddressPersistentLocalId(houseNumberAddressWasMigrated.AddressPersistentLocalId + 1);
            var boxNumberAddressWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsBoxNumberAddress(new AddressPersistentLocalId(houseNumberAddressWasMigrated.AddressPersistentLocalId))
                .WithAddressPersistentLocalId(boxNumberAddressPersistentLocalId)
                .WithStatus(AddressStatus.Retired);

            var command = new CorrectAddressRetirement(
                Fixture.Create<StreetNamePersistentLocalId>(),
                new AddressPersistentLocalId(boxNumberAddressWasMigrated.AddressPersistentLocalId),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    houseNumberAddressWasMigrated,
                    boxNumberAddressWasMigrated)
                .When(command)
                .Throws(new ParentAddressIsRemovedException(Fixture.Create<StreetNamePersistentLocalId>(), Fixture.Create<HouseNumber>())));
        }

        [Theory]
        [InlineData(StreetNameStatus.Rejected)]
        [InlineData(StreetNameStatus.Retired)]
        public void WithStreetNameInvalidStatus_ThenThrowsStreetNameHasInvalidStatusException(StreetNameStatus streetNameStatus)
        {
            var migratedStreetNameWasImported = Fixture.Create<MigratedStreetNameWasImported>().WithStatus(streetNameStatus);

            var addressWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress()
                .WithStatus(AddressStatus.Retired)
                .WithRemoved();

            var command = Fixture.Create<CorrectAddressRetirement>();

            Assert(new Scenario()
                .Given(_streamId,
                    migratedStreetNameWasImported,
                    addressWasMigrated)
                .When(command)
                .Throws(new StreetNameHasInvalidStatusException()));
        }

        [Theory]
        [InlineData(AddressStatus.Rejected)]
        [InlineData(AddressStatus.Proposed)]
        public void AddressWithInvalidStatuses_ThenThrowsAddressHasInvalidStatusException(AddressStatus addressStatus)
        {
            var addressWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress()
                .WithStatus(addressStatus);

            var command = Fixture.Create<CorrectAddressRetirement>();

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasMigrated)
                .When(command)
                .Throws(new AddressHasInvalidStatusException(command.AddressPersistentLocalId)));
        }

        [Fact]
        public void WithAlreadyCurrentAddress_ThenNone()
        {
            var addressWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress()
                .WithStatus(AddressStatus.Current);

            var command = Fixture.Create<CorrectAddressRetirement>();

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasMigrated)
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void WhenAddressAlreadyExists_ThrowAddressAlreadyExistsException()
        {
            var houseNumber = new HouseNumber("1");

            var firstAddressWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress(houseNumber)
                .WithStatus(AddressStatus.Retired);

            var secondAddressWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress(houseNumber)
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(firstAddressWasMigrated.AddressPersistentLocalId + 1))
                .WithStatus(AddressStatus.Current);

            var command = Fixture.Create<CorrectAddressRetirement>();

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    firstAddressWasMigrated,
                    secondAddressWasMigrated)
                .When(command)
                .Throws(new AddressAlreadyExistsException(houseNumber, null)));
        }

        [Fact]
        public void StateCheck()
        {
            // Arrange
            var addressWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress()
                .WithStatus(AddressStatus.Retired);

            var sut = new StreetNameFactory(NoSnapshotStrategy.Instance).Create();
            sut.Initialize(new[] { addressWasMigrated });

            // Act
            sut.CorrectAddressRetirement(Fixture.Create<AddressPersistentLocalId>());

            // Assert
            var address = sut.StreetNameAddresses
                .First(x => x.AddressPersistentLocalId == Fixture.Create<AddressPersistentLocalId>());

            address.Status.Should().Be(AddressStatus.Current);
        }

        [Theory]
        [InlineData(AddressStatus.Proposed)]
        [InlineData(AddressStatus.Rejected)]
        [InlineData(AddressStatus.Retired)]
        public void WhenParentAddressHasInvalidStatus_ThrowParentAddressHasInvalidStatusException(AddressStatus invalidStatus)
        {
            var houseNumberAddressWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress()
                .WithStatus(invalidStatus);

            var boxNumberAddressWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsBoxNumberAddress(new AddressPersistentLocalId(houseNumberAddressWasMigrated.AddressPersistentLocalId))
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(houseNumberAddressWasMigrated.AddressPersistentLocalId + 1))
                .WithStatus(AddressStatus.Retired);

            var command = new CorrectAddressRetirement(
                Fixture.Create<StreetNamePersistentLocalId>(),
                new AddressPersistentLocalId(boxNumberAddressWasMigrated.AddressPersistentLocalId),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    houseNumberAddressWasMigrated,
                    boxNumberAddressWasMigrated)
                .When(command)
                .Throws(new ParentAddressHasInvalidStatusException()));
        }
    }
}
