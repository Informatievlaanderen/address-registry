namespace AddressRegistry.Tests.AggregateTests.WhenCorrectingAddressApproval
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using EventExtensions;
    using FluentAssertions;
    using global::AutoFixture;
    using StreetName;
    using StreetName.Commands;
    using StreetName.Events;
    using StreetName.Exceptions;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenStreetName : AddressRegistryTest
    {
        private readonly StreetNameStreamId _streamId;

        public GivenStreetName(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());
            Fixture.Customize(new WithFixedAddressPersistentLocalId());
            Fixture.Customize(new WithFixedMunicipalityId());
            Fixture.Customize(new WithFixedValidHouseNumber());

            _streamId = Fixture.Create<StreetNameStreamId>();
        }

        [Fact]
        public void ThenAddressWasCorrectedToProposed()
        {
            var command = Fixture.Create<CorrectAddressApproval>();

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MigratedStreetNameWasImported>(),
                    CreateAddressWasMigratedToStreetName(
                        addressPersistentLocalId: Fixture.Create<AddressPersistentLocalId>(),
                        addressStatus: AddressStatus.Current))
                .When(command)
                .Then(
                    new Fact(_streamId,
                        new AddressWasCorrectedFromApprovedToProposed(
                            command.StreetNamePersistentLocalId,
                            command.AddressPersistentLocalId))));
        }

        [Fact]
        public void WithApprovedAndOfficiallyAssignedBoxNumberAddresses_ThenBoxNumberAddressesWereAlsoCorrected()
        {
            var houseNumberAddressWasMigratedToStreetName = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress(addressStatus: AddressStatus.Current)
                .WithOfficiallyAssigned(true);

            var boxNumberAddressWasMigratedToStreetName = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsBoxNumberAddress(new AddressPersistentLocalId(houseNumberAddressWasMigratedToStreetName.AddressPersistentLocalId))
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(houseNumberAddressWasMigratedToStreetName.AddressPersistentLocalId + 1))
                .WithOfficiallyAssigned(true)
                .WithStatus(AddressStatus.Current);

            var notOfficiallyAssignedBoxNumberAddress =  Fixture.Create<AddressWasMigratedToStreetName>()
                .AsBoxNumberAddress(new AddressPersistentLocalId(houseNumberAddressWasMigratedToStreetName.AddressPersistentLocalId))
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(houseNumberAddressWasMigratedToStreetName.AddressPersistentLocalId + 2))
                .WithOfficiallyAssigned(false)
                .WithStatus(AddressStatus.Current);

            var command = Fixture.Create<CorrectAddressApproval>();

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MigratedStreetNameWasImported>(),
                    houseNumberAddressWasMigratedToStreetName,
                    boxNumberAddressWasMigratedToStreetName,
                    notOfficiallyAssignedBoxNumberAddress)
                .When(command)
                .Then(
                    new Fact(_streamId,
                        new AddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected(
                            command.StreetNamePersistentLocalId,
                            new AddressPersistentLocalId(boxNumberAddressWasMigratedToStreetName.AddressPersistentLocalId))),
                    new Fact(_streamId,
                        new AddressWasCorrectedFromApprovedToProposed(
                            command.StreetNamePersistentLocalId,
                            new AddressPersistentLocalId(houseNumberAddressWasMigratedToStreetName.AddressPersistentLocalId)))));
        }

        [Fact]
        public void WhenAlreadyProposed_ThenNothing()
        {
            var command = Fixture.Create<CorrectAddressApproval>();

            var addressWasMigratedToStreetName = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress(addressStatus: AddressStatus.Proposed)
                .WithOfficiallyAssigned(true);

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MigratedStreetNameWasImported>(),
                    addressWasMigratedToStreetName)
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void WithoutExistingAddress_ThenThrowsAddressNotFoundException()
        {
            var command = Fixture.Create<CorrectAddressApproval>();

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MigratedStreetNameWasImported>())
                .When(command)
                .Throws(new AddressIsNotFoundException(Fixture.Create<AddressPersistentLocalId>())));
        }

        [Fact]
        public void OnRemovedAddress_ThenThrowsAddressIsRemovedException()
        {
            var command = Fixture.Create<CorrectAddressApproval>();

            var addressWasMigratedToStreetName = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress(addressStatus: AddressStatus.Proposed)
                .WithRemoved();

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MigratedStreetNameWasImported>(),
                    addressWasMigratedToStreetName)
                .When(command)
                .Throws(new AddressIsRemovedException(Fixture.Create<AddressPersistentLocalId>())));
        }

        [Theory]
        [InlineData(StreetNameStatus.Rejected)]
        [InlineData(StreetNameStatus.Retired)]
        public void OnStreetNameInvalidStatus_ThenThrowsStreetNameHasInvalidStatusException(StreetNameStatus streetNameStatus)
        {
            var command = Fixture.Create<CorrectAddressApproval>();

            var migratedStreetNameWasImported = Fixture.Create<MigratedStreetNameWasImported>().WithStatus(streetNameStatus);

            var addressWasMigratedToStreetName = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress(addressStatus: AddressStatus.Current);

            Assert(new Scenario()
                .Given(_streamId,
                    migratedStreetNameWasImported,
                    addressWasMigratedToStreetName)
                .When(command)
                .Throws(new StreetNameHasInvalidStatusException()));
        }

        [Theory]
        [InlineData(AddressStatus.Rejected)]
        [InlineData(AddressStatus.Retired)]
        public void AddressWithInvalidStatuses_ThenThrowsAddressHasInvalidStatusException(AddressStatus addressStatus)
        {
            var command = Fixture.Create<CorrectAddressApproval>();

            var addressWasMigratedToStreetName = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress(addressStatus: addressStatus)
                .WithOfficiallyAssigned(true);

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MigratedStreetNameWasImported>(),
                    addressWasMigratedToStreetName)
                .When(command)
                .Throws(new AddressHasInvalidStatusException(command.AddressPersistentLocalId)));
        }

        [Fact]
        public void WithNotOfficiallyAssignedAddress_ThenThrowsAddressIsNotOfficiallyAssignedException()
        {
            var command = Fixture.Create<CorrectAddressApproval>();

            var addressWasMigratedToStreetName = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress(addressStatus: AddressStatus.Current)
                .WithOfficiallyAssigned(false);

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MigratedStreetNameWasImported>(),
                    addressWasMigratedToStreetName)
                .When(command)
                .Throws(new AddressIsNotOfficiallyAssignedException()));
        }

        [Fact]
        public void StateCheck()
        {
            // Arrange
            var houseNumberAddressWasMigratedToStreetName = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress(addressStatus: AddressStatus.Current)
                .WithOfficiallyAssigned(true);

            var boxNumberAddressWasMigratedToStreetName = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsBoxNumberAddress(new AddressPersistentLocalId(houseNumberAddressWasMigratedToStreetName.AddressPersistentLocalId))
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(houseNumberAddressWasMigratedToStreetName.AddressPersistentLocalId + 1))
                .WithOfficiallyAssigned(true)
                .WithStatus(AddressStatus.Current);

            var sut = new StreetNameFactory(NoSnapshotStrategy.Instance).Create();
            sut.Initialize(new List<object>
            {
                Fixture.Create<StreetNameWasImported>(),
                houseNumberAddressWasMigratedToStreetName,
                boxNumberAddressWasMigratedToStreetName
            });

            // Act
            sut.CorrectAddressApproval(new AddressPersistentLocalId(houseNumberAddressWasMigratedToStreetName.AddressPersistentLocalId));

            // Assert
            var parentAddress =
                sut.StreetNameAddresses.First(x => x.AddressPersistentLocalId == houseNumberAddressWasMigratedToStreetName.AddressPersistentLocalId);
            var childAddress =
                sut.StreetNameAddresses.First(x => x.AddressPersistentLocalId == boxNumberAddressWasMigratedToStreetName.AddressPersistentLocalId);

            parentAddress.Status.Should().Be(AddressStatus.Proposed);
            childAddress.Status.Should().Be(AddressStatus.Proposed);
        }
    }
}
