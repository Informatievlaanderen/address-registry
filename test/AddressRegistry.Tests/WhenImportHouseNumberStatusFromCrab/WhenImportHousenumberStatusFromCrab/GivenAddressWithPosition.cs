namespace AddressRegistry.Tests.WhenImportHouseNumberStatusFromCrab
{
    using Address.Commands.Crab;
    using Address.Events;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using AutoFixture;
    using global::AutoFixture;
    using WhenImportHouseNumberStatusFromCrab;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddressWithPosition : AddressRegistryTest
    {
        public GivenAddressWithPosition(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Theory, DefaultData]
        public void ThenAddressBecameComplete(
            Fixture fixture,
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            AddressWasPositioned addressWasPositioned,
            ImportHouseNumberStatusFromCrab importHouseNumberStatusFromCrab)
        {
            importHouseNumberStatusFromCrab = importHouseNumberStatusFromCrab.WithStatus(CrabAddressStatus.Proposed);

            Assert(new Scenario()
            .Given(addressId,
                    addressWasRegistered,
                    addressWasPositioned)
            .When(importHouseNumberStatusFromCrab)
            .Then(addressId,
                    new AddressWasProposed(addressId),
                    new AddressWasOfficiallyAssigned(addressId),
                    new AddressBecameComplete(addressId),
                    importHouseNumberStatusFromCrab.ToLegacyEvent()));
        }
    }
}
