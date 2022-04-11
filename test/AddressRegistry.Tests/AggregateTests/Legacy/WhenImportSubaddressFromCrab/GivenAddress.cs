namespace AddressRegistry.Tests.AggregateTests.Legacy.WhenImportSubaddressFromCrab
{
    using Address;
    using Address.Commands.Crab;
    using Address.Events;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Crab;
    using global::AutoFixture;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddress : AddressRegistryTest
    {
        public GivenAddress(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Theory]
        [DefaultDataForSubaddress]
        public void ThenAddressBoxNumberWasChanged(
            Fixture fixture,
            AddressId addressId,
            ImportSubaddressFromCrab importSubaddressFromCrab
        )
        {
            Assert(RegisteredAddressScenario(fixture)
                .When(importSubaddressFromCrab)
                .Then(addressId,
                    new AddressBoxNumberWasChanged(addressId, new BoxNumber(importSubaddressFromCrab.BoxNumber)),
                    importSubaddressFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultDataForSubaddress]
        public void ThenNoChangeWhenBoxNumberIsTheSame(
            Fixture fixture,
            AddressId addressId,
            AddressBoxNumberWasChanged addressBoxNumberWasChanged,
            ImportSubaddressFromCrab importSubaddressFromCrab
        )
        {
            importSubaddressFromCrab = importSubaddressFromCrab
                .WithBoxNumber(new BoxNumber(addressBoxNumberWasChanged.BoxNumber));

            Assert(RegisteredAddressScenario(fixture)
                .Given(addressId, addressBoxNumberWasChanged)
                .When(importSubaddressFromCrab)
                .Then(addressId,
                    importSubaddressFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultDataForSubaddress]
        public void ThenAddressBoxNumberWasRemovedWhenBoxNumberIsEmpty(
            Fixture fixture,
            AddressId addressId,
            ImportSubaddressFromCrab importSubaddressFromCrab
        )
        {
            importSubaddressFromCrab = importSubaddressFromCrab
                .WithBoxNumber(new BoxNumber(string.Empty));

            Assert(RegisteredAddressScenario(fixture)
                .Given<AddressBoxNumberWasChanged>(addressId)
                .When(importSubaddressFromCrab)
                .Then(addressId,
                    new AddressBoxNumberWasRemoved(addressId),
                    importSubaddressFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultDataForSubaddress]
        public void ThenAddressBoxNumberWasCorrectedWhenModificationIsCorrection(
            Fixture fixture,
            AddressId addressId,
            ImportSubaddressFromCrab importSubaddressFromCrab
        )
        {
            importSubaddressFromCrab = importSubaddressFromCrab.WithCrabModification(CrabModification.Correction);

            Assert(RegisteredAddressScenario(fixture)
                .When(importSubaddressFromCrab)
                .Then(addressId,
                    new AddressBoxNumberWasCorrected(addressId, new BoxNumber(importSubaddressFromCrab.BoxNumber)),
                    importSubaddressFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultDataForSubaddress]
        public void ThenAddressWasRemovedWhenModificationIsDelete(
            Fixture fixture,
            AddressId addressId,
            ImportSubaddressFromCrab importSubaddressFromCrab)
        {
            importSubaddressFromCrab = importSubaddressFromCrab
                .WithCrabModification(CrabModification.Delete);

            Assert(RegisteredAddressScenario(fixture)
                .Given<AddressBoxNumberWasChanged>(addressId)
                .When(importSubaddressFromCrab)
                .Then(addressId,
                    new AddressWasRemoved(addressId),
                    importSubaddressFromCrab.ToLegacyEvent()));

        }

        [Theory]
        [DefaultDataForSubaddress]
        public void ThenAddressWasRetiredWhenLifetimeIsFinite(
            Fixture fixture,
            AddressId addressId,
            ImportSubaddressFromCrab importSubaddressFromCrab)
        {
            importSubaddressFromCrab = importSubaddressFromCrab
                .WithLifetime(new Fixture().Customize(new NodaTimeCustomization()).Customize(new WithFiniteLifetime()).Create<CrabLifetime>());

            Assert(RegisteredAddressScenario(fixture)
                .When(importSubaddressFromCrab)
                .Then(addressId,
                    new AddressBoxNumberWasChanged(addressId, importSubaddressFromCrab.BoxNumber),
                    new AddressWasRetired(addressId),
                    importSubaddressFromCrab.ToLegacyEvent()));

        }
    }
}
