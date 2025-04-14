namespace AddressRegistry.Tests.AggregateTests.WhenRejectingStreetName
{
    using System.Collections.Generic;
    using StreetName;
    using StreetName.Commands;
    using StreetName.Events;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using EventBuilders;
    using FluentAssertions;
    using global::AutoFixture;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenStreetName : AddressRegistryTest
    {
        private readonly StreetNameStreamId _streamId;

        public GivenStreetName(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());
            _streamId = Fixture.Create<StreetNameStreamId>();
        }

        [Fact]
        public void ThenStreetNameWasRejected()
        {
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();

            var command = Fixture.Create<RejectStreetName>();

            var streetNameWasImported = new StreetNameWasImported(
                streetNamePersistentLocalId,
                Fixture.Create<MunicipalityId>(),
                StreetNameStatus.Proposed);
            ((ISetProvenance)streetNameWasImported).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId, streetNameWasImported)
                .When(command)
                .Then(
                    new Fact(new StreetNameStreamId(command.PersistentLocalId),
                        new StreetNameWasRejected(streetNamePersistentLocalId))));
        }

        [Fact]
        public void WithAlreadyRejectedStreetName_ThenNone()
        {
            var command = Fixture.Create<RejectStreetName>();

            var streetNameWasImported = new StreetNameWasImported(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<MunicipalityId>(),
                StreetNameStatus.Rejected);
            ((ISetProvenance)streetNameWasImported).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    streetNameWasImported)
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void WithAddresses_ThenAddressesWereRejectedOrRetired()
        {
            var command = Fixture.Create<RejectStreetName>();

            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
            var streetNameWasImported = new StreetNameWasImported(
                streetNamePersistentLocalId,
                Fixture.Create<MunicipalityId>(),
                StreetNameStatus.Proposed);
            ((ISetProvenance)streetNameWasImported).SetProvenance(Fixture.Create<Provenance>());

            var proposedHouseNumber =
                new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Proposed)
                    .WithAddressPersistentLocalId(new AddressPersistentLocalId(1))
                    .Build();

            var proposedBoxNumber =
                new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Proposed)
                    .WithAddressPersistentLocalId(new AddressPersistentLocalId(2))
                    .WithBoxNumber(
                        boxNumber: Fixture.Create<BoxNumber>(),
                        parentAddressPersistentLocalId: new AddressPersistentLocalId(proposedHouseNumber.AddressPersistentLocalId))
                    .Build();

            var currentHouseNumber =
                new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Current)
                    .WithAddressPersistentLocalId(new AddressPersistentLocalId(3))
                    .Build();

            var currentBoxNumber =
                new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Current)
                    .WithAddressPersistentLocalId(new AddressPersistentLocalId(4))
                    .WithBoxNumber(
                        boxNumber: Fixture.Create<BoxNumber>(),
                        parentAddressPersistentLocalId: new AddressPersistentLocalId(currentHouseNumber.AddressPersistentLocalId))
                    .Build();

            var proposedBoxNumberForCurrentHouseNumber =
                new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Proposed)
                    .WithAddressPersistentLocalId(new AddressPersistentLocalId(5))
                    .WithBoxNumber(
                        boxNumber: Fixture.Create<BoxNumber>(),
                        parentAddressPersistentLocalId: new AddressPersistentLocalId(currentHouseNumber.AddressPersistentLocalId))
                    .Build();

            Assert(new Scenario()
                .Given(_streamId,
                    streetNameWasImported,
                    proposedHouseNumber,
                    proposedBoxNumber,
                    currentHouseNumber,
                    currentBoxNumber,
                    proposedBoxNumberForCurrentHouseNumber)
                .When(command)
                .Then(
                    new Fact(new StreetNameStreamId(command.PersistentLocalId),
                        new AddressWasRejectedBecauseStreetNameWasRejected(streetNamePersistentLocalId, new AddressPersistentLocalId(proposedBoxNumber.AddressPersistentLocalId))),
                    new Fact(new StreetNameStreamId(command.PersistentLocalId),
                        new AddressWasRejectedBecauseStreetNameWasRejected(streetNamePersistentLocalId, new AddressPersistentLocalId(proposedBoxNumberForCurrentHouseNumber.AddressPersistentLocalId))),
                    new Fact(new StreetNameStreamId(command.PersistentLocalId),
                        new AddressWasRetiredBecauseStreetNameWasRejected(streetNamePersistentLocalId, new AddressPersistentLocalId(currentBoxNumber.AddressPersistentLocalId))),
                    new Fact(new StreetNameStreamId(command.PersistentLocalId),
                        new AddressWasRejectedBecauseStreetNameWasRejected(streetNamePersistentLocalId, new AddressPersistentLocalId(proposedHouseNumber.AddressPersistentLocalId))),
                    new Fact(new StreetNameStreamId(command.PersistentLocalId),
                        new AddressWasRetiredBecauseStreetNameWasRejected(streetNamePersistentLocalId, new AddressPersistentLocalId(currentHouseNumber.AddressPersistentLocalId))),
                    new Fact(new StreetNameStreamId(command.PersistentLocalId),
                        new StreetNameWasRejected(streetNamePersistentLocalId))));
        }

        [Fact]
        public void StateCheck()
        {
            // Arrange
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();

            var migratedStreetNameWasImported = new MigratedStreetNameWasImported(
                Fixture.Create<StreetNameId>(),
                streetNamePersistentLocalId,
                Fixture.Create<MunicipalityId>(),
                Fixture.Create<NisCode>(),
                StreetNameStatus.Proposed);
            ((ISetProvenance)migratedStreetNameWasImported).SetProvenance(Fixture.Create<Provenance>());

            var proposedAddressWasMigratedToStreetName = new AddressWasMigratedToStreetNameBuilder(Fixture)
                .WithStreetNamePersistentLocalId(streetNamePersistentLocalId)
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(1))
                .Build();

            var currentAddressWasMigratedToStreetName = new AddressWasMigratedToStreetNameBuilder(Fixture)
                .WithStreetNamePersistentLocalId(new StreetNamePersistentLocalId(streetNamePersistentLocalId))
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(2))
                .WithStatus(AddressStatus.Current)
                .Build();

            var sut = new StreetNameFactory(NoSnapshotStrategy.Instance).Create();
            sut.Initialize(new List<object> { migratedStreetNameWasImported, proposedAddressWasMigratedToStreetName, currentAddressWasMigratedToStreetName });

            // Act
            sut.RejectStreetName();

            // Assert
            sut.Status.Should().Be(StreetNameStatus.Rejected);
            sut.StreetNameAddresses.FindByPersistentLocalId(new AddressPersistentLocalId(proposedAddressWasMigratedToStreetName.AddressPersistentLocalId))!
                .Status
                .Should()
                .Be(AddressStatus.Rejected);

            sut.StreetNameAddresses.FindByPersistentLocalId(new AddressPersistentLocalId(currentAddressWasMigratedToStreetName.AddressPersistentLocalId))!
                .Status
                .Should()
                .Be(AddressStatus.Retired);
        }
    }
}
