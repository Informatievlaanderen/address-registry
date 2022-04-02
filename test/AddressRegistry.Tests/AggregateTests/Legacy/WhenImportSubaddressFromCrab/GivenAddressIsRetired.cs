namespace AddressRegistry.Tests.AggregateTests.Legacy.WhenImportSubaddressFromCrab
{
    using Address.Commands.Crab;
    using Address.Events;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Crab;
    using global::AutoFixture;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddressIsRetired : AddressRegistryTest
    {
        public GivenAddressIsRetired(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Theory]
        [DefaultDataForSubaddress]
        public void ThenNoRetiredEventWhenLifetimeIsFinite(
            Fixture fixture,
            AddressId addressId,
            ImportSubaddressFromCrab importSubaddressFromCrab)
        {
            importSubaddressFromCrab = importSubaddressFromCrab
                .WithLifetime(new Fixture().Customize(new NodaTimeCustomization()).Customize(new WithFiniteLifetime()).Create<CrabLifetime>());

            Assert(RetiredAddressScenario(fixture)
                .When(importSubaddressFromCrab)
                .Then(addressId,
                    new AddressBoxNumberWasChanged(addressId, importSubaddressFromCrab.BoxNumber),
                    importSubaddressFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultDataForSubaddress]
        public void ThenPreviousStatusEventWhenLifetimeIsInfinite(
            Fixture fixture,
            AddressId addressId,
            ImportSubaddressFromCrab importSubaddressFromCrab)
        {
            Assert(RetiredAddressScenario(fixture)
                .Given<AddressWasRegistered>(addressId)
                .Given<AddressBecameCurrent>(addressId)
                .Given<AddressWasRetired>(addressId)
                .When(importSubaddressFromCrab)
                .Then(addressId,
                    new AddressBoxNumberWasChanged(addressId, importSubaddressFromCrab.BoxNumber),
                    new AddressBecameCurrent(addressId),
                    importSubaddressFromCrab.ToLegacyEvent()));
        }
    }
}
