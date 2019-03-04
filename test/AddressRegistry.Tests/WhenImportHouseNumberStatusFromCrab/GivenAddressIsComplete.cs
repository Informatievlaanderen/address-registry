namespace AddressRegistry.Tests.WhenImportHouseNumberStatusFromCrab
{
    using Address.Commands.Crab;
    using Address.Events;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using AutoFixture;
    using WhenImportHouseNumberStatusFromCrab;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddressIsComplete:AddressRegistryTest
    {
        public GivenAddressIsComplete(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Theory, DefaultData]
        public void ThenAddressBecameIncompleteWhenModificationIsDelete(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            AddressWasProposed addressWasProposed,
            AddressWasOfficiallyAssigned addressWasOfficiallyAssigned,
            AddressWasPositioned addressWasPositioned,
            AddressBecameComplete addressBecameComplete,
            ImportHouseNumberStatusFromCrab importHouseNumberStatusFromCrab)
        {
            importHouseNumberStatusFromCrab = importHouseNumberStatusFromCrab.WithCrabModification(CrabModification.Delete);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    addressWasProposed,
                    addressWasOfficiallyAssigned,
                    addressWasPositioned,
                    addressBecameComplete)
                .When(importHouseNumberStatusFromCrab)
                .Then(addressId,
                    new AddressStatusWasRemoved(addressId),
                    new AddressOfficialAssignmentWasRemoved(addressId),
                    new AddressBecameIncomplete(addressId),
                    importHouseNumberStatusFromCrab.ToLegacyEvent()));
        }
    }
}
