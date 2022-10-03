namespace AddressRegistry.Tests.AggregateTests.WhenCorrectingAddressApproval
{
    using System.Collections.Generic;
    using System.Linq;
    using StreetName;
    using StreetName.Commands;
    using StreetName.Events;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using FluentAssertions;
    using global::AutoFixture;
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
            var correctAddressApproval = new CorrectAddressApproval(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MigratedStreetNameWasImported>(),
                    CreateAddressWasMigratedToStreetName(
                        addressPersistentLocalId: Fixture.Create<AddressPersistentLocalId>(),
                        addressStatus: AddressStatus.Current))
                .When(correctAddressApproval)
                .Then(
                    new Fact(_streamId,
                        new AddressWasCorrectedFromApprovedToProposed(
                            correctAddressApproval.StreetNamePersistentLocalId,
                            correctAddressApproval.AddressPersistentLocalId))));
        }

        [Fact]
        public void WithApprovedAndOfficiallyAssignedBoxNumberAddresses_ThenBoxNumberAddressesWereAlsoCorrected()
        {
            var parentAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var childAddressPersistentLocalId = new AddressPersistentLocalId(2);
            var notOfficiallyAssignedChildAddressPersistentLocalId = new AddressPersistentLocalId(3);

            var correctAddressApproval = new CorrectAddressApproval(
                Fixture.Create<StreetNamePersistentLocalId>(),
                parentAddressPersistentLocalId,
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MigratedStreetNameWasImported>(),
                    CreateAddressWasMigratedToStreetName(
                        parentAddressPersistentLocalId,
                        addressStatus: AddressStatus.Current),
                    CreateAddressWasMigratedToStreetName(
                        childAddressPersistentLocalId,
                        parentAddressPersistentLocalId: parentAddressPersistentLocalId,
                        addressStatus: AddressStatus.Current,
                        isOfficiallyAssigned: true),
                    CreateAddressWasMigratedToStreetName(
                        notOfficiallyAssignedChildAddressPersistentLocalId,
                        parentAddressPersistentLocalId: parentAddressPersistentLocalId,
                        addressStatus: AddressStatus.Current,
                        isOfficiallyAssigned: false))
                .When(correctAddressApproval)
                .Then(
                    new Fact(_streamId,
                        new AddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected(
                            correctAddressApproval.StreetNamePersistentLocalId,
                            childAddressPersistentLocalId)),
                    new Fact(_streamId,
                        new AddressWasCorrectedFromApprovedToProposed(
                            correctAddressApproval.StreetNamePersistentLocalId,
                            parentAddressPersistentLocalId))));
        }

        [Fact]
        public void WhenAlreadyProposed_ThenNothing()
        {
            var correctAddressApproval = new CorrectAddressApproval(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MigratedStreetNameWasImported>(),
                    CreateAddressWasMigratedToStreetName(
                        addressPersistentLocalId: Fixture.Create<AddressPersistentLocalId>(),
                        addressStatus: AddressStatus.Proposed))
                .When(correctAddressApproval)
                .ThenNone());
        }

        [Fact]
        public void WithoutExistingAddress_ThenThrowsAddressNotFoundException()
        {
            var correctAddressApproval = new CorrectAddressApproval(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MigratedStreetNameWasImported>())
                .When(correctAddressApproval)
                .Throws(new AddressIsNotFoundException(Fixture.Create<AddressPersistentLocalId>())));
        }

        [Fact]
        public void OnRemovedAddress_ThenThrowsAddressIsRemovedException()
        {
            var correctAddressApproval = new CorrectAddressApproval(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MigratedStreetNameWasImported>(),
                    CreateAddressWasMigratedToStreetName(
                        addressPersistentLocalId: Fixture.Create<AddressPersistentLocalId>(),
                        isRemoved: true))
                .When(correctAddressApproval)
                .Throws(new AddressIsRemovedException(Fixture.Create<AddressPersistentLocalId>())));
        }

        [Theory]
        [InlineData(AddressStatus.Rejected)]
        [InlineData(AddressStatus.Retired)]
        public void AddressWithInvalidStatuses_ThenThrowsAddressHasInvalidStatusException(AddressStatus addressStatus)
        {
            var correctAddressApproval = new CorrectAddressApproval(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MigratedStreetNameWasImported>(),
                    CreateAddressWasMigratedToStreetName(
                        addressPersistentLocalId: Fixture.Create<AddressPersistentLocalId>(),
                        addressStatus: addressStatus))
                .When(correctAddressApproval)
                .Throws(new AddressHasInvalidStatusException()));
        }

        [Fact]
        public void WithNotOfficiallyAssignedAddress_ThenThrowsAddressIsNotOfficiallyAssignedException()
        {
            var correctAddressApproval = new CorrectAddressApproval(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MigratedStreetNameWasImported>(),
                    CreateAddressWasMigratedToStreetName(
                        addressPersistentLocalId: Fixture.Create<AddressPersistentLocalId>(),
                        addressStatus: AddressStatus.Current,
                        isOfficiallyAssigned: false))
                .When(correctAddressApproval)
                .Throws(new AddressIsNotOfficiallyAssignedException()));
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
                parentAddressPersistentLocalId: parentAddressPersistentLocalId,
                houseNumber: new HouseNumber(parentAddressWasMigrated.HouseNumber));

            var sut = new StreetNameFactory(NoSnapshotStrategy.Instance).Create();
            sut.Initialize(new List<object>
            {
                Fixture.Create<StreetNameWasImported>(),
                parentAddressWasMigrated,
                childAddressWasMigrated
            });

            // Act
            sut.CorrectAddressApproval(parentAddressPersistentLocalId);

            // Assert
            var parentAddress =
                sut.StreetNameAddresses.First(x => x.AddressPersistentLocalId == parentAddressPersistentLocalId);
            var childAddress =
                sut.StreetNameAddresses.First(x => x.AddressPersistentLocalId == childAddressPersistentLocalId);

            parentAddress.Status.Should().Be(AddressStatus.Proposed);
            childAddress.Status.Should().Be(AddressStatus.Proposed);
        }
    }
}
