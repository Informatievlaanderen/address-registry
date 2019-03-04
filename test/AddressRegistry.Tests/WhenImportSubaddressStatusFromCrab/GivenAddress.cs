namespace AddressRegistry.Tests.WhenImportSubaddressStatusFromCrab
{
    using Address.Commands.Crab;
    using Address.Events;
    using Address.Events.Crab;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Crab;
    using NodaTime;
    using System;
    using Xunit;
    using Xunit.Abstractions;

    public partial class GivenAddress : AddressRegistryTest
    {
        public GivenAddress(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Theory]
        [DefaultDataForSubaddress]
        public void WhenStatusIsProposed(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            ImportSubaddressStatusFromCrab importSubaddressStatusFromCrab)
        {
            importSubaddressStatusFromCrab = importSubaddressStatusFromCrab
                .WithStatus(CrabAddressStatus.Proposed);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered)
                .When(importSubaddressStatusFromCrab)
                .Then(addressId,
                    new AddressWasProposed(addressId),
                    new AddressWasOfficiallyAssigned(addressId),
                    importSubaddressStatusFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultDataForSubaddress]
        public void GivenWasCorrectedToOfficiallyAssignedWhenStatusIsProposed(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            AddressWasCorrectedToOfficiallyAssigned addressWasCorrectedToOfficiallyAssigned,
            ImportSubaddressStatusFromCrab importSubaddressStatusFromCrab)
        {
            importSubaddressStatusFromCrab = importSubaddressStatusFromCrab
                .WithStatus(CrabAddressStatus.Proposed);

            Assert(new Scenario()
                .Given(addressId, addressWasRegistered)
                .Given(addressId, addressWasCorrectedToOfficiallyAssigned)
                .When(importSubaddressStatusFromCrab)
                .Then(addressId,
                    new AddressWasProposed(addressId),
                    importSubaddressStatusFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultDataForSubaddress]
        public void WhenStatusIsReserved(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            ImportSubaddressStatusFromCrab importSubaddressStatusFromCrab)
        {
            importSubaddressStatusFromCrab = importSubaddressStatusFromCrab
                .WithStatus(CrabAddressStatus.Reserved);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered)
                .When(importSubaddressStatusFromCrab)
                .Then(addressId,
                    new AddressWasProposed(addressId),
                    new AddressWasOfficiallyAssigned(addressId),
                    importSubaddressStatusFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultDataForSubaddress]
        public void WhenStatusIsInUse(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            ImportSubaddressStatusFromCrab importSubaddressStatusFromCrab)
        {
            importSubaddressStatusFromCrab = importSubaddressStatusFromCrab
                .WithStatus(CrabAddressStatus.InUse);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered)
                .When(importSubaddressStatusFromCrab)
                .Then(addressId,
                    new AddressBecameCurrent(addressId),
                    new AddressWasOfficiallyAssigned(addressId),
                    importSubaddressStatusFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultDataForSubaddress]
        public void WhenStatusIsOutOfUse(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            ImportSubaddressStatusFromCrab importSubaddressStatusFromCrab)
        {
            importSubaddressStatusFromCrab = importSubaddressStatusFromCrab
                .WithStatus(CrabAddressStatus.OutOfUse);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered)
                .When(importSubaddressStatusFromCrab)
                .Then(addressId,
                    new AddressBecameCurrent(addressId),
                    new AddressWasOfficiallyAssigned(addressId),
                    importSubaddressStatusFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultDataForSubaddress]
        public void WhenStatusIsUnoffical(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            ImportSubaddressStatusFromCrab importSubaddressStatusFromCrab)
        {
            importSubaddressStatusFromCrab = importSubaddressStatusFromCrab
                .WithStatus(CrabAddressStatus.Unofficial);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered)
                .When(importSubaddressStatusFromCrab)
                .Then(addressId,
                    new AddressBecameCurrent(addressId),
                    new AddressBecameNotOfficiallyAssigned(addressId),
                    importSubaddressStatusFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultDataForSubaddress]
        public void GivenWasCorrectedToNotOffiallyAssignedWhenStatusIsUnoffical(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            AddressWasCorrectedToNotOfficiallyAssigned addressWasCorrectedToNotOfficiallyAssigned,
            ImportSubaddressStatusFromCrab importSubaddressStatusFromCrab)
        {
            importSubaddressStatusFromCrab = importSubaddressStatusFromCrab
                .WithStatus(CrabAddressStatus.Unofficial);

            Assert(new Scenario()
                .Given(addressId, addressWasRegistered)
                .Given(addressId, addressWasCorrectedToNotOfficiallyAssigned)
                .When(importSubaddressStatusFromCrab)
                .Then(addressId,
                    new AddressBecameCurrent(addressId),
                    importSubaddressStatusFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultDataForSubaddress]
        public void WithStatusProposedWhenStatusIsTheSame(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            AddressWasProposed addressWasProposed,
            AddressWasOfficiallyAssigned addressWasOfficiallyAssigned,
            ImportSubaddressStatusFromCrab importSubaddressStatusFromCrab)
        {
            importSubaddressStatusFromCrab = importSubaddressStatusFromCrab
                .WithStatus(CrabAddressStatus.Proposed);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    addressWasProposed,
                    addressWasOfficiallyAssigned)
                .When(importSubaddressStatusFromCrab)
                .Then(addressId,
                    importSubaddressStatusFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultDataForSubaddress]
        public void WithStatusProposedWhenModificationIsDelete(
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            ImportSubaddressStatusFromCrab importSubaddressStatusFromCrab)
        {
            importSubaddressStatusFromCrab = importSubaddressStatusFromCrab
                .WithCrabModification(CrabModification.Delete)
                .WithStatus(CrabAddressStatus.Proposed);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered)
                .When(importSubaddressStatusFromCrab)
                .Then(addressId,
                    importSubaddressStatusFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultDataForSubaddress]
        public void WithStatusCurrentWhenModificationIsDelete(AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            AddressBecameCurrent addressBecameCurrent,
            AddressWasOfficiallyAssigned addressWasOfficiallyAssigned,
            ImportSubaddressStatusFromCrab importSubaddressStatusFromCrab)
        {
            importSubaddressStatusFromCrab = importSubaddressStatusFromCrab
                .WithCrabModification(CrabModification.Delete);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    addressBecameCurrent,
                    addressWasOfficiallyAssigned
                )
                .When(importSubaddressStatusFromCrab)
                .Then(addressId,
                    new AddressStatusWasRemoved(addressId),
                    new AddressOfficialAssignmentWasRemoved(addressId),
                    importSubaddressStatusFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultDataForSubaddress]
        public void WithStatusProposedWhenStatusIsInUseAndNewerLifetime(AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            AddressWasProposed addressWasProposed,
            AddressWasOfficiallyAssigned addressWasOfficiallyAssigned,
            AddressSubaddressStatusWasImportedFromCrab addressSubaddressStatusWasImportedFromCrab,
            ImportSubaddressStatusFromCrab importSubaddressStatusFromCrab)
        {
            addressSubaddressStatusWasImportedFromCrab = addressSubaddressStatusWasImportedFromCrab
                .WithStatus(CrabAddressStatus.Proposed);

            importSubaddressStatusFromCrab = importSubaddressStatusFromCrab
                .WithLifetime(new CrabLifetime(addressSubaddressStatusWasImportedFromCrab.BeginDateTime.Value.PlusDays(1), null))
                .WithStatus(CrabAddressStatus.InUse);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    addressWasProposed,
                    addressWasOfficiallyAssigned,
                    addressSubaddressStatusWasImportedFromCrab)
                .When(importSubaddressStatusFromCrab)
                .Then(addressId,
                    new AddressBecameCurrent(addressId),
                    importSubaddressStatusFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultDataForSubaddress]
        public void WithStatusProposedWhenOlderLifetime(AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            AddressWasProposed addressWasProposed,
            AddressSubaddressStatusWasImportedFromCrab addressSubaddressStatusWasImportedFromCrab,
            AddressWasOfficiallyAssigned addressWasOfficiallyAssigned,
            ImportSubaddressStatusFromCrab importSubaddressStatusFromCrab)
        {
            addressSubaddressStatusWasImportedFromCrab = addressSubaddressStatusWasImportedFromCrab
                .WithStatus(CrabAddressStatus.Proposed);

            importSubaddressStatusFromCrab = importSubaddressStatusFromCrab
                .WithLifetime(new CrabLifetime(addressSubaddressStatusWasImportedFromCrab.BeginDateTime.Value.PlusDays(-1), null))
                .WithStatus(CrabAddressStatus.InUse);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    addressWasProposed,
                    addressWasOfficiallyAssigned,
                    addressSubaddressStatusWasImportedFromCrab
                )
                .When(importSubaddressStatusFromCrab)
                .Then(addressId,
                    importSubaddressStatusFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultDataForSubaddress]
        public void WithRemovedStatusWhenStatusInUseAndSameLifetimeOfPreviouslyRemovedStatus(AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            AddressWasProposed addressWasProposed,
            AddressWasOfficiallyAssigned addressWasOfficiallyAssigned,
            AddressSubaddressStatusWasImportedFromCrab addressSubaddressStatusWasImportedFromCrab,
            AddressStatusWasRemoved addressStatusWasRemoved,
            AddressSubaddressStatusWasImportedFromCrab addressSubaddressStatusWasImportedFromCrabDelete,
            ImportSubaddressStatusFromCrab importSubaddressStatusFromCrab,
            CrabLifetime lifetime)
        {
            var statusId = new CrabSubaddressStatusId(1);
            addressSubaddressStatusWasImportedFromCrab = addressSubaddressStatusWasImportedFromCrab
                .WithSubaddressStatusId(statusId)
                .WithStatus(CrabAddressStatus.Proposed)
                .WithBeginDate(lifetime.BeginDateTime);
            addressSubaddressStatusWasImportedFromCrabDelete = addressSubaddressStatusWasImportedFromCrabDelete
                .WithSubaddressStatusId(statusId)
                .WithStatus(CrabAddressStatus.Proposed)
                .WithCrabModification(CrabModification.Delete)
                .WithBeginDate(lifetime.BeginDateTime);

            importSubaddressStatusFromCrab = importSubaddressStatusFromCrab
                .WithLifetime(lifetime)
                .WithStatus(CrabAddressStatus.InUse);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    addressWasProposed,
                    addressWasOfficiallyAssigned,
                    addressSubaddressStatusWasImportedFromCrab,
                    addressStatusWasRemoved,
                    addressSubaddressStatusWasImportedFromCrabDelete
                )
                .When(importSubaddressStatusFromCrab)
                .Then(addressId,
                    new AddressBecameCurrent(addressId),
                    importSubaddressStatusFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultDataForSubaddress]
        public void WithStatusDeleteWhenNewerStatusIsPresent(AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            AddressWasProposed addressWasProposed,
            AddressWasOfficiallyAssigned addressWasOfficiallyAssigned,
            AddressSubaddressStatusWasImportedFromCrab addressSubaddressStatusWasImportedFromCrabForProposed,
            AddressBecameCurrent addressBecameCurrent,
            AddressSubaddressStatusWasImportedFromCrab addressSubaddressStatusWasImportedFromCrabForCurrent,
            ImportSubaddressStatusFromCrab importSubaddressStatusFromCrabDeleteProposed)
        {
            var lifetime = new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null);
            var deleteProposed = importSubaddressStatusFromCrabDeleteProposed
                .WithStatus(CrabAddressStatus.InUse)
                .WithLifetime(lifetime)
                .WithCrabModification(CrabModification.Delete)
                .WithStatusId(new CrabSubaddressStatusId(addressSubaddressStatusWasImportedFromCrabForProposed.SubaddressStatusId));

            addressSubaddressStatusWasImportedFromCrabForProposed = addressSubaddressStatusWasImportedFromCrabForProposed
                .WithBeginDate(lifetime.BeginDateTime)
                .WithStatus(CrabAddressStatus.Proposed);
            addressSubaddressStatusWasImportedFromCrabForCurrent = addressSubaddressStatusWasImportedFromCrabForCurrent
                .WithBeginDate(lifetime.BeginDateTime)
                .WithStatus(CrabAddressStatus.InUse);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    addressWasProposed,
                    addressWasOfficiallyAssigned,
                    addressSubaddressStatusWasImportedFromCrabForProposed,
                    addressBecameCurrent,
                    addressSubaddressStatusWasImportedFromCrabForCurrent)
                .When(deleteProposed)
                .Then(addressId,
                    deleteProposed.ToLegacyEvent()));
        }
    }
}
