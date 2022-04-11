namespace AddressRegistry.Tests.AggregateTests.Legacy.WhenMarkingAddressAsMigrated
{
    using Address;
    using Address.Events;
    using AddressRegistry.Address.Commands;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using global::AutoFixture;
    using StreetName.Commands;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddress : AddressRegistryTest
    {
        private readonly AddressId _addressId;

        public GivenAddress(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedAddressId());
            _addressId = Fixture.Create<AddressId>();
        }

        [Fact]
        public void ThenAddressWasMarkedAsMigrated()
        {
            var command = Fixture.Create<MarkAddressAsMigrated>();

            Assert(new Scenario()
                .Given(_addressId, Fixture.Create<AddressWasRegistered>())
                .When(command)
                .Then(
                    new Fact(_addressId, 
                        new AddressWasMigrated(_addressId, command.StreetNamePersistentLocalId))));
        }

        [Fact]
        public void AndAlreadyMigrated_ThenNone()
        {
            var command = Fixture.Create<MarkAddressAsMigrated>();

            Assert(new Scenario()
                .Given(
                    _addressId,
                    Fixture.Create<AddressWasRegistered>(),
                    Fixture.Create<AddressWasMigrated>())
                .When(command)
                .ThenNone());
        }
    }
}
