namespace AddressRegistry.Tests.AggregateTests.WhenCorrectingAddressRegularization
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using EventExtensions;
    using global::AutoFixture;
    using StreetName;
    using StreetName.Commands;
    using StreetName.Events;
    using StreetName.Exceptions;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenChildAddress : AddressRegistryTest
    {
        private readonly StreetNameStreamId _streamId;

        public GivenChildAddress(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());
            Fixture.Customize(new WithFixedAddressPersistentLocalId());
            _streamId = Fixture.Create<StreetNameStreamId>();
        }

        [Fact]
        public void WhenParentAddressIsProposed()
        {
            var houseNumberAddressWasProposedV2 = Fixture.Create<AddressWasProposedV2>()
                .AsHouseNumberAddress();

            var boxNumberAddressWasProposedV2 = Fixture.Create<AddressWasProposedV2>()
                .AsBoxNumberAddress(new AddressPersistentLocalId(houseNumberAddressWasProposedV2.AddressPersistentLocalId))
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(houseNumberAddressWasProposedV2.AddressPersistentLocalId + 1));

            var command = new CorrectAddressRegularization(
                Fixture.Create<StreetNamePersistentLocalId>(),
                new AddressPersistentLocalId(boxNumberAddressWasProposedV2.AddressPersistentLocalId),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    houseNumberAddressWasProposedV2,
                    boxNumberAddressWasProposedV2)
                .When(command)
                .Throws(new ParentAddressHasInvalidStatusException()));
        }

        [Theory]
        [InlineData(AddressStatus.Current)]
        [InlineData(AddressStatus.Rejected)]
        [InlineData(AddressStatus.Retired)]
        public void WhenParentAddressIsNotProposed(AddressStatus status)
        {
            var houseNumberAddressWasMigratedToStreetName = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress()
                .WithStatus(status);

            var boxNumberAddressWasProposedV2 = Fixture.Create<AddressWasProposedV2>()
                .AsBoxNumberAddress(new AddressPersistentLocalId(houseNumberAddressWasMigratedToStreetName.AddressPersistentLocalId))
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(houseNumberAddressWasMigratedToStreetName.AddressPersistentLocalId + 1));

            var command = new CorrectAddressRegularization(
                Fixture.Create<StreetNamePersistentLocalId>(),
                new AddressPersistentLocalId(boxNumberAddressWasProposedV2.AddressPersistentLocalId),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    houseNumberAddressWasMigratedToStreetName,
                    boxNumberAddressWasProposedV2)
                .When(command)
                .Then(new Fact(_streamId,
                    new AddressRegularizationWasCorrected(
                        Fixture.Create<StreetNamePersistentLocalId>(),
                        new AddressPersistentLocalId(boxNumberAddressWasProposedV2.AddressPersistentLocalId)))));
        }
    }
}
