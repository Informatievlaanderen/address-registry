namespace AddressRegistry.Tests.AggregateTests.WhenChangingAddressPostalCode
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

            _streamId = Fixture.Create<StreetNameStreamId>();
        }

        [Fact]
        public void ThenAddressPostalCodeWasChanged()
        {
            var addressWasProposedV2 = Fixture.Create<AddressWasProposedV2>().AsHouseNumberAddress();

            var command = Fixture.Create<ChangeAddressPostalCode>();

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasProposedV2)
                .When(command)
                .Then(new Fact(_streamId,
                    new AddressPostalCodeWasChangedV2(
                        Fixture.Create<StreetNamePersistentLocalId>(),
                        Fixture.Create<AddressPersistentLocalId>(),
                        Array.Empty<AddressPersistentLocalId>(),
                        command.PostalCode))));
        }

        [Fact]
        public void WithBoxNumberAddresses_ThenBoxNumberAddressPostalCodesWereAlsoChanged()
        {
            var houseNumberAddressWasProposedV2 = Fixture.Create<AddressWasProposedV2>().AsHouseNumberAddress();

            var boxNumberAddressWasProposedV2 = Fixture.Create<AddressWasProposedV2>()
                .AsBoxNumberAddress(new AddressPersistentLocalId(houseNumberAddressWasProposedV2.AddressPersistentLocalId))
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(houseNumberAddressWasProposedV2.AddressPersistentLocalId + 1))
                .WithHouseNumber(new HouseNumber(houseNumberAddressWasProposedV2.HouseNumber));

            var command =  Fixture.Create<ChangeAddressPostalCode>();

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    houseNumberAddressWasProposedV2,
                    boxNumberAddressWasProposedV2)
                .When(command)
                .Then(new Fact(
                    _streamId,
                    new AddressPostalCodeWasChangedV2(
                        Fixture.Create<StreetNamePersistentLocalId>(),
                        new AddressPersistentLocalId(houseNumberAddressWasProposedV2.AddressPersistentLocalId),
                        new [] { new AddressPersistentLocalId(boxNumberAddressWasProposedV2.AddressPersistentLocalId) },
                        command.PostalCode))));
        }

        [Fact]
        public void WithoutExistingAddress_ThenThrowsAddressNotFoundException()
        {
            var command =  Fixture.Create<ChangeAddressPostalCode>();

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>())
                .When(command)
                .Throws(new AddressIsNotFoundException(Fixture.Create<AddressPersistentLocalId>())));
        }

        [Fact]
        public void OnRemovedAddress_ThenThrowsAddressIsRemovedException()
        {
            var migrateRemovedAddressToStreetName = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress(addressStatus: AddressStatus.Proposed)
                .WithRemoved();

            var command =  Fixture.Create<ChangeAddressPostalCode>();

            Assert(new Scenario()
                .Given(_streamId,
                    migrateRemovedAddressToStreetName)
                .When(command)
                .Throws(new AddressIsRemovedException(Fixture.Create<AddressPersistentLocalId>())));
        }

        [Fact]
        public void WithAddressHasBoxNumber_ThenThrowsAddresHasBoxNumberException()
        {
            var houseNumberAddressWasProposedV2 = Fixture.Create<AddressWasProposedV2>().AsHouseNumberAddress();

            var boxNumberAddressWasProposedV2 = Fixture.Create<AddressWasProposedV2>()
                .AsBoxNumberAddress(new AddressPersistentLocalId(houseNumberAddressWasProposedV2.AddressPersistentLocalId))
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(houseNumberAddressWasProposedV2.AddressPersistentLocalId + 1))
                .WithHouseNumber(new HouseNumber(houseNumberAddressWasProposedV2.HouseNumber));

            var command = new ChangeAddressPostalCode(
                Fixture.Create<StreetNamePersistentLocalId>(),
                new AddressPersistentLocalId(boxNumberAddressWasProposedV2.AddressPersistentLocalId),
                Fixture.Create<PostalCode>(),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    houseNumberAddressWasProposedV2,
                    boxNumberAddressWasProposedV2)
                .When(command)
                .Throws(new AddressHasBoxNumberException()));
        }

        [Theory]
        [InlineData(AddressStatus.Rejected)]
        [InlineData(AddressStatus.Retired)]
        public void AddressWithInvalidStatuses_ThenThrowsAddressHasInvalidStatusException(AddressStatus addressStatus)
        {
            var addressWasMigratedToStreetName = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress(addressStatus: addressStatus);

            var command =  Fixture.Create<ChangeAddressPostalCode>();

            Assert(new Scenario()
                .Given(_streamId,
                    addressWasMigratedToStreetName)
                .When(command)
                .Throws(new AddressHasInvalidStatusException()));
        }

        [Fact]
        public void WithStreetNameRetired_ThenThrowsStreetNameHasInvalidStatusException()
        {
            var streetNameWasRetired = Fixture.Create<StreetNameWasRetired>();

            var addressWasMigratedToStreetName = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress(addressStatus: AddressStatus.Current);

            var command =  Fixture.Create<ChangeAddressPostalCode>();

            Assert(new Scenario()
                .Given(_streamId,
                    addressWasMigratedToStreetName,
                    streetNameWasRetired)
                .When(command)
                .Throws(new StreetNameHasInvalidStatusException()));
        }

        [Fact]
        public void WithStreetNameRejected_ThenThrowsStreetNameHasInvalidStatusException()
        {
            var streetNameWasRejected = Fixture.Create<StreetNameWasRejected>();

            var addressWasMigratedToStreetName = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress(addressStatus: AddressStatus.Current);

            var command =  Fixture.Create<ChangeAddressPostalCode>();

            Assert(new Scenario()
                .Given(_streamId,
                    addressWasMigratedToStreetName,
                    streetNameWasRejected)
                .When(command)
                .Throws(new StreetNameHasInvalidStatusException()));
        }

        [Fact]
        public void WithNoChangedPostalCode_ThenNone()
        {
            var addressWasMigratedToStreetName = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress(addressStatus: AddressStatus.Current);

            var command = new ChangeAddressPostalCode(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                new PostalCode(addressWasMigratedToStreetName.PostalCode!),
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
            var houseNumberAddressWasProposedV2 = Fixture.Create<AddressWasProposedV2>().AsHouseNumberAddress();

            var boxNumberAddressWasProposedV2 = Fixture.Create<AddressWasProposedV2>()
                .AsBoxNumberAddress(new AddressPersistentLocalId(houseNumberAddressWasProposedV2.AddressPersistentLocalId))
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(houseNumberAddressWasProposedV2.AddressPersistentLocalId + 1))
                .WithHouseNumber(new HouseNumber(houseNumberAddressWasProposedV2.HouseNumber));

            var streetName = new StreetNameFactory(NoSnapshotStrategy.Instance).Create();
            streetName.Initialize(new List<object> { houseNumberAddressWasProposedV2, boxNumberAddressWasProposedV2 });

            var expectedPostalCode = Fixture.Create<PostalCode>();

            // Act
            streetName.ChangeAddressPostalCode(
                new AddressPersistentLocalId(houseNumberAddressWasProposedV2.AddressPersistentLocalId),
                expectedPostalCode);

            // Assert
            var houseNumberAddress = streetName.StreetNameAddresses
                .First(x => x.AddressPersistentLocalId == houseNumberAddressWasProposedV2.AddressPersistentLocalId);
            var boxNumberAddress = streetName.StreetNameAddresses
                .First(x => x.AddressPersistentLocalId == boxNumberAddressWasProposedV2.AddressPersistentLocalId);

            houseNumberAddress.PostalCode.Should().Be(expectedPostalCode);
            boxNumberAddress.PostalCode.Should().Be(expectedPostalCode);
        }
    }
}
