namespace AddressRegistry.Tests.AggregateTests.Legacy.WhenImportSubaddressStatusFromCrab
{
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

        [Theory]
        [DefaultDataForSubaddress]
        public void ThenAddressBecameIncompleteWhenModificationIsDelete(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            AddressWasProposed addressWasProposed,
            AddressWasOfficiallyAssigned addressWasOfficiallyAssigned,
            AddressWasPositioned addressWasPositioned,
            AddressBecameComplete addressBecameComplete,
            ImportSubaddressStatusFromCrab importSubaddressStatusFromCrab)
        {
            importSubaddressStatusFromCrab = importSubaddressStatusFromCrab.WithCrabModification(CrabModification.Delete);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    addressWasProposed,
                    addressWasOfficiallyAssigned,
                    addressWasPositioned,
                    addressBecameComplete)
                .When(importSubaddressStatusFromCrab)
                .Then(addressId,
                    new AddressStatusWasRemoved(addressId),
                    new AddressOfficialAssignmentWasRemoved(addressId),
                    new AddressBecameIncomplete(addressId),
                    importSubaddressStatusFromCrab.ToLegacyEvent()));
        }
    }
}
