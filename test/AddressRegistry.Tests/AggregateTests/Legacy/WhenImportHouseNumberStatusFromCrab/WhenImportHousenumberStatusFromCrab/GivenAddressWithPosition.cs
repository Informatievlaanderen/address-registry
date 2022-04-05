namespace AddressRegistry.Tests.AggregateTests.Legacy.WhenImportHouseNumberStatusFromCrab.WhenImportHousenumberStatusFromCrab
{
    using Address.Commands.Crab;
    using Address.Events;
    using Address.ValueObjects;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using global::AutoFixture;
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
