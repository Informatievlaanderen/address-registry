namespace AddressRegistry.Tests.AggregateTests.Legacy.WhenImportHouseNumberStatusFromCrab
{
    using System;
    using Address.Commands.Crab;
    using Address.Events;
    using Address.Events.Crab;
    using Address.ValueObjects;
    using Address.ValueObjects.Crab;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using NodaTime;
    using WhenImportHousenumberStatusFromCrab;
    using Xunit;
    using Xunit.Abstractions;

    public partial class GivenAddress : AddressRegistryTest
    {
        public GivenAddress(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Theory]
        [DefaultData]
        public void WhenStatusIsProposed(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            ImportHouseNumberStatusFromCrab importHouseNumberStatusFromCrab)
        {
            importHouseNumberStatusFromCrab = importHouseNumberStatusFromCrab
                .WithStatus(CrabAddressStatus.Proposed);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered)
                .When(importHouseNumberStatusFromCrab)
                .Then(addressId,
                    new AddressWasProposed(addressId),
                    new AddressWasOfficiallyAssigned(addressId),
                    importHouseNumberStatusFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void GivenWasCorrectedToOfficiallyAssignedWhenStatusIsProposed(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            AddressWasCorrectedToOfficiallyAssigned addressWasCorrectedToOfficiallyAssigned,
            ImportHouseNumberStatusFromCrab importHouseNumberStatusFromCrab)
        {
            importHouseNumberStatusFromCrab = importHouseNumberStatusFromCrab
                .WithStatus(CrabAddressStatus.Proposed);

            Assert(new Scenario()
                .Given(addressId, addressWasRegistered)
                .Given(addressId, addressWasCorrectedToOfficiallyAssigned)
                .When(importHouseNumberStatusFromCrab)
                .Then(addressId,
                    new AddressWasProposed(addressId),
                    importHouseNumberStatusFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void WhenStatusIsReserved(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            ImportHouseNumberStatusFromCrab importHouseNumberStatusFromCrab)
        {
            importHouseNumberStatusFromCrab = importHouseNumberStatusFromCrab
                .WithStatus(CrabAddressStatus.Reserved);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered)
                .When(importHouseNumberStatusFromCrab)
                .Then(addressId,
                    new AddressWasProposed(addressId),
                    new AddressWasOfficiallyAssigned(addressId),
                    importHouseNumberStatusFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void WhenStatusIsInUse(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            ImportHouseNumberStatusFromCrab importHouseNumberStatusFromCrab)
        {
            importHouseNumberStatusFromCrab = importHouseNumberStatusFromCrab
                .WithStatus(CrabAddressStatus.InUse);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered)
                .When(importHouseNumberStatusFromCrab)
                .Then(addressId,
                    new AddressBecameCurrent(addressId),
                    new AddressWasOfficiallyAssigned(addressId),
                    importHouseNumberStatusFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void WhenStatusIsOutOfUse(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            ImportHouseNumberStatusFromCrab importHouseNumberStatusFromCrab)
        {
            importHouseNumberStatusFromCrab = importHouseNumberStatusFromCrab
                .WithStatus(CrabAddressStatus.OutOfUse);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered)
                .When(importHouseNumberStatusFromCrab)
                .Then(addressId,
                    new AddressBecameCurrent(addressId),
                    new AddressWasOfficiallyAssigned(addressId),
                    importHouseNumberStatusFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void WhenStatusIsUnoffical(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            ImportHouseNumberStatusFromCrab importHouseNumberStatusFromCrab)
        {
            importHouseNumberStatusFromCrab = importHouseNumberStatusFromCrab
                .WithStatus(CrabAddressStatus.Unofficial);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered)
                .When(importHouseNumberStatusFromCrab)
                .Then(addressId,
                    new AddressBecameCurrent(addressId),
                    new AddressBecameNotOfficiallyAssigned(addressId),
                    importHouseNumberStatusFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void GivenWasCorrectedToNotOffiallyAssignedWhenStatusIsUnoffical(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            AddressWasCorrectedToNotOfficiallyAssigned addressWasCorrectedToNotOfficiallyAssigned,
            ImportHouseNumberStatusFromCrab importHouseNumberStatusFromCrab)
        {
            importHouseNumberStatusFromCrab = importHouseNumberStatusFromCrab
                .WithStatus(CrabAddressStatus.Unofficial);

            Assert(new Scenario()
                .Given(addressId, addressWasRegistered)
                .Given(addressId, addressWasCorrectedToNotOfficiallyAssigned)
                .When(importHouseNumberStatusFromCrab)
                .Then(addressId,
                    new AddressBecameCurrent(addressId),
                    importHouseNumberStatusFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void WithStatusProposedWhenStatusIsTheSame(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            AddressWasProposed addressWasProposed,
            AddressWasOfficiallyAssigned addressWasOfficiallyAssigned,
            ImportHouseNumberStatusFromCrab importHouseNumberStatusFromCrab)
        {
            importHouseNumberStatusFromCrab = importHouseNumberStatusFromCrab
                .WithStatus(CrabAddressStatus.Proposed);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    addressWasProposed,
                    addressWasOfficiallyAssigned)
                .When(importHouseNumberStatusFromCrab)
                .Then(addressId,
                    importHouseNumberStatusFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void WithStatusProposedWhenModificationIsDelete(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            ImportHouseNumberStatusFromCrab importHouseNumberStatusFromCrab)
        {
            importHouseNumberStatusFromCrab = importHouseNumberStatusFromCrab
                .WithCrabModification(CrabModification.Delete)
                .WithStatus(CrabAddressStatus.Proposed);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered)
                .When(importHouseNumberStatusFromCrab)
                .Then(addressId,
                    importHouseNumberStatusFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void WithStatusCurrentWhenModificationIsDelete(AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            AddressBecameCurrent addressBecameCurrent,
            AddressWasOfficiallyAssigned addressWasOfficiallyAssigned,
            ImportHouseNumberStatusFromCrab importHouseNumberStatusFromCrab)
        {
            importHouseNumberStatusFromCrab = importHouseNumberStatusFromCrab
                .WithCrabModification(CrabModification.Delete);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    addressBecameCurrent,
                    addressWasOfficiallyAssigned
                )
                .When(importHouseNumberStatusFromCrab)
                .Then(addressId,
                    new AddressStatusWasRemoved(addressId),
                    new AddressOfficialAssignmentWasRemoved(addressId),
                    importHouseNumberStatusFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void WithStatusProposedWhenStatusIsInUseAndNewerLifetime(AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            AddressWasProposed addressWasProposed,
            AddressWasOfficiallyAssigned addressWasOfficiallyAssigned,
            AddressHouseNumberStatusWasImportedFromCrab addressHouseNumberStatusWasImportedFromCrab,
            ImportHouseNumberStatusFromCrab importHouseNumberStatusFromCrab)
        {
            addressHouseNumberStatusWasImportedFromCrab = addressHouseNumberStatusWasImportedFromCrab
                .WithStatus(CrabAddressStatus.Proposed);

            importHouseNumberStatusFromCrab = importHouseNumberStatusFromCrab
                .WithLifetime(new CrabLifetime(addressHouseNumberStatusWasImportedFromCrab.BeginDateTime.Value.PlusDays(1), null))
                .WithStatus(CrabAddressStatus.InUse);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    addressWasProposed,
                    addressWasOfficiallyAssigned,
                    addressHouseNumberStatusWasImportedFromCrab)
                .When(importHouseNumberStatusFromCrab)
                .Then(addressId,
                    new AddressBecameCurrent(addressId),
                    importHouseNumberStatusFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void WithStatusProposedWhenOlderLifetime(AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            AddressWasProposed addressWasProposed,
            AddressHouseNumberStatusWasImportedFromCrab addressHouseNumberStatusWasImportedFromCrab,
            AddressWasOfficiallyAssigned addressWasOfficiallyAssigned,
            ImportHouseNumberStatusFromCrab importHouseNumberStatusFromCrab)
        {
            addressHouseNumberStatusWasImportedFromCrab = addressHouseNumberStatusWasImportedFromCrab
                .WithStatus(CrabAddressStatus.Proposed);

            importHouseNumberStatusFromCrab = importHouseNumberStatusFromCrab
                .WithLifetime(new CrabLifetime(addressHouseNumberStatusWasImportedFromCrab.BeginDateTime.Value.PlusDays(-1), null))
                .WithStatus(CrabAddressStatus.InUse);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    addressWasProposed,
                    addressWasOfficiallyAssigned,
                    addressHouseNumberStatusWasImportedFromCrab
                )
                .When(importHouseNumberStatusFromCrab)
                .Then(addressId,
                    importHouseNumberStatusFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void WithRemovedStatusWhenStatusInUseAndSameLifetimeOfPreviouslyRemovedStatus(AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            AddressWasProposed addressWasProposed,
            AddressWasOfficiallyAssigned addressWasOfficiallyAssigned,
            AddressHouseNumberStatusWasImportedFromCrab addressHouseNumberStatusWasImportedFromCrab,
            AddressStatusWasRemoved addressStatusWasRemoved,
            AddressHouseNumberStatusWasImportedFromCrab addressHouseNumberStatusWasImportedFromCrabDelete,
            ImportHouseNumberStatusFromCrab importHouseNumberStatusFromCrab,
            CrabLifetime lifetime)
        {
            var statusId = new CrabHouseNumberStatusId(1);
            addressHouseNumberStatusWasImportedFromCrab = addressHouseNumberStatusWasImportedFromCrab
                .WithHouseNumberStatusId(statusId)
                .WithStatus(CrabAddressStatus.Proposed)
                .WithBeginDate(lifetime.BeginDateTime);
            addressHouseNumberStatusWasImportedFromCrabDelete = addressHouseNumberStatusWasImportedFromCrabDelete
                .WithHouseNumberStatusId(statusId)
                .WithStatus(CrabAddressStatus.Proposed)
                .WithCrabModification(CrabModification.Delete)
                .WithBeginDate(lifetime.BeginDateTime);

            importHouseNumberStatusFromCrab = importHouseNumberStatusFromCrab
                .WithLifetime(lifetime)
                .WithStatus(CrabAddressStatus.InUse);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    addressWasProposed,
                    addressWasOfficiallyAssigned,
                    addressHouseNumberStatusWasImportedFromCrab,
                    addressStatusWasRemoved,
                    addressHouseNumberStatusWasImportedFromCrabDelete
                )
                .When(importHouseNumberStatusFromCrab)
                .Then(addressId,
                    new AddressBecameCurrent(addressId),
                    importHouseNumberStatusFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void WithStatusDeleteWhenNewerStatusIsPresent(AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            AddressWasProposed addressWasProposed,
            AddressWasOfficiallyAssigned addressWasOfficiallyAssigned,
            AddressHouseNumberStatusWasImportedFromCrab addressHouseNumberStatusWasImportedFromCrabForProposed,
            AddressBecameCurrent addressBecameCurrent,
            AddressHouseNumberStatusWasImportedFromCrab addressHouseNumberStatusWasImportedFromCrabForCurrent,
            ImportHouseNumberStatusFromCrab importHouseNumberStatusFromCrabDeleteProposed)
        {
            var lifetime = new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null);
            var deleteProposed = importHouseNumberStatusFromCrabDeleteProposed
                .WithStatus(CrabAddressStatus.InUse)
                .WithLifetime(lifetime)
                .WithCrabModification(CrabModification.Delete)
                .WithStatusId(new CrabHouseNumberStatusId(addressHouseNumberStatusWasImportedFromCrabForProposed.HouseNumberStatusId));

            addressHouseNumberStatusWasImportedFromCrabForProposed = addressHouseNumberStatusWasImportedFromCrabForProposed
                .WithBeginDate(lifetime.BeginDateTime)
                .WithStatus(CrabAddressStatus.Proposed);
            addressHouseNumberStatusWasImportedFromCrabForCurrent = addressHouseNumberStatusWasImportedFromCrabForCurrent
                .WithBeginDate(lifetime.BeginDateTime)
                .WithStatus(CrabAddressStatus.InUse);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    addressWasProposed,
                    addressWasOfficiallyAssigned,
                    addressHouseNumberStatusWasImportedFromCrabForProposed,
                    addressBecameCurrent,
                    addressHouseNumberStatusWasImportedFromCrabForCurrent)
                .When(deleteProposed)
                .Then(addressId,
                    deleteProposed.ToLegacyEvent()));
        }
    }
}
