namespace AddressRegistry.Tests.AggregateTests.Legacy.WhenImportSubaddressStatusFromCrab
{
    using Address;
    using Address.Commands.Crab;
    using Address.Events;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using global::AutoFixture;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddressWithPosition : AddressRegistryTest
    {
        public GivenAddressWithPosition(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Theory]
        [DefaultDataForSubaddress]
        public void ThenAddressBecameComplete(
            Fixture fixture,
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            AddressWasPositioned addressWasPositioned,
            ImportSubaddressStatusFromCrab importSubaddressStatusFromCrab)
        {
            importSubaddressStatusFromCrab = importSubaddressStatusFromCrab.WithStatus(CrabAddressStatus.Proposed);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    addressWasPositioned)
                .When(importSubaddressStatusFromCrab)
                .Then(addressId,
                    new AddressWasProposed(addressId),
                    new AddressWasOfficiallyAssigned(addressId),
                    new AddressBecameComplete(addressId),
                    importSubaddressStatusFromCrab.ToLegacyEvent()));
        }
    }
}
