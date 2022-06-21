namespace AddressRegistry.Tests.AggregateTests.Legacy.WhenImportHouseNumberMailCantonFromCrab
{
    using Address;
    using Address.Commands.Crab;
    using Address.Events;
    using Address.Exceptions;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddressIsRemoved:AddressRegistryTest
    {
        public GivenAddressIsRemoved(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Theory, DefaultData]
        public void ThenAddressRemovedException(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            AddressWasRemoved addressWasRemoved,
            ImportHouseNumberMailCantonFromCrab importHouseNumberMailCantonFromCrab)
        {
            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    addressWasRemoved)
                .When(importHouseNumberMailCantonFromCrab)
                .Throws(new AddressRemovedException($"Cannot change removed address for address id {addressId}")));
        }
    }
}
