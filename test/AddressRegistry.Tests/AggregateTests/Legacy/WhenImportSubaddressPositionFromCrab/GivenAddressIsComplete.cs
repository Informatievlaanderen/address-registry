namespace AddressRegistry.Tests.AggregateTests.Legacy.WhenImportSubaddressPositionFromCrab
{
    using Address;
    using Address.Commands.Crab;
    using Address.Events;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddressIsComplete : AddressRegistryTest
    {
        public GivenAddressIsComplete(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Theory, DefaultDataForSubaddress]
        public void ThenAddressBecameIncompleteWhenModificationIsDelete(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            AddressWasProposed addressWasProposed,
            AddressWasOfficiallyAssigned addressWasOfficiallyAssigned,
            AddressWasPositioned addressWasPositioned,
            AddressBecameComplete addressBecameComplete,
            ImportSubaddressPositionFromCrab importSubaddressPositionFromCrab)
        {
            //addressWasPositioned
            importSubaddressPositionFromCrab = importSubaddressPositionFromCrab.WithCrabModification(CrabModification.Delete);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    addressWasProposed,
                    addressWasOfficiallyAssigned,
                    addressWasPositioned,
                    addressBecameComplete)
                .When(importSubaddressPositionFromCrab)
                .Then(addressId,
                    new AddressPositionWasRemoved(addressId),
                    new AddressBecameIncomplete(addressId),
                    importSubaddressPositionFromCrab.ToLegacyEvent()));
        }
    }
}
