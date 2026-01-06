namespace AddressRegistry.Tests.AggregateTests.WhenCorrectingAddressRegularization
{
    using System.Collections.Generic;
    using System.Linq;
    using Api.BackOffice.Abstractions;
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
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());
            Fixture.Customize(new WithFixedAddressPersistentLocalId());
            _streamId = Fixture.Create<StreetNameStreamId>();
        }

        [Fact]
        public void ThenAddressWasDeRegulated()
        {
            var addressWasProposedV2 = Fixture.Create<AddressWasProposedV2>().AsHouseNumberAddress();
            var addressWasRegularized = Fixture.Create<AddressWasRegularized>();

            var command = Fixture.Create<CorrectAddressRegularization>();

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasProposedV2,
                    addressWasRegularized)
                .When(command)
                .Then(new Fact(_streamId, Fixture.Create<AddressRegularizationWasCorrected>())));
        }

        [Fact]
        public void WithoutProposedAddress_ThenThrowsAddressNotFoundException()
        {
            var command = Fixture.Create<CorrectAddressRegularization>();

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

            var addressWasMigratedToStreetName = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress()
                .WithRemoved();

            var command = Fixture.Create<CorrectAddressRegularization>();

            Assert(new Scenario()
                .Given(_streamId,
                    streetNameWasImported,
                    addressWasMigratedToStreetName)
                .When(command)
                .Throws(new AddressIsRemovedException(Fixture.Create<AddressPersistentLocalId>())));
        }

        [Theory]
        [InlineData(AddressStatus.Rejected)]
        [InlineData(AddressStatus.Retired)]
        public void AddressWithInvalidStatus_ThenThrowsAddressHasInvalidStatusException(AddressStatus addressStatus)
        {
            var streetNameWasImported = Fixture.Create<StreetNameWasImported>().WithStatus(StreetNameStatus.Current);

            var addressWasMigratedToStreetName = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress()
                .WithStatus(addressStatus);

            var command = Fixture.Create<CorrectAddressRegularization>();

            Assert(new Scenario()
                .Given(_streamId,
                    streetNameWasImported,
                    addressWasMigratedToStreetName)
                .When(command)
                .Throws(new AddressHasInvalidStatusException(command.AddressPersistentLocalId)));
        }

        [Fact]
        public void WithAlreadyDeRegulatedAddress_ThenNone()
        {
            var addressWasProposedV2 = Fixture.Create<AddressWasProposedV2>().AsHouseNumberAddress();
            var addressWasDeregulated = Fixture.Create<AddressWasDeregulated>();

            var command = Fixture.Create<CorrectAddressRegularization>();

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasProposedV2,
                    addressWasDeregulated)
                .When(command)
                .ThenNone());
        }

        [Theory]
        [InlineData(AddressStatus.Current)]
        [InlineData(AddressStatus.Proposed)]
        public void StateCheck(AddressStatus status)
        {
            // Arrange
            var addressWasMigratedToStreetName = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress()
                .WithOfficiallyAssigned(true)
                .WithStatus(status);

            var sut = new StreetNameFactory(NoSnapshotStrategy.Instance).Create();
            sut.Initialize(new List<object> { addressWasMigratedToStreetName });

            // Act
            sut.CorrectAddressRegularization(Fixture.Create<AddressPersistentLocalId>());

            // Assert
            var address = sut.StreetNameAddresses.First(
                x => x.AddressPersistentLocalId == Fixture.Create<AddressPersistentLocalId>());

            address.IsOfficiallyAssigned.Should().BeFalse();
            address.Status.Should().Be(AddressStatus.Current);
        }
    }
}
