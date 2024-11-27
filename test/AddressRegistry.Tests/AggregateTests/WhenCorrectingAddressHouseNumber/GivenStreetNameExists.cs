namespace AddressRegistry.Tests.AggregateTests.WhenCorrectingAddressHouseNumber
{
    using System;
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
        public void ThenAddressHouseNumberWasCorrected()
        {
            var addressWasProposedV2 = Fixture.Create<AddressWasProposedV2>()
                .AsHouseNumberAddress(new HouseNumber("1"));

            var removedAddressWasMigratedToStreetName = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress(new HouseNumber(addressWasProposedV2.HouseNumber), AddressStatus.Current)
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(addressWasProposedV2.AddressPersistentLocalId + 1))
                .WithRemoved();

            var command = new CorrectAddressHouseNumber(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                new HouseNumber("2"),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasProposedV2,
                    removedAddressWasMigratedToStreetName)
                .When(command)
                .Then(new Fact(_streamId,
                    new AddressHouseNumberWasCorrectedV2(
                        Fixture.Create<StreetNamePersistentLocalId>(),
                        Fixture.Create<AddressPersistentLocalId>(),
                        Array.Empty<AddressPersistentLocalId>(),
                        command.HouseNumber))));
        }

        [Fact]
        public void WithBoxNumberAddresses_ThenBoxNumberAddressHouseNumbersWereAlsoChanged()
        {
            var houseNumberAddressWasProposedV2 = Fixture.Create<AddressWasProposedV2>()
                .AsHouseNumberAddress(new HouseNumber("99"));

            var boxNumberAddressWasProposedV2 = Fixture.Create<AddressWasProposedV2>()
                .AsBoxNumberAddress(new AddressPersistentLocalId(houseNumberAddressWasProposedV2.AddressPersistentLocalId))
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(houseNumberAddressWasProposedV2.AddressPersistentLocalId + 1));

            var secondBoxNumberAddressWasProposedV2 = Fixture.Create<AddressWasProposedV2>()
                .AsBoxNumberAddress(new AddressPersistentLocalId(houseNumberAddressWasProposedV2.AddressPersistentLocalId))
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(houseNumberAddressWasProposedV2.AddressPersistentLocalId + 2));

            var command = new CorrectAddressHouseNumber(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                new HouseNumber("100"),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    houseNumberAddressWasProposedV2,
                    boxNumberAddressWasProposedV2,
                    secondBoxNumberAddressWasProposedV2)
                .When(command)
                .Then(new Fact(
                    _streamId,
                    new AddressHouseNumberWasCorrectedV2(
                        Fixture.Create<StreetNamePersistentLocalId>(),
                        new AddressPersistentLocalId(houseNumberAddressWasProposedV2.AddressPersistentLocalId),
                        new[]
                        {
                            new AddressPersistentLocalId(boxNumberAddressWasProposedV2.AddressPersistentLocalId),
                            new AddressPersistentLocalId(secondBoxNumberAddressWasProposedV2.AddressPersistentLocalId),
                        },
                        command.HouseNumber))));
        }

        [Theory]
        [InlineData("1A", "1A")]
        [InlineData("1A", "1a")]
        public void WithAlreadyExistingHouseNumber_ThenThrowsAddressAlreadyExistsException(string existingHouseNumber, string correctedHouseNumber)
        {
            var firstAddressWasMigratedToStreetName = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress(new HouseNumber("1"), AddressStatus.Current);

            var secondAddressWasMigratedToStreetName = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress(new HouseNumber(existingHouseNumber), AddressStatus.Current)
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(firstAddressWasMigratedToStreetName.AddressPersistentLocalId + 1));

            var command = new CorrectAddressHouseNumber(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                new HouseNumber(correctedHouseNumber),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    firstAddressWasMigratedToStreetName,
                    secondAddressWasMigratedToStreetName)
                .When(command)
                .Throws(new AddressAlreadyExistsException(new HouseNumber(correctedHouseNumber), null!)));
        }

        [Fact]
        public void WithoutExistingAddress_ThenThrowsAddressNotFoundException()
        {
            var command = Fixture.Create<CorrectAddressHouseNumber>();

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>())
                .When(command)
                .Throws(new AddressIsNotFoundException(Fixture.Create<AddressPersistentLocalId>())));
        }

        [Fact]
        public void OnRemovedAddress_ThenThrowsAddressIsRemovedException()
        {
            var addressWasMigratedToStreetName = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress()
                .WithRemoved();

            var command =Fixture.Create<CorrectAddressHouseNumber>();

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
            var command =Fixture.Create<CorrectAddressHouseNumber>();

            var migratedStreetNameWasImported = Fixture.Create<MigratedStreetNameWasImported>().WithStatus(streetNameStatus);

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
            var addressWasMigratedToStreetName = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress(addressStatus: addressStatus);

            var command =Fixture.Create<CorrectAddressHouseNumber>();

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
            var houseNumberAddressWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress(new HouseNumber("100"), AddressStatus.Current);
            var boxNumberAddressWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsBoxNumberAddress(new AddressPersistentLocalId(houseNumberAddressWasMigrated.AddressPersistentLocalId))
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(houseNumberAddressWasMigrated.AddressPersistentLocalId + 1));

            var command = new CorrectAddressHouseNumber(
                Fixture.Create<StreetNamePersistentLocalId>(),
                new AddressPersistentLocalId(boxNumberAddressWasMigrated.AddressPersistentLocalId),
                new HouseNumber("101"),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    houseNumberAddressWasMigrated,
                    boxNumberAddressWasMigrated)
                .When(command)
                .Throws(new AddressHasBoxNumberException()));
        }

        [Fact]
        public void WithNoChangedHouseNumber_ThenNone()
        {
            var addressWasMigratedToStreetName = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress(addressStatus: AddressStatus.Current);

            var command = Fixture.Create<CorrectAddressHouseNumber>();

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasMigratedToStreetName)
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void WithCaseSensitiveChange_ThenAddressHouseNumberWasCorrected()
        {
            var addressWasMigratedToStreetName = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress(addressStatus: AddressStatus.Current, houseNumber: new HouseNumber("1a"));

            var command = new CorrectAddressHouseNumber(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                new HouseNumber("1A"),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasMigratedToStreetName)
                .When(command)
                .Then(new Fact(_streamId,
                    new AddressHouseNumberWasCorrectedV2(
                        Fixture.Create<StreetNamePersistentLocalId>(),
                        Fixture.Create<AddressPersistentLocalId>(),
                        Array.Empty<AddressPersistentLocalId>(),
                        command.HouseNumber))));
        }

        [Fact]
        public void StateCheck()
        {
            // Arrange
            var houseNumberAddressWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress(new HouseNumber("100"), AddressStatus.Current);
            var boxNumberAddressWasMigrated = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsBoxNumberAddress(new AddressPersistentLocalId(houseNumberAddressWasMigrated.AddressPersistentLocalId))
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(houseNumberAddressWasMigrated.AddressPersistentLocalId + 1));

            var sut = new StreetNameFactory(NoSnapshotStrategy.Instance).Create();
            sut.Initialize(new List<object>
            {
                Fixture.Create<StreetNameWasImported>(),
                houseNumberAddressWasMigrated,
                boxNumberAddressWasMigrated
            });

            var expectedHouseNumber = new HouseNumber("101");

            // Act
            sut.CorrectAddressHouseNumber(
                new AddressPersistentLocalId(houseNumberAddressWasMigrated.AddressPersistentLocalId),
                expectedHouseNumber);

            // Assert
            var houseNumberAddress =
                sut.StreetNameAddresses.First(x => x.AddressPersistentLocalId == houseNumberAddressWasMigrated.AddressPersistentLocalId);
            var boxNumberAddress =
                sut.StreetNameAddresses.First(x => x.AddressPersistentLocalId == boxNumberAddressWasMigrated.AddressPersistentLocalId);

            houseNumberAddress.HouseNumber.Should().Be(expectedHouseNumber);
            boxNumberAddress.HouseNumber.Should().Be(expectedHouseNumber);
        }
    }
}
