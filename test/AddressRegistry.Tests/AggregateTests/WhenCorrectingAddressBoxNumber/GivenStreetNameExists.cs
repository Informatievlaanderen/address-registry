namespace AddressRegistry.Tests.AggregateTests.WhenCorrectingAddressBoxNumber
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using EventExtensions;
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
            var houseNumberAddressWasProposed = Fixture.Create<AddressWasProposedV2>()
                .AsHouseNumberAddress();

            var boxNumberAddressWasProposed = Fixture.Create<AddressWasProposedV2>()
                .AsBoxNumberAddress(
                    new AddressPersistentLocalId(houseNumberAddressWasProposed.AddressPersistentLocalId),
                    new BoxNumber("A"))
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(houseNumberAddressWasProposed.AddressPersistentLocalId + 1))
                .WithHouseNumber(new HouseNumber(houseNumberAddressWasProposed.HouseNumber));

            var removedAddressWasMigratedToStreetName = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsBoxNumberAddress(
                    new AddressPersistentLocalId(houseNumberAddressWasProposed.AddressPersistentLocalId),
                    new BoxNumber(boxNumberAddressWasProposed.BoxNumber!))
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(boxNumberAddressWasProposed.AddressPersistentLocalId + 1))
                .WithHouseNumber(new HouseNumber(boxNumberAddressWasProposed.HouseNumber))
                .WithRemoved();

            var command = new CorrectAddressBoxNumber(
                Fixture.Create<StreetNamePersistentLocalId>(),
                new AddressPersistentLocalId(boxNumberAddressWasProposed.AddressPersistentLocalId),
                new BoxNumber("B"),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    houseNumberAddressWasProposed,
                    boxNumberAddressWasProposed,
                    removedAddressWasMigratedToStreetName)
                .When(command)
                .Then(new Fact(_streamId,
                    new AddressBoxNumberWasCorrectedV2(
                        Fixture.Create<StreetNamePersistentLocalId>(),
                        new AddressPersistentLocalId(boxNumberAddressWasProposed.AddressPersistentLocalId),
                        command.BoxNumber))));
        }

        [Theory]
        [InlineData("A", "A")]
        [InlineData("A", "a")]
        public void WithDuplicateBoxNumberForHouseNumber_ThenThrowsAddressAlreadyExistsException(
            string currentBoxNumber,
            string correctedBoxNumber)
        {
            var houseNumberAddressWasProposed = Fixture.Create<AddressWasProposedV2>()
                .AsHouseNumberAddress();

            var boxNumberAddressWasProposed = Fixture.Create<AddressWasProposedV2>()
                .AsBoxNumberAddress(
                    new AddressPersistentLocalId(houseNumberAddressWasProposed.AddressPersistentLocalId),
                    new BoxNumber("B"))
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(houseNumberAddressWasProposed.AddressPersistentLocalId + 1))
                .WithHouseNumber(new HouseNumber(houseNumberAddressWasProposed.HouseNumber));

            var secondBoxNumberAddressWasProposed = Fixture.Create<AddressWasProposedV2>()
                .AsBoxNumberAddress(
                    new AddressPersistentLocalId(houseNumberAddressWasProposed.AddressPersistentLocalId),
                    new BoxNumber(currentBoxNumber))
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(houseNumberAddressWasProposed.AddressPersistentLocalId + 2))
                .WithHouseNumber(new HouseNumber(houseNumberAddressWasProposed.HouseNumber));

            var command = new CorrectAddressBoxNumber(
                Fixture.Create<StreetNamePersistentLocalId>(),
                new AddressPersistentLocalId(boxNumberAddressWasProposed.AddressPersistentLocalId),
                new BoxNumber(correctedBoxNumber),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    houseNumberAddressWasProposed,
                    boxNumberAddressWasProposed,
                    secondBoxNumberAddressWasProposed)
                .When(command)
                .Throws(new AddressAlreadyExistsException(
                    new HouseNumber(houseNumberAddressWasProposed.HouseNumber),
                    new BoxNumber(correctedBoxNumber))));
        }

        [Fact]
        public void WithRemovedAddress_ThenThrowsAddressIsRemovedException()
        {
            var addressWasMigratedToStreetName = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress()
                .WithRemoved();

            var command = Fixture.Create<CorrectAddressBoxNumber>();

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
        public void WithStreetNameHasInvalidStatus_ThenThrowsStreetNameHasInvalidStatusException(StreetNameStatus streetNameStatus)
        {
            var migratedStreetNameWasImported = Fixture.Create<MigratedStreetNameWasImported>().WithStatus(streetNameStatus);

            var command = Fixture.Create<CorrectAddressBoxNumber>();

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
        public void WithAddressHasInvalidStatus_ThenThrowsAddressHasInvalidStatusException(AddressStatus addressStatus)
        {
            var houseNumberAddressWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress();

            var boxNumberAddressWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsBoxNumberAddress(
                    new AddressPersistentLocalId(houseNumberAddressWasMigrated.AddressPersistentLocalId),
                    new BoxNumber("A"))
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(houseNumberAddressWasMigrated.AddressPersistentLocalId + 1))
                .WithHouseNumber(new HouseNumber(houseNumberAddressWasMigrated.HouseNumber))
                .WithStatus(addressStatus);

            var command = new CorrectAddressBoxNumber(
                Fixture.Create<StreetNamePersistentLocalId>(),
                new AddressPersistentLocalId(boxNumberAddressWasMigrated.AddressPersistentLocalId),
                new BoxNumber("B"),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    houseNumberAddressWasMigrated,
                    boxNumberAddressWasMigrated)
                .When(command)
                .Throws(new AddressHasInvalidStatusException()));
        }

        [Fact]
        public void WithAddressWithoutBoxNumber_ThenThrowsAddressHasNoBoxNumberException()
        {
            var houseNumberAddressWasProposed = Fixture.Create<AddressWasProposedV2>()
                .AsHouseNumberAddress();

            var command = Fixture.Create<CorrectAddressBoxNumber>();

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    houseNumberAddressWasProposed)
                .When(command)
                .Throws(new AddressHasNoBoxNumberException()));
        }

        [Fact]
        public void WithNoChangedBoxNumber_ThenNone()
        {
            var houseNumberAddressWasProposedV2 = Fixture.Create<AddressWasProposedV2>()
                .AsHouseNumberAddress();

            var boxNumberAddressWasProposedV2 = Fixture.Create<AddressWasProposedV2>()
                .AsBoxNumberAddress(
                    new AddressPersistentLocalId(houseNumberAddressWasProposedV2.AddressPersistentLocalId),
                    new BoxNumber("A"))
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(houseNumberAddressWasProposedV2.AddressPersistentLocalId + 1))
                .WithHouseNumber(new HouseNumber(houseNumberAddressWasProposedV2.HouseNumber));

            var command = new CorrectAddressBoxNumber(
                Fixture.Create<StreetNamePersistentLocalId>(),
                new AddressPersistentLocalId(boxNumberAddressWasProposedV2.AddressPersistentLocalId),
                new BoxNumber(boxNumberAddressWasProposedV2.BoxNumber!),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    houseNumberAddressWasProposedV2,
                    boxNumberAddressWasProposedV2)
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void WithCaseSensitiveChange_ThenAddressBoxNumberWasCorrected()
        {
            var houseNumberAddressWasProposedV2 = Fixture.Create<AddressWasProposedV2>()
                .AsHouseNumberAddress();

            var boxNumberAddressWasProposedV2 = Fixture.Create<AddressWasProposedV2>()
                .AsBoxNumberAddress(
                    new AddressPersistentLocalId(houseNumberAddressWasProposedV2.AddressPersistentLocalId),
                    new BoxNumber("a"))
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(houseNumberAddressWasProposedV2.AddressPersistentLocalId + 1))
                .WithHouseNumber(new HouseNumber(houseNumberAddressWasProposedV2.HouseNumber));

            var command = new CorrectAddressBoxNumber(
                Fixture.Create<StreetNamePersistentLocalId>(),
                new AddressPersistentLocalId(boxNumberAddressWasProposedV2.AddressPersistentLocalId),
                new BoxNumber("A"),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    houseNumberAddressWasProposedV2,
                    boxNumberAddressWasProposedV2)
                .When(command)
                .Then(new Fact(_streamId,
                    new AddressBoxNumberWasCorrectedV2(
                        Fixture.Create<StreetNamePersistentLocalId>(),
                        new AddressPersistentLocalId(command.AddressPersistentLocalId),
                        command.BoxNumber))));
        }

        [Fact]
        public void StateCheck()
        {
            // Arrange
            var houseNumberAddressWasProposed = Fixture.Create<AddressWasProposedV2>()
                .AsHouseNumberAddress();

            var boxNumberAddressWasProposed = Fixture.Create<AddressWasProposedV2>()
                .AsBoxNumberAddress(
                    new AddressPersistentLocalId(houseNumberAddressWasProposed.AddressPersistentLocalId),
                    new BoxNumber("A"))
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(houseNumberAddressWasProposed.AddressPersistentLocalId + 1))
                .WithHouseNumber(new HouseNumber(houseNumberAddressWasProposed.HouseNumber));

            var streetName = new StreetNameFactory(NoSnapshotStrategy.Instance).Create();
            streetName.Initialize(new List<object>
            {
                Fixture.Create<StreetNameWasImported>(),
                houseNumberAddressWasProposed,
                boxNumberAddressWasProposed
            });

            var expectedBoxNumber = new BoxNumber("B");

            // Act
            streetName.CorrectAddressBoxNumber(
                new AddressPersistentLocalId(boxNumberAddressWasProposed.AddressPersistentLocalId),
                expectedBoxNumber);

            // Assert
            var childAddress = streetName.StreetNameAddresses.First(x =>
                x.AddressPersistentLocalId == new AddressPersistentLocalId(boxNumberAddressWasProposed.AddressPersistentLocalId));

            childAddress.BoxNumber.Should().Be(expectedBoxNumber);
        }
    }
}
