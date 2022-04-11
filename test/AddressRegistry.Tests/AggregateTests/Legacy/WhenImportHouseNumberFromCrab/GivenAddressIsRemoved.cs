namespace AddressRegistry.Tests.AggregateTests.Legacy.WhenImportHouseNumberFromCrab
{
    using Address;
    using Address.Commands.Crab;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Crab;
    using global::AutoFixture;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddressIsRemoved : AddressRegistryTest
    {
        public GivenAddressIsRemoved(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Theory]
        [DefaultData]
        public void ThenAddressRemovedException(
            Fixture fixture,
            AddressId addressId,
            ImportHouseNumberFromCrab importHouseNumberFromCrab)
        {
            Assert(RemovedAddressScenario(fixture)
                .When(importHouseNumberFromCrab)
                .Throws(new AddressRemovedException($"Cannot change removed address for address id {addressId}")));
        }

        [Theory]
        [DefaultData]
        public void ThenAddressRemovedExceptionIfModificationIsDelete(
            Fixture fixture,
            AddressId addressId,
            ImportHouseNumberFromCrab importHouseNumberFromCrab)
        {
            importHouseNumberFromCrab = importHouseNumberFromCrab.WithCrabModification(CrabModification.Delete);

            Assert(RemovedAddressScenario(fixture)
                .When(importHouseNumberFromCrab)
                .Throws(new AddressRemovedException($"Cannot change removed address for address id {addressId}")));
        }
    }
}
