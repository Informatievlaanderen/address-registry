namespace AddressRegistry.Tests.AggregateTests.Legacy.WhenImportSubaddressStatusFromCrab
{
    using Address;
    using Address.Commands.Crab;
    using Address.Events;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddressIsRetired : AddressRegistryTest
    {
        public GivenAddressIsRetired(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Theory]
        [DefaultDataForSubaddress]
        public void ThenNoStatusChangeButNotOfficiallyAssigned(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            AddressWasRetired addressWasRetired,
            ImportSubaddressStatusFromCrab importSubaddressStatusFromCrab)
        {
            importSubaddressStatusFromCrab = importSubaddressStatusFromCrab.WithStatus(CrabAddressStatus.Unofficial);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    addressWasRetired)
                .When(importSubaddressStatusFromCrab)
                .Then(addressId,
                    new AddressBecameNotOfficiallyAssigned(addressId),
                    importSubaddressStatusFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultDataForSubaddress]
        public void ThenNoStatusChangeButOfficiallyAssigned(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            AddressWasRetired addressWasRetired,
            ImportSubaddressStatusFromCrab importSubaddressStatusFromCrab)
        {
            importSubaddressStatusFromCrab = importSubaddressStatusFromCrab.WithStatus(CrabAddressStatus.InUse);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    addressWasRetired)
                .When(importSubaddressStatusFromCrab)
                .Then(addressId,
                    new AddressWasOfficiallyAssigned(addressId),
                    importSubaddressStatusFromCrab.ToLegacyEvent()));
        }
    }
}
