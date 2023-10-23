namespace AddressRegistry.Tests.AggregateTests.Legacy.WhenImportHouseNumberMailCantonFromCrab
{
    using Address;
    using Address.Commands.Crab;
    using Address.Crab;
    using Address.Events;
    using Address.Events.Crab;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using EventExtensions;
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
        public void ThenAddressPostalCodeWasChanged(
            Fixture fixture,
            AddressId addressId,
            ImportHouseNumberMailCantonFromCrab importHouseNumberMailCantonFromCrab
        )
        {
            Assert(RegisteredAddressScenario(fixture)
                .When(importHouseNumberMailCantonFromCrab)
                .Then(addressId,
                    new AddressPostalCodeWasChanged(addressId, new PostalCode(importHouseNumberMailCantonFromCrab.MailCantonCode)),
                    importHouseNumberMailCantonFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void GivenChangedPostalCodeThenNoChangeWhenMailCantonCodeIsTheSame(
            Fixture fixture,
            AddressId addressId,
            AddressPostalCodeWasChanged addressPostalCodeWasChanged,
            ImportHouseNumberMailCantonFromCrab importHouseNumberMailCantonFromCrab
        )
        {
            importHouseNumberMailCantonFromCrab = importHouseNumberMailCantonFromCrab
                .WithCrabMailCantonCode(new CrabMailCantonCode(addressPostalCodeWasChanged.PostalCode));

            Assert(RegisteredAddressScenario(fixture)
                .Given(addressId, addressPostalCodeWasChanged)
                .When(importHouseNumberMailCantonFromCrab)
                .Then(addressId,
                    importHouseNumberMailCantonFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void GivenCorrectedPostalCodeThenNoChangeWhenMailCantonCodeIsTheSame(
            Fixture fixture,
            AddressId addressId,
            AddressPostalCodeWasCorrected addressPostalCodeWasCorrected,
            ImportHouseNumberMailCantonFromCrab importHouseNumberMailCantonFromCrab
        )
        {
            importHouseNumberMailCantonFromCrab = importHouseNumberMailCantonFromCrab
                .WithCrabMailCantonCode(new CrabMailCantonCode(addressPostalCodeWasCorrected.PostalCode));

            Assert(RegisteredAddressScenario(fixture)
                .Given(addressId, addressPostalCodeWasCorrected)
                .When(importHouseNumberMailCantonFromCrab)
                .Then(addressId,
                    importHouseNumberMailCantonFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void ThenAddressPostalCodeWasRemovedWhenModificationIsDelete(
            Fixture fixture,
            AddressId addressId,
            AddressPostalCodeWasChanged addressPostalCodeWasChanged,
            AddressHouseNumberMailCantonWasImportedFromCrab addressHouseNumberMailCantonWasImportedFromCrab,
            ImportHouseNumberMailCantonFromCrab importHouseNumberMailCantonFromCrab
        )
        {
            importHouseNumberMailCantonFromCrab = importHouseNumberMailCantonFromCrab
                .WithCrabModification(CrabModification.Delete)
                .WithCrabMailCantonCode(new CrabMailCantonCode(addressPostalCodeWasChanged.PostalCode));

            addressHouseNumberMailCantonWasImportedFromCrab =  addressHouseNumberMailCantonWasImportedFromCrab
                .WithCrabModification(CrabModification.Insert)
                .WithHouseNumberMailCantonId(importHouseNumberMailCantonFromCrab.HouseNumberMailCantonId)
                .WithPostalCode(addressPostalCodeWasChanged.PostalCode);

            Assert(RegisteredAddressScenario(fixture)
                .Given(addressId, addressPostalCodeWasChanged, addressHouseNumberMailCantonWasImportedFromCrab)
                .When(importHouseNumberMailCantonFromCrab)
                .Then(addressId,
                    new AddressPostalCodeWasRemoved(addressId),
                    importHouseNumberMailCantonFromCrab.ToLegacyEvent()));
        }

        [Theory, DefaultData]
        public void ThenAddressPostalCodeWasChangedWhenNewerLifetime(
            Fixture fixture,
            AddressId addressId,
            CrabLifetime lifetime,
            ImportHouseNumberMailCantonFromCrab importHouseNumberMailCantonFromCrab
        )
        {
            importHouseNumberMailCantonFromCrab = importHouseNumberMailCantonFromCrab
                .WithLifetime(lifetime);

            Assert(RegisteredAddressScenario(fixture)
                .Given<AddressPostalCodeWasChanged>(addressId)
                .Given<AddressHouseNumberMailCantonWasImportedFromCrab>(addressId, e => e.WithBeginDate(lifetime.BeginDateTime.Value.PlusDays(-1)))
                .When(importHouseNumberMailCantonFromCrab)
                .Then(addressId,
                    new AddressPostalCodeWasChanged(addressId, new PostalCode(importHouseNumberMailCantonFromCrab.MailCantonCode)),
                    importHouseNumberMailCantonFromCrab.ToLegacyEvent()));
        }

        [Theory, DefaultData]
        public void ThenNoPostalCodeChangeWhenOlderLifetime(
            Fixture fixture,
            AddressId addressId,
            CrabLifetime lifetime,
            string mailCantonCode,
            Provenance provenance,
            AddressHouseNumberMailCantonWasImportedFromCrab addressHouseNumberMailCantonWasImportedFromCrab,
            ImportHouseNumberMailCantonFromCrab importHouseNumberMailCantonFromCrab)
        {
            addressHouseNumberMailCantonWasImportedFromCrab = addressHouseNumberMailCantonWasImportedFromCrab
                .WithBeginDate(lifetime.BeginDateTime.Value.PlusDays(1))
                .WithPostalCode(mailCantonCode);

            importHouseNumberMailCantonFromCrab = importHouseNumberMailCantonFromCrab
                .WithLifetime(lifetime);

            var addressPostalCodeWasChanged = new AddressPostalCodeWasChanged(addressId, new PostalCode(mailCantonCode));
            ((ISetProvenance) addressPostalCodeWasChanged).SetProvenance(provenance);

            Assert(RegisteredAddressScenario(fixture)
                .Given(addressId,
                    addressPostalCodeWasChanged,
                    addressHouseNumberMailCantonWasImportedFromCrab)
                .When(importHouseNumberMailCantonFromCrab)
                .Then(addressId,
                    importHouseNumberMailCantonFromCrab.ToLegacyEvent()));
        }

        [Theory, DefaultData]
        public void WithRemovedPostalCodeWhenSameLifetimeOfPreviouslyRemovedPostalCode(
            Fixture fixture,
            AddressId addressId,
            CrabHouseNumberMailCantonId crabHouseNumberMailCantonId,
            ImportHouseNumberMailCantonFromCrab importHouseNumberMailCantonFromCrab,
            CrabLifetime lifetime
        )
        {
            importHouseNumberMailCantonFromCrab = importHouseNumberMailCantonFromCrab
                .WithLifetime(lifetime);

            Assert(RegisteredAddressScenario(fixture)
                .Given<AddressPostalCodeWasChanged>(addressId)
                .Given<AddressHouseNumberMailCantonWasImportedFromCrab>(addressId,
                    e => e.WithBeginDate(lifetime.BeginDateTime)
                        .WithHouseNumberMailCantonId(crabHouseNumberMailCantonId))
                .Given<AddressPositionWasRemoved>(addressId)
                .Given<AddressHouseNumberMailCantonWasImportedFromCrab>(addressId,
                    e => e.WithBeginDate(lifetime.BeginDateTime)
                        .WithHouseNumberMailCantonId(crabHouseNumberMailCantonId)
                        .WithCrabModification(CrabModification.Delete))
                .When(importHouseNumberMailCantonFromCrab)
                .Then(addressId,
                    new AddressPostalCodeWasChanged(addressId, new PostalCode(importHouseNumberMailCantonFromCrab.MailCantonCode)),
                    importHouseNumberMailCantonFromCrab.ToLegacyEvent()));
        }

        [Theory, DefaultData]
        public void WhenModificationIsCorrection(
            Fixture fixture,
            AddressId addressId,
            ImportHouseNumberMailCantonFromCrab importHouseNumberMailCantonFromCrab)
        {
            importHouseNumberMailCantonFromCrab = importHouseNumberMailCantonFromCrab
                .WithCrabModification(CrabModification.Correction);

            Assert(RegisteredAddressScenario(fixture)
                .Given<AddressPostalCodeWasChanged>(addressId)
                .When(importHouseNumberMailCantonFromCrab)
                .Then(addressId,
                    new AddressPostalCodeWasCorrected(addressId, new PostalCode(importHouseNumberMailCantonFromCrab.MailCantonCode)),
                    importHouseNumberMailCantonFromCrab.ToLegacyEvent()));
        }
    }
}
