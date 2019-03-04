namespace AddressRegistry.Tests.WhenImportSubaddressPositionFromCrab
{
    using Address.Commands.Crab;
    using Address.Events;
    using Address.Events.Crab;
    using AddressRegistry.Tests.WhenImportSubaddressPositionFromCrab;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Crab;
    using global::AutoFixture;
    using NodaTime;
    using System;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddress : AddressRegistryTest
    {
        public GivenAddress(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }

        [Theory]
        [InlineDefaultDataForSubaddress(CrabAddressPositionOrigin.ManualIndicationFromLot, GeometryMethod.AppointedByAdministrator, GeometrySpecification.Lot)]
        [InlineDefaultDataForSubaddress(CrabAddressPositionOrigin.ManualIndicationFromParcel, GeometryMethod.AppointedByAdministrator, GeometrySpecification.Parcel)]
        [InlineDefaultDataForSubaddress(CrabAddressPositionOrigin.ManualIndicationFromBuilding, GeometryMethod.AppointedByAdministrator, GeometrySpecification.BuildingUnit)]
        [InlineDefaultDataForSubaddress(CrabAddressPositionOrigin.ManualIndicationFromMailbox, GeometryMethod.AppointedByAdministrator, GeometrySpecification.BuildingUnit)]
        [InlineDefaultDataForSubaddress(CrabAddressPositionOrigin.ManualIndicationFromUtilityConnection, GeometryMethod.AppointedByAdministrator, GeometrySpecification.BuildingUnit)]
        [InlineDefaultDataForSubaddress(CrabAddressPositionOrigin.ManualIndicationFromAccessToTheRoad, GeometryMethod.AppointedByAdministrator, GeometrySpecification.Parcel)]
        [InlineDefaultDataForSubaddress(CrabAddressPositionOrigin.ManualIndicationFromEntryOfBuilding, GeometryMethod.AppointedByAdministrator, GeometrySpecification.Entry)]
        [InlineDefaultDataForSubaddress(CrabAddressPositionOrigin.ManualIndicationFromStand, GeometryMethod.AppointedByAdministrator, GeometrySpecification.Stand)]
        [InlineDefaultDataForSubaddress(CrabAddressPositionOrigin.ManualIndicationFromBerth, GeometryMethod.AppointedByAdministrator, GeometrySpecification.Berth)]
        [InlineDefaultDataForSubaddress(CrabAddressPositionOrigin.DerivedFromBuilding, GeometryMethod.DerivedFromObject, GeometrySpecification.BuildingUnit)]
        [InlineDefaultDataForSubaddress(CrabAddressPositionOrigin.DerivedFromParcelGrb, GeometryMethod.DerivedFromObject, GeometrySpecification.Parcel)]
        [InlineDefaultDataForSubaddress(CrabAddressPositionOrigin.DerivedFromParcelCadastre, GeometryMethod.DerivedFromObject, GeometrySpecification.Parcel)]
        [InlineDefaultDataForSubaddress(CrabAddressPositionOrigin.InterpolatedBasedOnAdjacentHouseNumbersBuilding, GeometryMethod.Interpolated, GeometrySpecification.Parcel)]
        [InlineDefaultDataForSubaddress(CrabAddressPositionOrigin.InterpolatedBasedOnAdjacentHouseNumbersParcelGrb, GeometryMethod.Interpolated, GeometrySpecification.Parcel)]
        [InlineDefaultDataForSubaddress(CrabAddressPositionOrigin.InterpolatedBasedOnAdjacentHouseNumbersParcelCadastre, GeometryMethod.Interpolated, GeometrySpecification.Parcel)]
        [InlineDefaultDataForSubaddress(CrabAddressPositionOrigin.InterpolatedBasedOnRoadConnection, GeometryMethod.Interpolated, GeometrySpecification.RoadSegment)]
        [InlineDefaultDataForSubaddress(CrabAddressPositionOrigin.DerivedFromStreet, GeometryMethod.DerivedFromObject, GeometrySpecification.RoadSegment)]
        [InlineDefaultDataForSubaddress(CrabAddressPositionOrigin.DerivedFromMunicipality, GeometryMethod.DerivedFromObject, GeometrySpecification.Municipality)]
        public void WhenCrabAddressPositionOrigin(
            CrabAddressPositionOrigin crabAddressPositionOrigin,
            GeometryMethod geometryMethod,
            GeometrySpecification geometrySpecification,
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            ImportSubaddressPositionFromCrab importSubaddressPositionFromCrab)
        {
            importSubaddressPositionFromCrab = importSubaddressPositionFromCrab
                .WithCrabAddressPositionOrigin(crabAddressPositionOrigin);

            Assert(new Scenario()
                .Given(addressId, addressWasRegistered)
                .When(importSubaddressPositionFromCrab)
                .Then(addressId,
                    new AddressWasPositioned(addressId, new AddressGeometry(geometryMethod, geometrySpecification, GeometryHelpers.CreateEwkbFrom(importSubaddressPositionFromCrab.AddressPosition))),
                    importSubaddressPositionFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultDataForSubaddress]
        public void WhenModificationIsDelete(
            Fixture fixture,
            AddressId addressId,
            ImportSubaddressPositionFromCrab importSubaddressPositionFromCrab)
        {
            importSubaddressPositionFromCrab = importSubaddressPositionFromCrab
                .WithCrabModification(CrabModification.Delete);

            Assert(RegisteredAddressScenario(fixture)
                .When(importSubaddressPositionFromCrab)
                .Then(addressId,
                    importSubaddressPositionFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultDataForSubaddress]
        public void ThenNoPositionChangeWhenPositionIsTheSame(
            Fixture fixture,
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            WkbGeometry wkbGeometry,
            ImportSubaddressPositionFromCrab importSubaddressPositionFromCrab)
        {
            var addressWasPositioned = new AddressWasPositioned(addressId, new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Lot, GeometryHelpers.CreateEwkbFrom(wkbGeometry)));
            ((ISetProvenance)addressWasPositioned).SetProvenance(fixture.Create<Provenance>());

            importSubaddressPositionFromCrab = importSubaddressPositionFromCrab
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromLot)
                .WithWkbGeometry(wkbGeometry);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    addressWasPositioned)
                .When(importSubaddressPositionFromCrab)
                .Then(addressId,
                    importSubaddressPositionFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultDataForSubaddress]
        public void ThenAddressWasPositionedWhenNewerLifetimeAndHigherQuality(
            Fixture fixture,
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            WkbGeometry wkbGeometry,
            AddressSubaddressPositionWasImportedFromCrab addressSubaddressPositionWasImportedFromCrab,
            ImportSubaddressPositionFromCrab importSubaddressPositionFromCrab)
        {
            var addressWasPositioned = new AddressWasPositioned(addressId, new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Lot, GeometryHelpers.CreateEwkbFrom(wkbGeometry)));
            ((ISetProvenance)addressWasPositioned).SetProvenance(fixture.Create<Provenance>());

            addressSubaddressPositionWasImportedFromCrab = addressSubaddressPositionWasImportedFromCrab
                .WithWkbGeometry(wkbGeometry)
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromLot);

            importSubaddressPositionFromCrab = importSubaddressPositionFromCrab
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromStand)
                .WithLifetime(new CrabLifetime(addressSubaddressPositionWasImportedFromCrab.BeginDateTime.Value.PlusDays(1),
                    addressSubaddressPositionWasImportedFromCrab.EndDateTime));

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    addressWasPositioned,
                    addressSubaddressPositionWasImportedFromCrab)
                .When(importSubaddressPositionFromCrab)
                .Then(addressId,
                    new AddressWasPositioned(addressId,
                        new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Stand, GeometryHelpers.CreateEwkbFrom(importSubaddressPositionFromCrab.AddressPosition))),
                    importSubaddressPositionFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultDataForSubaddress]
        public void NoPositionedWhenNewerLifetimeAndLowerQuality(
            Fixture fixture,
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            WkbGeometry wkbGeometry,
            AddressSubaddressPositionWasImportedFromCrab addressSubaddressPositionWasImportedFromCrab,
            ImportSubaddressPositionFromCrab importSubaddressPositionFromCrab)
        {
            var addressWasPositioned = new AddressWasPositioned(addressId, new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Stand, GeometryHelpers.CreateEwkbFrom(wkbGeometry)));
            ((ISetProvenance)addressWasPositioned).SetProvenance(fixture.Create<Provenance>());

            addressSubaddressPositionWasImportedFromCrab = addressSubaddressPositionWasImportedFromCrab
                .WithWkbGeometry(wkbGeometry)
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromStand);

            importSubaddressPositionFromCrab = importSubaddressPositionFromCrab
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromLot)
                .WithLifetime(new CrabLifetime(addressSubaddressPositionWasImportedFromCrab.BeginDateTime.Value.PlusDays(1),
                    addressSubaddressPositionWasImportedFromCrab.EndDateTime));

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    addressWasPositioned,
                    addressSubaddressPositionWasImportedFromCrab)
                .When(importSubaddressPositionFromCrab)
                .Then(addressId,
                    importSubaddressPositionFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultDataForSubaddress]
        public void ThenNoPositionChangeWhenOlderLifetimeAndLessQuality(
            Fixture fixture,
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            WkbGeometry wkbGeometry,
            AddressSubaddressPositionWasImportedFromCrab addressSubaddressPositionWasImportedFromCrab,
            ImportSubaddressPositionFromCrab importSubaddressPositionFromCrab)
        {
            var addressWasPositioned = new AddressWasPositioned(addressId, new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Stand, GeometryHelpers.CreateEwkbFrom(wkbGeometry)));
            ((ISetProvenance)addressWasPositioned).SetProvenance(fixture.Create<Provenance>());

            addressSubaddressPositionWasImportedFromCrab = addressSubaddressPositionWasImportedFromCrab
                .WithWkbGeometry(wkbGeometry)
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromStand);

            importSubaddressPositionFromCrab = importSubaddressPositionFromCrab
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromLot)
                .WithLifetime(new CrabLifetime(addressSubaddressPositionWasImportedFromCrab.BeginDateTime.Value.PlusDays(-1),
                    addressSubaddressPositionWasImportedFromCrab.EndDateTime));

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    addressWasPositioned,
                    addressSubaddressPositionWasImportedFromCrab)
                .When(importSubaddressPositionFromCrab)
                .Then(addressId,
                    importSubaddressPositionFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultDataForSubaddress]
        public void ThenPositionChangeWhenOlderLifetimeAndHigherQuality(
            Fixture fixture,
            AddressId addressId,
            WkbGeometry wkbGeometry,
            CrabLifetime lifetime,
            ImportSubaddressPositionFromCrab importSubaddressPositionFromCrab)
        {
            var addressGeometry = new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Lot, GeometryHelpers.CreateEwkbFrom(wkbGeometry));

            importSubaddressPositionFromCrab = importSubaddressPositionFromCrab
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromStand)
                .WithLifetime(lifetime);

            Assert(RegisteredAddressScenario(fixture)
                .Given<AddressWasPositioned>(addressId, e => e.WithAddressGeometry(addressGeometry))
                .Given<AddressSubaddressPositionWasImportedFromCrab>(addressId, e => e
                    .WithWkbGeometry(wkbGeometry)
                    .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromLot)
                    .WithBeginDate(lifetime.BeginDateTime.Value.PlusDays(1)))
                .When(importSubaddressPositionFromCrab)
                .Then(addressId,
                    new AddressWasPositioned(addressId,
                        new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Stand, GeometryHelpers.CreateEwkbFrom(importSubaddressPositionFromCrab.AddressPosition))),
                    importSubaddressPositionFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultDataForSubaddress]
        public void WithRemovedPositionWhenSameLifetimeOfPreviouslyRemovedPosition(
            Fixture fixture,
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            WkbGeometry wkbGeometry,
            AddressSubaddressPositionWasImportedFromCrab addressSubaddressPositionWasImportedFromCrab,
            AddressPositionWasRemoved addressPositionWasRemoved,
            AddressSubaddressPositionWasImportedFromCrab addressSubaddressPositionWasImportedFromCrabDelete,
            ImportSubaddressPositionFromCrab importSubaddressPositionFromCrab,
            CrabLifetime lifetime)
        {
            var addressWasPositioned = new AddressWasPositioned(addressId, new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Lot, GeometryHelpers.CreateEwkbFrom(wkbGeometry)));
            ((ISetProvenance)addressWasPositioned).SetProvenance(fixture.Create<Provenance>());
            addressSubaddressPositionWasImportedFromCrab = addressSubaddressPositionWasImportedFromCrab
                .WithWkbGeometry(wkbGeometry)
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromLot)
                .WithBeginDate(lifetime.BeginDateTime);
            addressSubaddressPositionWasImportedFromCrabDelete = addressSubaddressPositionWasImportedFromCrabDelete
                .WithWkbGeometry(wkbGeometry)
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromLot)
                .WithBeginDate(lifetime.BeginDateTime)
                .WithModification(CrabModification.Delete);

            importSubaddressPositionFromCrab = importSubaddressPositionFromCrab
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromStand)
                .WithLifetime(lifetime);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    addressWasPositioned,
                    addressSubaddressPositionWasImportedFromCrab,
                    addressPositionWasRemoved,
                    addressSubaddressPositionWasImportedFromCrabDelete
                )
                .When(importSubaddressPositionFromCrab)
                .Then(addressId,
                    new AddressWasPositioned(addressId,
                        new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Stand, GeometryHelpers.CreateEwkbFrom(importSubaddressPositionFromCrab.AddressPosition))),
                    importSubaddressPositionFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultDataForSubaddress]
        public void WhenModificationIsCorrection(
            Fixture fixture,
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            WkbGeometry wkbGeometry,
            ImportSubaddressPositionFromCrab importSubaddressPositionFromCrab)
        {
            var addressWasPositioned = new AddressWasPositioned(addressId, new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Lot, GeometryHelpers.CreateEwkbFrom(wkbGeometry)));
            ((ISetProvenance)addressWasPositioned).SetProvenance(fixture.Create<Provenance>());

            importSubaddressPositionFromCrab = importSubaddressPositionFromCrab
                .WithCrabModification(CrabModification.Correction)
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.DerivedFromBuilding);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    addressWasPositioned
                )
                .When(importSubaddressPositionFromCrab)
                .Then(addressId,
                    new AddressPositionWasCorrected(addressId,
                        new AddressGeometry(GeometryMethod.DerivedFromObject, GeometrySpecification.BuildingUnit, GeometryHelpers.CreateEwkbFrom(importSubaddressPositionFromCrab.AddressPosition))),
                    importSubaddressPositionFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultDataForSubaddress]
        public void WhenPositionQualityIsHigher(
            Fixture fixture,
            AddressId addressId,
            CrabLifetime crabLifetime,
            AddressWasRegistered addressWasRegistered,
            AddressSubaddressPositionWasImportedFromCrab addressSubaddressPositionWasImported,
            ImportSubaddressPositionFromCrab importSubaddressPositionFromCrab)
        {
            var addressWasPositioned = new AddressWasPositioned(addressId,
                new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Lot, new ExtendedWkbGeometry(addressSubaddressPositionWasImported.AddressPosition)));
            ((ISetProvenance)addressWasPositioned).SetProvenance(fixture.Create<Provenance>());

            addressSubaddressPositionWasImported = addressSubaddressPositionWasImported
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromLot)
                .WithBeginDate(crabLifetime.BeginDateTime);

            importSubaddressPositionFromCrab = importSubaddressPositionFromCrab
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromEntryOfBuilding)
                .WithLifetime(crabLifetime);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    addressWasPositioned,
                    addressSubaddressPositionWasImported)
                .When(importSubaddressPositionFromCrab)
                .Then(addressId,
                    new AddressWasPositioned(addressId,
                        new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Entry, GeometryHelpers.CreateEwkbFrom(importSubaddressPositionFromCrab.AddressPosition))),
                    importSubaddressPositionFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultDataForSubaddress]
        public void WhenPositionQualityIsLower(
            Fixture fixture,
            AddressId addressId,
            CrabLifetime crabLifetime,
            AddressWasRegistered addressWasRegistered,
            AddressSubaddressPositionWasImportedFromCrab addressSubaddressPositionWasImported,
            ImportSubaddressPositionFromCrab importSubaddressPositionFromCrab)
        {
            var addressWasPositioned = new AddressWasPositioned(addressId,
                new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Lot, new ExtendedWkbGeometry(addressSubaddressPositionWasImported.AddressPosition)));
            ((ISetProvenance)addressWasPositioned).SetProvenance(fixture.Create<Provenance>());

            addressSubaddressPositionWasImported = addressSubaddressPositionWasImported
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromLot)
                .WithBeginDate(crabLifetime.BeginDateTime);

            importSubaddressPositionFromCrab = importSubaddressPositionFromCrab
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.DerivedFromParcelCadastre)
                .WithLifetime(crabLifetime);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    addressWasPositioned,
                    addressSubaddressPositionWasImported)
                .When(importSubaddressPositionFromCrab)
                .Then(addressId,
                    importSubaddressPositionFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultDataForSubaddress]
        public void When2PositionsAreCurrentAndHighestGetsDeleted(
            Fixture fixture,
            AddressId addressId,
            CrabLifetime crabLifetime,
            AddressWasRegistered addressWasRegistered,
            AddressSubaddressPositionWasImportedFromCrab addressSubaddressPositionWasImportedHigh,
            AddressSubaddressPositionWasImportedFromCrab addressSubaddressPositionWasImportedLow,
            ImportSubaddressPositionFromCrab importSubaddressPositionFromCrab)
        {
            var addressWasPositioned = new AddressWasPositioned(addressId,
                new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Lot,
                    new ExtendedWkbGeometry(addressSubaddressPositionWasImportedHigh.AddressPosition)));
            ((ISetProvenance)addressWasPositioned).SetProvenance(fixture.Create<Provenance>());

            addressSubaddressPositionWasImportedHigh = addressSubaddressPositionWasImportedHigh
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromLot)
                .WithBeginDate(crabLifetime.BeginDateTime);

            addressSubaddressPositionWasImportedLow = addressSubaddressPositionWasImportedLow
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.DerivedFromParcelCadastre)
                .WithBeginDate(crabLifetime.BeginDateTime);

            importSubaddressPositionFromCrab = importSubaddressPositionFromCrab
                .WithPositionId(new CrabAddressPositionId(addressSubaddressPositionWasImportedHigh.AddressPositionId))
                .WithCrabModification(CrabModification.Delete)
                .WithLifetime(crabLifetime);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    addressWasPositioned,
                    addressSubaddressPositionWasImportedHigh,
                    addressSubaddressPositionWasImportedLow)
                .When(importSubaddressPositionFromCrab)
                .Then(addressId,
                    new AddressWasPositioned(addressId,
                        new AddressGeometry(GeometryMethod.DerivedFromObject, GeometrySpecification.Parcel,
                            new ExtendedWkbGeometry(addressSubaddressPositionWasImportedLow.AddressPosition))),
                    importSubaddressPositionFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultDataForSubaddress]
        public void When2PositionsAreCurrentAndHighestGetsHistorized(
            Fixture fixture,
            AddressId addressId,
            CrabLifetime crabLifetime,
            AddressWasRegistered addressWasRegistered,
            AddressSubaddressPositionWasImportedFromCrab addressSubaddressPositionWasImportedHigh,
            AddressSubaddressPositionWasImportedFromCrab addressSubaddressPositionWasImportedLow,
            ImportSubaddressPositionFromCrab importSubaddressPositionFromCrab)
        {
            var addressWasPositioned = new AddressWasPositioned(addressId,
                new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Lot,
                    new ExtendedWkbGeometry(addressSubaddressPositionWasImportedHigh.AddressPosition)));
            ((ISetProvenance)addressWasPositioned).SetProvenance(fixture.Create<Provenance>());

            addressSubaddressPositionWasImportedHigh = addressSubaddressPositionWasImportedHigh
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromLot)
                .WithBeginDate(crabLifetime.BeginDateTime);

            addressSubaddressPositionWasImportedLow = addressSubaddressPositionWasImportedLow
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.DerivedFromParcelCadastre)
                .WithBeginDate(crabLifetime.BeginDateTime);

            importSubaddressPositionFromCrab = importSubaddressPositionFromCrab
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromLot)
                .WithPositionId(new CrabAddressPositionId(addressSubaddressPositionWasImportedHigh.AddressPositionId))
                .WithLifetime(new CrabLifetime(crabLifetime.BeginDateTime, LocalDateTime.FromDateTime(DateTime.Now)));

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    addressWasPositioned,
                    addressSubaddressPositionWasImportedHigh,
                    addressSubaddressPositionWasImportedLow)
                .When(importSubaddressPositionFromCrab)
                .Then(addressId,
                    new AddressWasPositioned(addressId,
                        new AddressGeometry(GeometryMethod.DerivedFromObject, GeometrySpecification.Parcel,
                            new ExtendedWkbGeometry(addressSubaddressPositionWasImportedLow.AddressPosition))),
                    importSubaddressPositionFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultDataForSubaddress]
        public void When2PositionsAreCurrentAndLowestGetsDeleted(
            Fixture fixture,
            AddressId addressId,
            CrabLifetime crabLifetime,
            AddressWasRegistered addressWasRegistered,
            AddressSubaddressPositionWasImportedFromCrab addressSubaddressPositionWasImportedHigh,
            AddressSubaddressPositionWasImportedFromCrab addressSubaddressPositionWasImportedLow,
            ImportSubaddressPositionFromCrab importSubaddressPositionFromCrab)
        {
            var addressWasPositioned = new AddressWasPositioned(addressId,
                new AddressGeometry(GeometryMethod.DerivedFromObject, GeometrySpecification.Parcel, new ExtendedWkbGeometry(addressSubaddressPositionWasImportedHigh.AddressPosition)));
            ((ISetProvenance)addressWasPositioned).SetProvenance(fixture.Create<Provenance>());
            var addressWasPositioned2 = new AddressWasPositioned(addressId,
                new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Lot,
                    new ExtendedWkbGeometry(addressSubaddressPositionWasImportedHigh.AddressPosition)));
            ((ISetProvenance)addressWasPositioned2).SetProvenance(fixture.Create<Provenance>());

            addressSubaddressPositionWasImportedLow = addressSubaddressPositionWasImportedLow
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.DerivedFromParcelCadastre)
                .WithBeginDate(crabLifetime.BeginDateTime);

            addressSubaddressPositionWasImportedHigh = addressSubaddressPositionWasImportedHigh
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromLot)
                .WithBeginDate(crabLifetime.BeginDateTime);

            importSubaddressPositionFromCrab = importSubaddressPositionFromCrab
                .WithPositionId(new CrabAddressPositionId(addressSubaddressPositionWasImportedLow.AddressPositionId))
                .WithCrabModification(CrabModification.Delete)
                .WithLifetime(crabLifetime);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    addressWasPositioned,
                    addressSubaddressPositionWasImportedLow,
                    addressWasPositioned2,
                    addressSubaddressPositionWasImportedHigh)
                .When(importSubaddressPositionFromCrab)
                .Then(addressId,
                    importSubaddressPositionFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultDataForSubaddress]
        public void When2PositionsAreCurrentAndLowestGetsHistorized(
            Fixture fixture,
            AddressId addressId,
            CrabLifetime crabLifetime,
            AddressWasRegistered addressWasRegistered,
            AddressSubaddressPositionWasImportedFromCrab addressSubaddressPositionWasImportedHigh,
            AddressSubaddressPositionWasImportedFromCrab addressSubaddressPositionWasImportedLow,
            ImportSubaddressPositionFromCrab importSubaddressPositionFromCrab)
        {
            var addressWasPositioned = new AddressWasPositioned(addressId,
                new AddressGeometry(GeometryMethod.DerivedFromObject, GeometrySpecification.Parcel, new ExtendedWkbGeometry(addressSubaddressPositionWasImportedHigh.AddressPosition)));
            ((ISetProvenance)addressWasPositioned).SetProvenance(fixture.Create<Provenance>());
            var addressWasPositioned2 = new AddressWasPositioned(addressId,
                new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Lot,
                    new ExtendedWkbGeometry(addressSubaddressPositionWasImportedHigh.AddressPosition)));
            ((ISetProvenance)addressWasPositioned2).SetProvenance(fixture.Create<Provenance>());

            addressSubaddressPositionWasImportedLow = addressSubaddressPositionWasImportedLow
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.DerivedFromParcelCadastre)
                .WithBeginDate(crabLifetime.BeginDateTime);

            addressSubaddressPositionWasImportedHigh = addressSubaddressPositionWasImportedHigh
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromLot)
                .WithBeginDate(crabLifetime.BeginDateTime);

            importSubaddressPositionFromCrab = importSubaddressPositionFromCrab
                .WithPositionId(new CrabAddressPositionId(addressSubaddressPositionWasImportedLow.AddressPositionId))
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.DerivedFromParcelCadastre)
                .WithLifetime(new CrabLifetime(crabLifetime.BeginDateTime, LocalDateTime.FromDateTime(DateTime.Now)));

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    addressWasPositioned,
                    addressSubaddressPositionWasImportedLow,
                    addressWasPositioned2,
                    addressSubaddressPositionWasImportedHigh)
                .When(importSubaddressPositionFromCrab)
                .Then(addressId,
                    importSubaddressPositionFromCrab.ToLegacyEvent()));
        }
    }
}
