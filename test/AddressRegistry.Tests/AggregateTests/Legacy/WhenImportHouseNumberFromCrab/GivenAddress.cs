namespace AddressRegistry.Tests.AggregateTests.Legacy.WhenImportHouseNumberFromCrab
{
    using Address.Commands.Crab;
    using Address.Events;
    using Address.ValueObjects;
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
        [DefaultData]
        public void ThenAddressWasRemovedIfModificationIsDeleted(
            Fixture fixture,
            AddressId addressId,
            ImportHouseNumberFromCrab importHouseNumberFromCrab)
        {
            importHouseNumberFromCrab = importHouseNumberFromCrab
                .WithCrabModification(CrabModification.Delete);

            Assert(RegisteredAddressScenario(fixture)
                .When(importHouseNumberFromCrab)
                .Then(addressId,
                    new AddressWasRemoved(addressId),
                    importHouseNumberFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void ThenAddressWasRetiredIfLifetimeIsFinite(
            Fixture fixture,
            AddressId addressId,
            ImportHouseNumberFromCrab importHouseNumberFromCrab)
        {
            importHouseNumberFromCrab = importHouseNumberFromCrab.
                WithLifetime(new Fixture().Customize(new NodaTimeCustomization()).Customize(new WithFiniteLifetime()).Create<CrabLifetime>());

            Assert(RegisteredAddressScenario(fixture)
                .When(importHouseNumberFromCrab)
                .Then(addressId,
                    new AddressWasRetired(addressId),
                    importHouseNumberFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void ThenHouseNumberChanged(
            Fixture fixture,
            AddressId addressId,
            ImportHouseNumberFromCrab importHouseNumberFromCrab)
        {
            importHouseNumberFromCrab = importHouseNumberFromCrab
                .WithHouseNumber(new Fixture().Create<HouseNumber>());

            Assert(RegisteredAddressScenario(fixture)
                .When(importHouseNumberFromCrab)
                .Then(addressId,
                    new AddressHouseNumberWasChanged(addressId, importHouseNumberFromCrab.HouseNumber),
                    importHouseNumberFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void GivenCorrectedHouseNumberThenHouseNumberChanged(
            Fixture fixture,
            AddressId addressId,
            AddressHouseNumberWasCorrected addressHouseNumberWasCorrected,
            ImportHouseNumberFromCrab importHouseNumberFromCrab)
        {
            addressHouseNumberWasCorrected = addressHouseNumberWasCorrected.WithHouseNumber(new Fixture().Create<HouseNumber>());

            Assert(RegisteredAddressScenario(fixture)
                .Given(addressId, addressHouseNumberWasCorrected)
                .When(importHouseNumberFromCrab)
                .Then(addressId,
                    new AddressHouseNumberWasChanged(addressId, importHouseNumberFromCrab.HouseNumber),
                    importHouseNumberFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void ThenHouseNumberWasCorrected(
            Fixture fixture,
            AddressId addressId,
            ImportHouseNumberFromCrab importHouseNumberFromCrab)
        {
            importHouseNumberFromCrab = importHouseNumberFromCrab
                .WithHouseNumber(new Fixture().Create<HouseNumber>())
                .WithCrabModification(CrabModification.Correction);

            Assert(RegisteredAddressScenario(fixture)
                .When(importHouseNumberFromCrab)
                .Then(addressId,
                    new AddressHouseNumberWasCorrected(addressId, importHouseNumberFromCrab.HouseNumber),
                    importHouseNumberFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void GivenNoStreetNameThenAddressStreetNameWasChanged(
            Fixture fixture,
            AddressId addressId,
            StreetNameId streetNameId,
            ImportHouseNumberFromCrab importHouseNumberFromCrab)
        {
            importHouseNumberFromCrab = importHouseNumberFromCrab
                .WithStreetNameId(new Fixture().Customize(new WithStreetNameIdExcept(streetNameId)).Create<CrabStreetNameId>());

            Assert(RegisteredAddressScenario(fixture)
                .When(importHouseNumberFromCrab)
                .Then(addressId,
                    new AddressStreetNameWasChanged(addressId, StreetNameId.CreateFor(importHouseNumberFromCrab.StreetNameId)),
                importHouseNumberFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void GivenStreetNameThenAddressStreetNameWasChanged(
            Fixture fixture,
            AddressId addressId,
            StreetNameId streetNameId,
            AddressStreetNameWasChanged streetNameWasChanged,
            ImportHouseNumberFromCrab importHouseNumberFromCrab)
        {
            streetNameWasChanged = streetNameWasChanged.WithStreetNameId(new Fixture().Customize(new WithStreetNameIdExcept(streetNameId)).Create<StreetNameId>());

            Assert(RegisteredAddressScenario(fixture)
                .Given(addressId, streetNameWasChanged)
                .When(importHouseNumberFromCrab)
                .Then(addressId,
                    new AddressStreetNameWasChanged(addressId, StreetNameId.CreateFor(importHouseNumberFromCrab.StreetNameId)),
                    importHouseNumberFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void GivenCorrectedStreetNameThenAddressStreetNameWasChanged(
            Fixture fixture,
            AddressId addressId,
            StreetNameId streetNameId,
            AddressStreetNameWasCorrected addressStreetNameWasCorrected,
            ImportHouseNumberFromCrab importHouseNumberFromCrab)
        {
            addressStreetNameWasCorrected = addressStreetNameWasCorrected.WithStreetNameId(new Fixture().Customize(new WithStreetNameIdExcept(streetNameId)).Create<StreetNameId>());

            Assert(RegisteredAddressScenario(fixture)
                .Given(addressId, addressStreetNameWasCorrected)
                .When(importHouseNumberFromCrab)
                .Then(addressId,
                    new AddressStreetNameWasChanged(addressId, StreetNameId.CreateFor(importHouseNumberFromCrab.StreetNameId)),
                    importHouseNumberFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void ThenAddressStreetNameWasCorrected(
            Fixture fixture,
            AddressId addressId,
            StreetNameId streetNameId,
            ImportHouseNumberFromCrab importHouseNumberFromCrab)
        {
            importHouseNumberFromCrab = importHouseNumberFromCrab
                .WithStreetNameId(new Fixture().Customize(new WithStreetNameIdExcept(streetNameId)).Create<CrabStreetNameId>())
                .WithCrabModification(CrabModification.Correction);

            Assert(RegisteredAddressScenario(fixture)
                .When(importHouseNumberFromCrab)
                .Then(addressId,
                    new AddressStreetNameWasCorrected(addressId, StreetNameId.CreateFor(importHouseNumberFromCrab.StreetNameId)),
                importHouseNumberFromCrab.ToLegacyEvent()));
        }
    }
}
