namespace AddressRegistry.Tests.AggregateTests.Legacy.WhenImportSubaddressPositionFromCrab
{
    using Address.Commands.Crab;
    using Address.Events;
    using Address.ValueObjects;
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

        [Theory, DefaultDataForSubaddress]
        public void ThenAddressRemovedException(AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            AddressWasRemoved addressWasRemoved,
            ImportSubaddressPositionFromCrab importSubaddressPositionFromCrab)
        {
            Assert(new Scenario()
                .Given(addressId, addressWasRegistered, addressWasRemoved)
                .When(importSubaddressPositionFromCrab)
                .ThenNone()); //Changed due to in some edge cases modify events occur after delete and deletes of those properties occurred too
        }

        [Theory, DefaultDataForSubaddress]
        public void ThenNoExceptionWhenModificationIsDelete(AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            AddressWasRemoved addressWasRemoved,
            ImportSubaddressPositionFromCrab importSubaddressPositionFromCrab)
        {
            importSubaddressPositionFromCrab = importSubaddressPositionFromCrab
                .WithCrabModification(CrabModification.Delete);

            Assert(new Scenario()
                .Given(addressId, addressWasRegistered, addressWasRemoved)
                .When(importSubaddressPositionFromCrab)
                .Then(addressId,
                    importSubaddressPositionFromCrab.ToLegacyEvent()));
        }
    }
}
