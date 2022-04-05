namespace AddressRegistry.Tests.AggregateTests.Legacy.WhenImportSubaddressStatusFromCrab
{
    using Address.Commands.Crab;
    using Address.Events;
    using Address.ValueObjects;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using global::AutoFixture;
    using WhenImportHouseNumberSubaddressFromCrab;
    using Xunit;

    public partial class GivenAddress
    {
        [Theory]
        [DefaultDataForSubaddress]
        public void ThenAddressWasCorrectedToProposedIfStatusIsProposedAndCorrection(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            ImportSubaddressStatusFromCrab importSubaddressStatusFromCrab)
        {
            importSubaddressStatusFromCrab = importSubaddressStatusFromCrab
                .WithStatus(CrabAddressStatus.Proposed)
                .WithCrabModification(CrabModification.Correction);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered)
                .When(importSubaddressStatusFromCrab)
                .Then(addressId,
                    new AddressWasCorrectedToProposed(addressId),
                    new AddressWasCorrectedToOfficiallyAssigned(addressId),
                    importSubaddressStatusFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultDataForSubaddress]
        public void ThenAddressWasCorrectedToProposedIfStatusIsReservedAndCorrection(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            ImportSubaddressStatusFromCrab importSubaddressStatusFromCrab)
        {
            importSubaddressStatusFromCrab = importSubaddressStatusFromCrab
                .WithStatus(CrabAddressStatus.Reserved)
                .WithCrabModification(CrabModification.Correction);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered)
                .When(importSubaddressStatusFromCrab)
                .Then(addressId,
                    new AddressWasCorrectedToProposed(addressId),
                    new AddressWasCorrectedToOfficiallyAssigned(addressId),
                    importSubaddressStatusFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultDataForSubaddress]
        public void ThenAddressWasCorrectedToCurrentIfStatusIsInUseAndCorrection(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            ImportSubaddressStatusFromCrab importSubaddressStatusFromCrab)
        {
            importSubaddressStatusFromCrab = importSubaddressStatusFromCrab
                .WithStatus(CrabAddressStatus.InUse)
                .WithCrabModification(CrabModification.Correction);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered)
                .When(importSubaddressStatusFromCrab)
                .Then(addressId,
                    new AddressWasCorrectedToCurrent(addressId),
                    new AddressWasCorrectedToOfficiallyAssigned(addressId),
                    importSubaddressStatusFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultDataForSubaddress]
        public void ThenAddressWasCorrectedToCurrentIfStatusIsOutOfUseAndCorrection(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            ImportSubaddressStatusFromCrab importSubaddressStatusFromCrab)
        {
            importSubaddressStatusFromCrab = importSubaddressStatusFromCrab
                .WithStatus(CrabAddressStatus.OutOfUse)
                .WithCrabModification(CrabModification.Correction);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered)
                .When(importSubaddressStatusFromCrab)
                .Then(addressId,
                    new AddressWasCorrectedToCurrent(addressId),
                    new AddressWasCorrectedToOfficiallyAssigned(addressId),
                    importSubaddressStatusFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultDataForSubaddress]
        public void ThenAddressWasCorrectedToCurrentIfStatusIsUnofficalAndCorrection(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            ImportSubaddressStatusFromCrab importSubaddressStatusFromCrab)
        {
            importSubaddressStatusFromCrab = importSubaddressStatusFromCrab
                .WithStatus(CrabAddressStatus.Unofficial)
                .WithCrabModification(CrabModification.Correction);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered)
                .When(importSubaddressStatusFromCrab)
                .Then(addressId,
                    new AddressWasCorrectedToCurrent(addressId),
                    new AddressWasCorrectedToNotOfficiallyAssigned(addressId),
                    importSubaddressStatusFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultDataForSubaddress]
        public void ThenAddressWasCorrectedToRetiredIfLifetimeIsFiniteAndCorrection(
            Fixture fixture,
            AddressId addressId,
            ImportHouseNumberSubaddressFromCrab importHouseNumberSubaddressFromCrab)
        {
            importHouseNumberSubaddressFromCrab = importHouseNumberSubaddressFromCrab
                .WithLifetime(new Fixture().Customize(new NodaTimeCustomization()).Customize(new WithFiniteLifetime()).Create<CrabLifetime>())
                .WithCrabModification(CrabModification.Correction);

            Assert(RegisteredAddressScenario(fixture)
                .When(importHouseNumberSubaddressFromCrab)
                .Then(addressId,
                    new AddressWasCorrectedToRetired(addressId),
                    importHouseNumberSubaddressFromCrab.ToLegacyEvent()));
        }
    }
}
