namespace AddressRegistry.Tests.AggregateTests.WhenApprovingStreetName
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

    public class GivenProposedAddressesForMunicipalityMerger : AddressRegistryTest
    {
        private readonly StreetNameStreamId _streamId;

        public GivenProposedAddressesForMunicipalityMerger(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());
            _streamId = Fixture.Create<StreetNameStreamId>();
        }

        [Fact]
        public void ThenProposedAddressesAreApproved()
        {
            var command = Fixture.Create<ApproveStreetName>();

            var mergerAddressPersistentLocalIdOne = new AddressPersistentLocalId(1);
            var mergerAddressPersistentLocalIdTwo = new AddressPersistentLocalId(2);
            var addressPersistentLocalId = new AddressPersistentLocalId(3);

            var addressWasProposedForMunicipalityMergerOne = Fixture.Create<AddressWasProposedForMunicipalityMerger>()
                .AsHouseNumberAddress()
                .WithAddressPersistentLocalId(mergerAddressPersistentLocalIdOne);

            var addressWasProposedForMunicipalityMergerTwo = Fixture.Create<AddressWasProposedForMunicipalityMerger>()
                .AsHouseNumberAddress()
                .WithAddressPersistentLocalId(mergerAddressPersistentLocalIdTwo);

            var addressWasRejected = Fixture.Create<AddressWasRejected>()
                .WithAddressPersistentLocalId(mergerAddressPersistentLocalIdTwo);

            var addressWasProposedV2 = Fixture.Create<AddressWasProposedV2>()
                .AsHouseNumberAddress()
                .WithAddressPersistentLocalId(addressPersistentLocalId);

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasProposedForMunicipalityMergerOne,
                    addressWasProposedForMunicipalityMergerTwo,
                    addressWasRejected,
                    addressWasProposedV2)
                .When(command)
                .Then(
                    new Fact(new StreetNameStreamId(command.PersistentLocalId),
                    new StreetNameWasApproved(command.PersistentLocalId)),
                    new Fact(new StreetNameStreamId(command.PersistentLocalId),
                    new AddressWasApproved(command.PersistentLocalId, mergerAddressPersistentLocalIdOne))));
        }
    }
}
