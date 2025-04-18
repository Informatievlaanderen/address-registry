namespace AddressRegistry.Tests.AggregateTests.WhenCorrectingAddressBoxNumbers
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
            Fixture.Customize(new WithFixedValidHouseNumber());

            _streamId = Fixture.Create<StreetNameStreamId>();
        }

        [Fact]
        public void ThenAddressBoxNumbersWereCorrected()
        {
            var houseNumberAddressWasProposed = Fixture.Create<AddressWasProposedV2>()
                .AsHouseNumberAddress();

            var boxNumberAddress1WasProposed = Fixture.Create<AddressWasProposedV2>()
                .AsBoxNumberAddress(
                    new AddressPersistentLocalId(houseNumberAddressWasProposed.AddressPersistentLocalId),
                    new BoxNumber("A"))
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(houseNumberAddressWasProposed.AddressPersistentLocalId + 1))
                .WithHouseNumber(new HouseNumber(houseNumberAddressWasProposed.HouseNumber));

            var boxNumberAddress2WasProposed = Fixture.Create<AddressWasProposedV2>()
                .AsBoxNumberAddress(
                    new AddressPersistentLocalId(houseNumberAddressWasProposed.AddressPersistentLocalId),
                    new BoxNumber("B"))
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(houseNumberAddressWasProposed.AddressPersistentLocalId + 2))
                .WithHouseNumber(new HouseNumber(houseNumberAddressWasProposed.HouseNumber));

            var removedAddressWasMigratedToStreetName = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsBoxNumberAddress(
                    new AddressPersistentLocalId(houseNumberAddressWasProposed.AddressPersistentLocalId),
                    new BoxNumber(boxNumberAddress1WasProposed.BoxNumber!))
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(boxNumberAddress1WasProposed.AddressPersistentLocalId + 3))
                .WithHouseNumber(new HouseNumber(boxNumberAddress1WasProposed.HouseNumber))
                .WithRemoved();

            var command = new CorrectAddressBoxNumbers(
                Fixture.Create<StreetNamePersistentLocalId>(),
                new Dictionary<AddressPersistentLocalId, BoxNumber>
                {
                    { new AddressPersistentLocalId(boxNumberAddress1WasProposed.AddressPersistentLocalId), new BoxNumber("B") },
                    { new AddressPersistentLocalId(boxNumberAddress2WasProposed.AddressPersistentLocalId), new BoxNumber("C") }
                },
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    houseNumberAddressWasProposed,
                    boxNumberAddress1WasProposed,
                    boxNumberAddress2WasProposed,
                    removedAddressWasMigratedToStreetName)
                .When(command)
                .Then(
                    new Fact(_streamId,
                        new AddressBoxNumbersWereCorrected(
                            Fixture.Create<StreetNamePersistentLocalId>(),
                            new Dictionary<AddressPersistentLocalId, BoxNumber>
                            {
                                {
                                    new AddressPersistentLocalId(boxNumberAddress1WasProposed.AddressPersistentLocalId),
                                    new BoxNumber("B")
                                },
                                {
                                    new AddressPersistentLocalId(boxNumberAddress2WasProposed.AddressPersistentLocalId),
                                    new BoxNumber("C")
                                }
                            }
                        )))
                );
        }

        [Theory]
        [InlineData(StreetNameStatus.Rejected)]
        [InlineData(StreetNameStatus.Retired)]
        public void WithStreetNameHasInvalidStatus_ThenThrowsStreetNameHasInvalidStatusException(StreetNameStatus streetNameStatus)
        {
            var migratedStreetNameWasImported = Fixture.Create<MigratedStreetNameWasImported>()
                .WithStatus(streetNameStatus);

            var command = Fixture.Create<CorrectAddressBoxNumbers>();

            Assert(new Scenario()
                .Given(_streamId,
                    migratedStreetNameWasImported,
                    CreateAddressWasMigratedToStreetName(
                        addressPersistentLocalId: Fixture.Create<AddressPersistentLocalId>()))
                .When(command)
                .Throws(new StreetNameHasInvalidStatusException()));
        }

        [Fact]
        public void WithNonExistingAddress_ThenThrowsAddressIsNotFoundException()
        {
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();

            var command = new CorrectAddressBoxNumbers(
                Fixture.Create<StreetNamePersistentLocalId>(),
                new Dictionary<AddressPersistentLocalId, BoxNumber>
                {
                    { addressPersistentLocalId, Fixture.Create<BoxNumber>() }
                },
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>()
                )
                .When(command)
                .Throws(new AddressIsNotFoundException(
                    addressPersistentLocalId
                )));
        }

        [Fact]
        public void WithRemovedAddress_ThenThrowsAddressIsRemovedException()
        {
            var houseNumberAddressWasProposed = Fixture.Create<AddressWasProposedV2>()
                .AsHouseNumberAddress();
            var houseNumberAddressWasRemoved = new AddressWasRemovedV2(
                new StreetNamePersistentLocalId(houseNumberAddressWasProposed.StreetNamePersistentLocalId),
                new AddressPersistentLocalId(houseNumberAddressWasProposed.AddressPersistentLocalId));
            ((ISetProvenance)houseNumberAddressWasRemoved).SetProvenance(Fixture.Create<Provenance>());

            var command = new CorrectAddressBoxNumbers(
                Fixture.Create<StreetNamePersistentLocalId>(),
                new Dictionary<AddressPersistentLocalId, BoxNumber>
                {
                    { new AddressPersistentLocalId(houseNumberAddressWasProposed.AddressPersistentLocalId), Fixture.Create<BoxNumber>() }
                },
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    houseNumberAddressWasProposed,
                    houseNumberAddressWasRemoved
                )
                .When(command)
                .Throws(new AddressIsRemovedException(
                    new AddressPersistentLocalId(houseNumberAddressWasProposed.AddressPersistentLocalId)
                )));
        }

        [Fact]
        public void WithAddressWithoutBoxNumber_ThenThrowsAddressHasNoBoxNumberException()
        {
            var houseNumberAddressWasProposed = Fixture.Create<AddressWasProposedV2>()
                .AsHouseNumberAddress();

            var command = new CorrectAddressBoxNumbers(
                Fixture.Create<StreetNamePersistentLocalId>(),
                new Dictionary<AddressPersistentLocalId, BoxNumber>
                {
                    { new AddressPersistentLocalId(houseNumberAddressWasProposed.AddressPersistentLocalId), Fixture.Create<BoxNumber>() }
                },
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    houseNumberAddressWasProposed
                )
                .When(command)
                .Throws(new AddressHasNoBoxNumberException(
                    new AddressPersistentLocalId(houseNumberAddressWasProposed.AddressPersistentLocalId)
                )));
        }

        [Fact]
        public void WithNotActiveAddress_ThenThrowsAddressHasInvalidStatusException()
        {
            var houseNumberAddressWasProposed = Fixture.Create<AddressWasProposedV2>()
                .AsHouseNumberAddress();
            var boxNumberAddressWasProposed = Fixture.Create<AddressWasProposedV2>()
                .AsBoxNumberAddress(new AddressPersistentLocalId(houseNumberAddressWasProposed.AddressPersistentLocalId));
            var boxNumberAddressWasRejected = Fixture.Create<AddressWasRejected>()
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(boxNumberAddressWasProposed.AddressPersistentLocalId));

            var command = new CorrectAddressBoxNumbers(
                Fixture.Create<StreetNamePersistentLocalId>(),
                new Dictionary<AddressPersistentLocalId, BoxNumber>
                {
                    { new AddressPersistentLocalId(boxNumberAddressWasProposed.AddressPersistentLocalId), Fixture.Create<BoxNumber>() }
                },
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    houseNumberAddressWasProposed,
                    boxNumberAddressWasProposed,
                    boxNumberAddressWasRejected
                )
                .When(command)
                .Throws(new AddressHasInvalidStatusException(
                    new AddressPersistentLocalId(boxNumberAddressWasProposed.AddressPersistentLocalId)
                )));
        }

        [Fact]
        public void WithNoChanges_ThenNone()
        {
            var houseNumberAddressWasProposed = Fixture.Create<AddressWasProposedV2>()
                .AsHouseNumberAddress();

            var boxNumberAddress1WasProposed = Fixture.Create<AddressWasProposedV2>()
                .AsBoxNumberAddress(
                    new AddressPersistentLocalId(houseNumberAddressWasProposed.AddressPersistentLocalId),
                    new BoxNumber("A"))
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(houseNumberAddressWasProposed.AddressPersistentLocalId + 1))
                .WithHouseNumber(new HouseNumber(houseNumberAddressWasProposed.HouseNumber));

            var command = new CorrectAddressBoxNumbers(
                Fixture.Create<StreetNamePersistentLocalId>(),
                new Dictionary<AddressPersistentLocalId, BoxNumber>
                {
                    { new AddressPersistentLocalId(boxNumberAddress1WasProposed.AddressPersistentLocalId), new BoxNumber("A") }
                },
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    houseNumberAddressWasProposed,
                    boxNumberAddress1WasProposed)
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void WithDuplicateBoxNumbers_ThenThrowsAddressAlreadyExistsException()
        {
            var houseNumberAddressWasProposed = Fixture.Create<AddressWasProposedV2>()
                .AsHouseNumberAddress();

            var boxNumberAddress1WasProposed = Fixture.Create<AddressWasProposedV2>()
                .AsBoxNumberAddress(
                    new AddressPersistentLocalId(houseNumberAddressWasProposed.AddressPersistentLocalId),
                    new BoxNumber("A"))
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(houseNumberAddressWasProposed.AddressPersistentLocalId + 1))
                .WithHouseNumber(new HouseNumber(houseNumberAddressWasProposed.HouseNumber));

            var boxNumberAddress2WasProposed = Fixture.Create<AddressWasProposedV2>()
                .AsBoxNumberAddress(
                    new AddressPersistentLocalId(houseNumberAddressWasProposed.AddressPersistentLocalId),
                    new BoxNumber("B"))
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(houseNumberAddressWasProposed.AddressPersistentLocalId + 2))
                .WithHouseNumber(new HouseNumber(houseNumberAddressWasProposed.HouseNumber));

            var command = new CorrectAddressBoxNumbers(
                Fixture.Create<StreetNamePersistentLocalId>(),
                new Dictionary<AddressPersistentLocalId, BoxNumber>
                {
                    { new AddressPersistentLocalId(boxNumberAddress1WasProposed.AddressPersistentLocalId), new BoxNumber("B") },
                    { new AddressPersistentLocalId(boxNumberAddress2WasProposed.AddressPersistentLocalId), new BoxNumber("B") }
                },
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    houseNumberAddressWasProposed,
                    boxNumberAddress1WasProposed,
                    boxNumberAddress2WasProposed
                )
                .When(command)
                .Throws(new AddressAlreadyExistsException(
                    new HouseNumber(houseNumberAddressWasProposed.HouseNumber),
                    new BoxNumber("B"))));
        }

        [Fact]
        public void WithDifferentHouseNumbers_ThenBoxNumberHouseNumberDoesNotMatchParentHouseNumberException()
        {
            var houseNumberAddress1WasProposed = Fixture.Create<AddressWasProposedV2>()
                .AsHouseNumberAddress()
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(1));

            var houseNumberAddress2WasProposed = Fixture.Create<AddressWasProposedV2>()
                .AsHouseNumberAddress()
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(2));

            var boxNumberAddress1WasProposed = Fixture.Create<AddressWasProposedV2>()
                .AsBoxNumberAddress(
                    new AddressPersistentLocalId(1),
                    new BoxNumber("A"))
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(11))
                .WithHouseNumber(new HouseNumber(houseNumberAddress1WasProposed.HouseNumber));

            var boxNumberAddress2WasProposed = Fixture.Create<AddressWasProposedV2>()
                .AsBoxNumberAddress(
                    new AddressPersistentLocalId(2),
                    new BoxNumber("A"))
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(21))
                .WithHouseNumber(new HouseNumber(houseNumberAddress2WasProposed.HouseNumber));

            var command = new CorrectAddressBoxNumbers(
                Fixture.Create<StreetNamePersistentLocalId>(),
                new Dictionary<AddressPersistentLocalId, BoxNumber>
                {
                    { new AddressPersistentLocalId(boxNumberAddress1WasProposed.AddressPersistentLocalId), new BoxNumber("B") },
                    { new AddressPersistentLocalId(boxNumberAddress2WasProposed.AddressPersistentLocalId), new BoxNumber("C") }
                },
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    houseNumberAddress1WasProposed,
                    houseNumberAddress2WasProposed,
                    boxNumberAddress1WasProposed,
                    boxNumberAddress2WasProposed
                )
                .When(command)
                .Throws(new BoxNumberHouseNumberDoesNotMatchParentHouseNumberException()));
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

            var boxNumbersWereCorrected = new AddressBoxNumbersWereCorrected(
                Fixture.Create<StreetNamePersistentLocalId>(),
                new Dictionary<AddressPersistentLocalId, BoxNumber>
                {
                    {
                        new AddressPersistentLocalId(boxNumberAddressWasProposed.AddressPersistentLocalId),
                        new BoxNumber("B")
                    }
                });

            var streetName = new StreetNameFactory(NoSnapshotStrategy.Instance).Create();
            streetName.Initialize(new List<object>
            {
                Fixture.Create<StreetNameWasImported>(),
                houseNumberAddressWasProposed,
                boxNumberAddressWasProposed,
                boxNumbersWereCorrected
            });

            // Assert
            var childAddress = streetName.StreetNameAddresses.First(x =>
                x.AddressPersistentLocalId == new AddressPersistentLocalId(boxNumberAddressWasProposed.AddressPersistentLocalId));

            childAddress.BoxNumber.Should().Be(new BoxNumber("B"));
        }
    }
}
