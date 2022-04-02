namespace AddressRegistry.Tests.AggregateTests.Legacy.WhenImportSubaddressStatusFromCrab
{
    using Address.Commands.Crab;
    using Address.Events;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddressIsRemoved : AddressRegistryTest
    {
        public GivenAddressIsRemoved(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Theory]
        [DefaultDataForSubaddress]
        public void ThenAddressRemovedException(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            AddressWasRemoved addressWasRemoved,
            ImportSubaddressStatusFromCrab importSubaddressStatusFromCrab)
        {
            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    addressWasRemoved
                )
                .When(importSubaddressStatusFromCrab)
                .ThenNone()); //Changed due to in some edge cases modify events occur after delete and deletes of those properties occurred too
        }
    }
}
