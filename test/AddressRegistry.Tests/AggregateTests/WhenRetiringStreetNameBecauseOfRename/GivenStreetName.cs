namespace AddressRegistry.Tests.AggregateTests.WhenRetiringStreetNameBecauseOfRename
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using EventExtensions;
    using global::AutoFixture;
    using StreetName;
    using StreetName.Commands;
    using StreetName.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenStreetName : AddressRegistryTest
    {
        private readonly StreetNameStreamId _streamId;

        public GivenStreetName(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());
            Fixture.Customizations.Add(new WithUniqueInteger());
            _streamId = Fixture.Create<StreetNameStreamId>();
        }

        [Fact]
        public void ThenAddressesWereRetiredOrRejectedAndStreetNameWasRetired()
        {
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();

            var command = Fixture.Create<RetireStreetNameBecauseOfRename>();

            var streetNameWasImported = Fixture.Create<StreetNameWasImported>()
                    .WithStatus(StreetNameStatus.Current);

            var proposedAddress = Fixture.Create<AddressWasProposedV2>()
                .AsHouseNumberAddress();
            var currentAddress = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress()
                .WithStatus(AddressStatus.Current);

            Assert(new Scenario()
                .Given(_streamId,
                    streetNameWasImported,
                    proposedAddress,
                    currentAddress)
                .When(command)
                .Then(
                    new Fact(new StreetNameStreamId(command.PersistentLocalId),
                        new AddressWasRejectedBecauseOfReaddress(streetNamePersistentLocalId, new AddressPersistentLocalId(proposedAddress.AddressPersistentLocalId))),
                    new Fact(new StreetNameStreamId(command.PersistentLocalId),
                        new AddressWasRetiredBecauseOfReaddress(streetNamePersistentLocalId, new AddressPersistentLocalId(currentAddress.AddressPersistentLocalId))),
                    new Fact(new StreetNameStreamId(command.PersistentLocalId),
                        new StreetNameWasRetired(streetNamePersistentLocalId))));
        }

        [Fact]
        public void WithInActiveAddresses_ThenStreetNameWasRetired()
        {
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();

            var command = Fixture.Create<RetireStreetNameBecauseOfRename>();

            var streetNameWasImported = Fixture.Create<StreetNameWasImported>()
                    .WithStatus(StreetNameStatus.Current);

            var retiredAddress = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress()
                .WithStatus(AddressStatus.Retired);
            var rejectedAddress = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress()
                .WithStatus(AddressStatus.Rejected);
            var removedAddress = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress()
                .WithStatus(AddressStatus.Current)
                .WithRemoved();

            Assert(new Scenario()
                .Given(_streamId,
                    streetNameWasImported,
                    retiredAddress,
                    rejectedAddress,
                    removedAddress)
                .When(command)
                .Then(new Fact(new StreetNameStreamId(command.PersistentLocalId),
                        new StreetNameWasRetired(streetNamePersistentLocalId))));
        }
    }
}
