namespace AddressRegistry.Tests.AggregateTests.Legacy.WhenImportHouseNumberPositionFromCrab
{
    using Address.Commands.Crab;
    using Address.Events;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddressIsRemoved : AddressRegistryTest
    {
        public GivenAddressIsRemoved(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Theory, DefaultData]
        public void ThenAddressRemovedException(AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            AddressWasRemoved addressWasRemoved,
            ImportHouseNumberPositionFromCrab importHouseNumberPositionFromCrab)
        {
            Assert(new Scenario()
                .Given(addressId, addressWasRegistered, addressWasRemoved)
                .When(importHouseNumberPositionFromCrab)
                .Throws(new AddressRemovedException($"Cannot change removed address for address id {addressId}")));
        }

        [Theory, DefaultData]
        public void ThenNoExceptionWhenModificationIsDelete(AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            AddressWasRemoved addressWasRemoved,
            ImportHouseNumberPositionFromCrab importHouseNumberPositionFromCrab)
        {
            importHouseNumberPositionFromCrab = importHouseNumberPositionFromCrab
                .WithCrabModification(CrabModification.Delete);

            Assert(new Scenario()
                .Given(addressId, addressWasRegistered, addressWasRemoved)
                .When(importHouseNumberPositionFromCrab)
                .Then(addressId,
                    importHouseNumberPositionFromCrab.ToLegacyEvent()));
        }
    }
}
