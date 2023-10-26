namespace AddressRegistry.Tests.AggregateTests.WhenApprovingAddress
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.StreetNameRegistry;
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
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());
            Fixture.Customize(new WithFixedAddressPersistentLocalId());
            _streamId = Fixture.Create<StreetNameStreamId>();
        }

        [Fact]
        public void WithProposedAddress_ThenAddressWasApproved()
        {
            var command = Fixture.Create<ApproveAddress>();

            var addressWasProposedV2 = Fixture.Create<AddressWasProposedV2>().AsHouseNumberAddress();

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasProposedV2)
                .When(command)
                .Then(new Fact(_streamId,
                    new AddressWasApproved(
                        Fixture.Create<StreetNamePersistentLocalId>(),
                        Fixture.Create<AddressPersistentLocalId>()))));
        }

        [Fact]
        public void WithoutProposedAddress_ThenThrowsAddressNotFoundException()
        {
            var command = Fixture.Create<ApproveAddress>();

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>())
                .When(command)
                .Throws(new AddressIsNotFoundException(Fixture.Create<AddressPersistentLocalId>())));
        }

        [Fact]
        public void OnRemovedAddress_ThenThrowsAddressIsRemovedException()
        {
            var streetNameWasImported = Fixture.Create<StreetNameWasImported>().WithStatus(StreetNameStatus.Current);

            var removedAddressWasMigratedToStreetName = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress(addressStatus: AddressStatus.Proposed)
                .WithRemoved();

            var command = Fixture.Create<ApproveAddress>();

            Assert(new Scenario()
                .Given(_streamId,
                    streetNameWasImported,
                    removedAddressWasMigratedToStreetName)
                .When(command)
                .Throws(new AddressIsRemovedException(Fixture.Create<AddressPersistentLocalId>())));
        }

        [Theory]
        [InlineData(StreetNameStatus.Rejected)]
        [InlineData(StreetNameStatus.Retired)]
        public void WithStreetNameHasInvalidStatus_ThenThrowsStreetNameHasInvalidStatusException(StreetNameStatus streetNameStatus)
        {
            var migratedStreetNameWasImported = Fixture.Create<MigratedStreetNameWasImported>().WithStatus(streetNameStatus);

            var command = Fixture.Create<ApproveAddress>();

            Assert(new Scenario()
                .Given(_streamId,
                    migratedStreetNameWasImported,
                    Fixture.Create<AddressWasProposedV2>().WithParentAddressPersistentLocalId(null))
                .When(command)
                .Throws(new StreetNameHasInvalidStatusException()));
        }

        [Theory]
        [InlineData(AddressStatus.Rejected)]
        [InlineData(AddressStatus.Retired)]
        public void AddressWithInvalidStatuses_ThenThrowsAddressHasInvalidStatusException(AddressStatus addressStatus)
        {
            var migratedStreetNameWasImported = Fixture.Create<MigratedStreetNameWasImported>().WithStatus(StreetNameStatus.Current);

            var addressWasMigratedToStreetName = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress(addressStatus: addressStatus);

            var command = Fixture.Create<ApproveAddress>();

            Assert(new Scenario()
                .Given(_streamId,
                    migratedStreetNameWasImported,
                    addressWasMigratedToStreetName)
                .When(command)
                .Throws(new AddressHasInvalidStatusException()));
        }

        [Fact]
        public void WithHouseNumberAddressNotInStatusCurrent_ThenThrowsParentAddressHasInvalidStatusException()
        {
            var streetNameWasImported = Fixture.Create<StreetNameWasImported>().WithStatus(StreetNameStatus.Current);

            var houseNumberAddressWasMigratedToStreetName = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress(addressStatus: AddressStatus.Proposed);

            var boxNumberAddressWasMigratedToStreetName = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsBoxNumberAddress(
                    new AddressPersistentLocalId(houseNumberAddressWasMigratedToStreetName.AddressPersistentLocalId))
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(houseNumberAddressWasMigratedToStreetName.AddressPersistentLocalId + 1))
                .WithStatus(AddressStatus.Proposed)
                .WithHouseNumber(new HouseNumber(houseNumberAddressWasMigratedToStreetName.HouseNumber));

            var approveBoxNumberAddress = new ApproveAddress(
                Fixture.Create<StreetNamePersistentLocalId>(),
                new AddressPersistentLocalId(boxNumberAddressWasMigratedToStreetName.AddressPersistentLocalId),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    streetNameWasImported,
                    houseNumberAddressWasMigratedToStreetName,
                    boxNumberAddressWasMigratedToStreetName)
                .When(approveBoxNumberAddress)
                .Throws(new ParentAddressHasInvalidStatusException()));
        }

        [Fact]
        public void WithAlreadyApprovedAddress_ThenNone()
        {
            var addressWasProposedV2 = Fixture.Create<AddressWasProposedV2>().AsHouseNumberAddress();

            var addressWasApproved = Fixture.Create<AddressWasApproved>();

            var command = Fixture.Create<ApproveAddress>();

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasProposedV2,
                    addressWasApproved)
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void StateCheck()
        {
            var addressWasApproved = Fixture.Create<AddressWasApproved>();

            var streetName = new StreetNameFactory(NoSnapshotStrategy.Instance).Create();
            streetName.Initialize(new List<object>
            {
                Fixture.Create<StreetNameWasProposedV2>(),
                Fixture.Create<AddressWasProposedV2>().WithParentAddressPersistentLocalId(null),
                addressWasApproved
            });

            var address = streetName.StreetNameAddresses
                .Single(x => x.AddressPersistentLocalId == Fixture.Create<AddressPersistentLocalId>());

            address.Status.Should().Be(AddressStatus.Current);
            address.LastEventHash.Should().Be(addressWasApproved.GetHash());
        }
    }
}
