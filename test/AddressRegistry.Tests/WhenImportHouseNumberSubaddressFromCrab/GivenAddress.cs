namespace AddressRegistry.Tests.WhenImportHouseNumberSubaddressFromCrab
{
    using Address.Commands.Crab;
    using Address.Events;
    using AutoFixture;
    using global::AutoFixture;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddress:AddressRegistryTest
    {
        public GivenAddress(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        //ImportHouseNumberSubaddressFromCrab routes to ImportHouseNumberFromCrab which is already tested
        //Only one "happy path" test here, to check if the command is correctly routed

        [Theory]
        [DefaultDataForSubaddress]
        public void ThenHouseNumberChanged(
            Fixture fixture,
            AddressId addressId,
            ImportHouseNumberSubaddressFromCrab importHouseNumberSubaddressFromCrab)
        {
            importHouseNumberSubaddressFromCrab = importHouseNumberSubaddressFromCrab
                .WithHouseNumber(new Fixture().Create<HouseNumber>());

            Assert(RegisteredAddressScenario(fixture)
                .When(importHouseNumberSubaddressFromCrab)
                .Then(addressId,
                    new AddressHouseNumberWasChanged(addressId, importHouseNumberSubaddressFromCrab.HouseNumber),
                    importHouseNumberSubaddressFromCrab.ToLegacyEvent()));
        }
    }
}
