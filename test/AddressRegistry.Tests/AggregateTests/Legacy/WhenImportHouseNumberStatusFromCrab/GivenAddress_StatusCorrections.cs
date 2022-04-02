namespace AddressRegistry.Tests.AggregateTests.Legacy.WhenImportHouseNumberStatusFromCrab
{
    using Address.Commands.Crab;
    using Address.Events;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using global::AutoFixture;
    using WhenImportHouseNumberFromCrab;
    using WhenImportHousenumberStatusFromCrab;
    using Xunit;

    public partial class GivenAddress
    {
        [Theory]
        [DefaultData]
        public void ThenAddressWasCorrectedToProposedIfStatusIsProposedAndCorrection(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            ImportHouseNumberStatusFromCrab importHouseNumberStatusFromCrab)
        {
            importHouseNumberStatusFromCrab = importHouseNumberStatusFromCrab
                .WithStatus(CrabAddressStatus.Proposed)
                .WithCrabModification(CrabModification.Correction);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered)
                .When(importHouseNumberStatusFromCrab)
                .Then(addressId,
                    new AddressWasCorrectedToProposed(addressId),
                    new AddressWasCorrectedToOfficiallyAssigned(addressId),
                    importHouseNumberStatusFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void GivenPreviouslyCorrectedToProposedThenAddressWasCorrectedToCurrentIfStatusIsInUseAndCorrection(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            AddressWasCorrectedToProposed addressWasCorrectedToProposed,
            ImportHouseNumberStatusFromCrab importHouseNumberStatusFromCrab)
        {
            importHouseNumberStatusFromCrab = importHouseNumberStatusFromCrab
                .WithStatus(CrabAddressStatus.InUse)
                .WithCrabModification(CrabModification.Correction);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    addressWasCorrectedToProposed)
                .When(importHouseNumberStatusFromCrab)
                .Then(addressId,
                    new AddressWasCorrectedToCurrent(addressId),
                    new AddressWasCorrectedToOfficiallyAssigned(addressId),
                    importHouseNumberStatusFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void ThenAddressWasCorrectedToProposedIfStatusIsReservedAndCorrection(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            ImportHouseNumberStatusFromCrab importHouseNumberStatusFromCrab)
        {
            importHouseNumberStatusFromCrab = importHouseNumberStatusFromCrab
                .WithStatus(CrabAddressStatus.Reserved)
                .WithCrabModification(CrabModification.Correction);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered)
                .When(importHouseNumberStatusFromCrab)
                .Then(addressId,
                    new AddressWasCorrectedToProposed(addressId),
                    new AddressWasCorrectedToOfficiallyAssigned(addressId),
                    importHouseNumberStatusFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void ThenAddressWasCorrectedToCurrentIfStatusIsInUseAndCorrection(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            ImportHouseNumberStatusFromCrab importHouseNumberStatusFromCrab)
        {
            importHouseNumberStatusFromCrab = importHouseNumberStatusFromCrab
                .WithStatus(CrabAddressStatus.InUse)
                .WithCrabModification(CrabModification.Correction);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered)
                .When(importHouseNumberStatusFromCrab)
                .Then(addressId,
                    new AddressWasCorrectedToCurrent(addressId),
                    new AddressWasCorrectedToOfficiallyAssigned(addressId),
                    importHouseNumberStatusFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void ThenAddressWasCorrectedToCurrentIfStatusIsOutOfUseAndCorrection(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            ImportHouseNumberStatusFromCrab importHouseNumberStatusFromCrab)
        {
            importHouseNumberStatusFromCrab = importHouseNumberStatusFromCrab
                .WithStatus(CrabAddressStatus.OutOfUse)
                .WithCrabModification(CrabModification.Correction);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered)
                .When(importHouseNumberStatusFromCrab)
                .Then(addressId,
                    new AddressWasCorrectedToCurrent(addressId),
                    new AddressWasCorrectedToOfficiallyAssigned(addressId),
                    importHouseNumberStatusFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void ThenAddressWasCorrectedToCurrentIfStatusIsUnofficalAndCorrection(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            ImportHouseNumberStatusFromCrab importHouseNumberStatusFromCrab)
        {
            importHouseNumberStatusFromCrab = importHouseNumberStatusFromCrab
                .WithStatus(CrabAddressStatus.Unofficial)
                .WithCrabModification(CrabModification.Correction);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered)
                .When(importHouseNumberStatusFromCrab)
                .Then(addressId,
                    new AddressWasCorrectedToCurrent(addressId),
                    new AddressWasCorrectedToNotOfficiallyAssigned(addressId),
                    importHouseNumberStatusFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void ThenAddressWasCorrectedToRetiredIfLifetimeIsFiniteAndCorrection(
            Fixture fixture,
            AddressId addressId,
            ImportHouseNumberFromCrab importHouseNumberFromCrab)
        {
            importHouseNumberFromCrab = importHouseNumberFromCrab.
                WithLifetime(new Fixture().Customize(new NodaTimeCustomization()).Customize(new WithFiniteLifetime()).Create<CrabLifetime>())
                .WithCrabModification(CrabModification.Correction);

            Assert(RegisteredAddressScenario(fixture)
                .When(importHouseNumberFromCrab)
                .Then(addressId,
                    new AddressWasCorrectedToRetired(addressId),
                    importHouseNumberFromCrab.ToLegacyEvent()));
        }
    }
}
