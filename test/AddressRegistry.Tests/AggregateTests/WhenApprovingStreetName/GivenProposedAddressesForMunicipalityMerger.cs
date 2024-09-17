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
        public void ThenProposedAddressesWithDesiredStatusCurrentAreApproved()
        {
            var command = Fixture.Create<ApproveStreetName>();

            var mergerAddressPersistentLocalIdOne = new AddressPersistentLocalId(1);
            var mergerAddressPersistentLocalIdTwo = new AddressPersistentLocalId(2);
            var addressPersistentLocalId = new AddressPersistentLocalId(3);

            var addressWasProposedForMunicipalityMergerOne = Fixture.Create<AddressWasProposedForMunicipalityMerger>()
                .AsHouseNumberAddress()
                .WithAddressPersistentLocalId(mergerAddressPersistentLocalIdOne)
                .WithDesiredStatus(AddressStatus.Current);

            var addressWasProposedForMunicipalityMergerTwo = Fixture.Create<AddressWasProposedForMunicipalityMerger>()
                .AsHouseNumberAddress()
                .WithAddressPersistentLocalId(mergerAddressPersistentLocalIdTwo)
                .WithDesiredStatus(AddressStatus.Current);

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

        [Fact]
        public void ThenProposedAddressesWithDesiredStatusProposedStayProposed()
        {
            var command = Fixture.Create<ApproveStreetName>();

            var mergerAddressPersistentLocalIdOne = new AddressPersistentLocalId(1);
            var mergerAddressPersistentLocalIdOneBoxOne = new AddressPersistentLocalId(11);
            var mergerAddressPersistentLocalIdTwo = new AddressPersistentLocalId(2);
            var addressPersistentLocalId = new AddressPersistentLocalId(3);

            var addressWasProposedForMunicipalityMergerOne = Fixture.Create<AddressWasProposedForMunicipalityMerger>()
                .AsHouseNumberAddress()
                .WithAddressPersistentLocalId(mergerAddressPersistentLocalIdOne)
                .WithDesiredStatus(AddressStatus.Proposed);

            var addressWasProposedForMunicipalityMergerOneBoxOne = Fixture.Create<AddressWasProposedForMunicipalityMerger>()
                .AsBoxNumberAddress(mergerAddressPersistentLocalIdOne)
                .WithAddressPersistentLocalId(mergerAddressPersistentLocalIdOneBoxOne)
                .WithDesiredStatus(AddressStatus.Proposed);

            var addressWasProposedForMunicipalityMergerTwo = Fixture.Create<AddressWasProposedForMunicipalityMerger>()
                .AsHouseNumberAddress()
                .WithAddressPersistentLocalId(mergerAddressPersistentLocalIdTwo)
                .WithDesiredStatus(AddressStatus.Proposed);

            var addressWasRejected = Fixture.Create<AddressWasRejected>()
                .WithAddressPersistentLocalId(mergerAddressPersistentLocalIdTwo);

            var addressWasProposedV2 = Fixture.Create<AddressWasProposedV2>()
                .AsHouseNumberAddress()
                .WithAddressPersistentLocalId(addressPersistentLocalId);

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasProposedForMunicipalityMergerOne,
                    addressWasProposedForMunicipalityMergerOneBoxOne,
                    addressWasProposedForMunicipalityMergerTwo,
                    addressWasRejected,
                    addressWasProposedV2)
                .When(command)
                .Then(
                    new Fact(new StreetNameStreamId(command.PersistentLocalId),
                    new StreetNameWasApproved(command.PersistentLocalId))));
        }
    }
}
