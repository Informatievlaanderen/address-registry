namespace AddressRegistry.Tests.WhenImportHouseNumberFromCrab
{
    using Address.Commands.Crab;
    using Address.Events;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using AutoFixture;
    using global::AutoFixture;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddressIsRetired : AddressRegistryTest
    {
        public GivenAddressIsRetired(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Theory]
        [DefaultData]
        public void ThenAddressWasProposedIfPreviousStatusWasProposed(
            Fixture fixture,
            AddressId addressId,
            ImportHouseNumberFromCrab importHouseNumberFromCrab)
        {
            Assert(RegisteredAddressScenario(fixture)
                .Given<AddressWasProposed>(addressId)
                .Given<AddressWasRetired>(addressId)
                .When(importHouseNumberFromCrab)
                .Then(addressId,
                    new AddressWasProposed(addressId),
                    importHouseNumberFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void ThenAddressBecameCurrentIfPreviousStatusWasCurrent(
            Fixture fixture,
            AddressId addressId,
            ImportHouseNumberFromCrab importHouseNumberFromCrab)
        {
            Assert(RegisteredAddressScenario(fixture)
                .Given<AddressBecameCurrent>(addressId)
                .Given<AddressWasRetired>(addressId)
                .When(importHouseNumberFromCrab)
                .Then(addressId,
                    new AddressBecameCurrent(addressId),
                    importHouseNumberFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void ThenAddressStatusWasRemovedIfPreviousStatusWasRemoved(
            Fixture fixture,
            AddressId addressId,
            ImportHouseNumberFromCrab importHouseNumberFromCrab)
        {
            Assert(RegisteredAddressScenario(fixture)
                .Given<AddressStatusWasRemoved>(addressId)
                .Given<AddressWasRetired>(addressId)
                .When(importHouseNumberFromCrab)
                .Then(addressId,
                    new AddressStatusWasRemoved(addressId),
                    importHouseNumberFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void ThenNoStatusChangeIfLifetimeIsFinite(
            Fixture fixture,
            AddressId addressId,
            ImportHouseNumberFromCrab importHouseNumberFromCrab)
        {
            importHouseNumberFromCrab = importHouseNumberFromCrab
                .WithLifetime(new Fixture().Customize(new NodaTimeCustomization()).Customize(new WithFiniteLifetime()).Create<CrabLifetime>());

            Assert(RetiredAddressScenario(fixture)
                .When(importHouseNumberFromCrab)
                .Then(addressId,
                    importHouseNumberFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void ThenAddressWasCorrectedToProposedIfPreviousStatusWasProposed(
            Fixture fixture,
            AddressId addressId,
            ImportHouseNumberFromCrab importHouseNumberFromCrab)
        {
            importHouseNumberFromCrab = importHouseNumberFromCrab.WithCrabModification(CrabModification.Correction);

            Assert(RegisteredAddressScenario(fixture)
                .Given<AddressWasProposed>(addressId)
                .Given<AddressWasRetired>(addressId)
                .When(importHouseNumberFromCrab)
                .Then(addressId,
                    new AddressWasCorrectedToProposed(addressId),
                    importHouseNumberFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void ThenAddressWasCorrectedToCurrentIfPreviousStatusWasCurrent(
            Fixture fixture,
            AddressId addressId,
            ImportHouseNumberFromCrab importHouseNumberFromCrab)
        {
            importHouseNumberFromCrab = importHouseNumberFromCrab.WithCrabModification(CrabModification.Correction);

            Assert(RegisteredAddressScenario(fixture)
                .Given<AddressBecameCurrent>(addressId)
                .Given<AddressWasRetired>(addressId)
                .When(importHouseNumberFromCrab)
                .Then(addressId,
                    new AddressWasCorrectedToCurrent(addressId),
                    importHouseNumberFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void ThenAddressStatusWasCorrectedToRemovedIfPreviousStatusWasRemoved(
            Fixture fixture,
            AddressId addressId,
            ImportHouseNumberFromCrab importHouseNumberFromCrab)
        {
            importHouseNumberFromCrab = importHouseNumberFromCrab.WithCrabModification(CrabModification.Correction);

            Assert(RegisteredAddressScenario(fixture)
                .Given<AddressStatusWasRemoved>(addressId)
                .Given<AddressWasRetired>(addressId)
                .When(importHouseNumberFromCrab)
                .Then(addressId,
                    new AddressStatusWasCorrectedToRemoved(addressId),
                    importHouseNumberFromCrab.ToLegacyEvent()));
        }
    }
}
