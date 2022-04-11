namespace AddressRegistry.Tests.AggregateTests.Legacy.WhenImportHouseNumberStatusFromCrab
{
    using Address;
    using Address.Commands.Crab;
    using Address.Events;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using WhenImportHousenumberStatusFromCrab;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddressIsRetired : AddressRegistryTest
    {
        public GivenAddressIsRetired(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Theory]
        [DefaultData]
        public void ThenNoStatusChangeButNotOfficiallyAssigned(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            AddressWasRetired addressWasRetired,
            ImportHouseNumberStatusFromCrab importHouseNumberStatusFromCrab)
        {
            importHouseNumberStatusFromCrab = importHouseNumberStatusFromCrab.WithStatus(CrabAddressStatus.Unofficial);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    addressWasRetired)
                .When(importHouseNumberStatusFromCrab)
                .Then(addressId,
                    new AddressBecameNotOfficiallyAssigned(addressId),
                    importHouseNumberStatusFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void ThenNoStatusChangeButOfficiallyAssigned(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            AddressWasRetired addressWasRetired,
            ImportHouseNumberStatusFromCrab importHouseNumberStatusFromCrab)
        {
            importHouseNumberStatusFromCrab = importHouseNumberStatusFromCrab.WithStatus(CrabAddressStatus.InUse);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    addressWasRetired)
                .When(importHouseNumberStatusFromCrab)
                .Then(addressId,
                    new AddressWasOfficiallyAssigned(addressId),
                    importHouseNumberStatusFromCrab.ToLegacyEvent()));
        }
    }
}
