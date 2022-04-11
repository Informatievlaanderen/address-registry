namespace AddressRegistry.Tests.AggregateTests.Legacy.WhenImportSubaddressFromCrab
{
    using Address;
    using Address.Commands.Crab;
    using Address.Events;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using WhenImportHouseNumberSubaddressFromCrab;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddressIsRemoved:AddressRegistryTest
    {
        public GivenAddressIsRemoved(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Theory, DefaultDataForSubaddress]
        public void ThenAddressRemovedException(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            AddressWasRemoved addressWasRemoved,
            ImportSubaddressFromCrab importSubaddressFromCrab)
        {
            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    addressWasRemoved)
                .When(importSubaddressFromCrab)
                .Throws(new AddressRemovedException($"Cannot change removed address for address id {addressId}")));
        }

        [Theory, DefaultDataForSubaddress]
        public void RemoveViaHouseNumberThenAddressRemovedException(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            AddressWasRemoved addressWasRemoved,
            ImportHouseNumberSubaddressFromCrab importHouseNumberFromCrab,
            ImportSubaddressFromCrab importSubaddressFromCrab)
        {
            importHouseNumberFromCrab = importHouseNumberFromCrab.WithCrabModification(CrabModification.Delete);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    importSubaddressFromCrab.ToLegacyEvent(),
                    addressWasRemoved)
                .When(importHouseNumberFromCrab)
                .Then(addressId, importHouseNumberFromCrab.ToLegacyEvent()));
        }

        [Theory, DefaultDataForSubaddress]
        public void RemoveViaSubaddressThenAddressRemovedException(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            AddressWasRemoved addressWasRemoved,
            ImportSubaddressFromCrab importSubaddressFromCrab)
        {
            var importSubaddressFromCrab2 = importSubaddressFromCrab.WithCrabModification(CrabModification.Delete);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    importSubaddressFromCrab.ToLegacyEvent(),
                    addressWasRemoved)
                .When(importSubaddressFromCrab2)
                .Then(addressId, importSubaddressFromCrab2.ToLegacyEvent()));
        }
    }
}
