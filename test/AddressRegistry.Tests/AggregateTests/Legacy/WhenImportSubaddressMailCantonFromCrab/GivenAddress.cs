namespace AddressRegistry.Tests.AggregateTests.Legacy.WhenImportSubaddressMailCantonFromCrab
{
    using Address.Commands.Crab;
    using Address.Events;
    using Address.ValueObjects;
    using AutoFixture;
    using global::AutoFixture;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddress : AddressRegistryTest
    {
        public GivenAddress(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        //ImportSubaddressMailCantonFromCrab routes to ImportHouseNumberMailCantonFromCrab which is already tested
        //Only one "happy path" test here, to check if the command is correctly routed

        [Theory]
        [DefaultDataForSubaddress]
        public void ThenAddressPostalCodeWasChanged(
            Fixture fixture,
            AddressId addressId,
            ImportSubaddressMailCantonFromCrab importSubaddressMailCantonFromCrab
        )
        {
            Assert(RegisteredAddressScenario(fixture)
                .When(importSubaddressMailCantonFromCrab)
                .Then(addressId,
                    new AddressPostalCodeWasChanged(addressId, new PostalCode(importSubaddressMailCantonFromCrab.MailCantonCode)),
                    importSubaddressMailCantonFromCrab.ToLegacyEvent()));
        }
    }
}
