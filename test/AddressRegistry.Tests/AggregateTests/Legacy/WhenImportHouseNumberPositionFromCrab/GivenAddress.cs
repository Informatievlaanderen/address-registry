namespace AddressRegistry.Tests.AggregateTests.Legacy.WhenImportHouseNumberPositionFromCrab
{
    using System;
    using Address;
    using Address.Commands.Crab;
    using Address.Crab;
    using Address.Events;
    using Address.Events.Crab;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using EventExtensions;
    using global::AutoFixture;
    using NodaTime;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddress : AddressRegistryTest
    {
        public GivenAddress(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }

        [Theory]
        [InlineDefaultData(CrabAddressPositionOrigin.ManualIndicationFromLot, GeometryMethod.AppointedByAdministrator, GeometrySpecification.Lot)]
        [InlineDefaultData(CrabAddressPositionOrigin.ManualIndicationFromParcel, GeometryMethod.AppointedByAdministrator, GeometrySpecification.Parcel)]
        [InlineDefaultData(CrabAddressPositionOrigin.ManualIndicationFromBuilding, GeometryMethod.AppointedByAdministrator, GeometrySpecification.BuildingUnit)]
        [InlineDefaultData(CrabAddressPositionOrigin.ManualIndicationFromMailbox, GeometryMethod.AppointedByAdministrator, GeometrySpecification.BuildingUnit)]
        [InlineDefaultData(CrabAddressPositionOrigin.ManualIndicationFromUtilityConnection, GeometryMethod.AppointedByAdministrator, GeometrySpecification.BuildingUnit)]
        [InlineDefaultData(CrabAddressPositionOrigin.ManualIndicationFromAccessToTheRoad, GeometryMethod.AppointedByAdministrator, GeometrySpecification.Parcel)]
        [InlineDefaultData(CrabAddressPositionOrigin.ManualIndicationFromEntryOfBuilding, GeometryMethod.AppointedByAdministrator, GeometrySpecification.Entry)]
        [InlineDefaultData(CrabAddressPositionOrigin.ManualIndicationFromStand, GeometryMethod.AppointedByAdministrator, GeometrySpecification.Stand)]
        [InlineDefaultData(CrabAddressPositionOrigin.ManualIndicationFromBerth, GeometryMethod.AppointedByAdministrator, GeometrySpecification.Berth)]
        [InlineDefaultData(CrabAddressPositionOrigin.DerivedFromBuilding, GeometryMethod.DerivedFromObject, GeometrySpecification.BuildingUnit)]
        [InlineDefaultData(CrabAddressPositionOrigin.DerivedFromParcelGrb, GeometryMethod.DerivedFromObject, GeometrySpecification.Parcel)]
        [InlineDefaultData(CrabAddressPositionOrigin.DerivedFromParcelCadastre, GeometryMethod.DerivedFromObject, GeometrySpecification.Parcel)]
        [InlineDefaultData(CrabAddressPositionOrigin.InterpolatedBasedOnAdjacentHouseNumbersBuilding, GeometryMethod.Interpolated, GeometrySpecification.Parcel)]
        [InlineDefaultData(CrabAddressPositionOrigin.InterpolatedBasedOnAdjacentHouseNumbersParcelGrb, GeometryMethod.Interpolated, GeometrySpecification.Parcel)]
        [InlineDefaultData(CrabAddressPositionOrigin.InterpolatedBasedOnAdjacentHouseNumbersParcelCadastre, GeometryMethod.Interpolated, GeometrySpecification.Parcel)]
        [InlineDefaultData(CrabAddressPositionOrigin.InterpolatedBasedOnRoadConnection, GeometryMethod.Interpolated, GeometrySpecification.RoadSegment)]
        [InlineDefaultData(CrabAddressPositionOrigin.DerivedFromStreet, GeometryMethod.DerivedFromObject, GeometrySpecification.RoadSegment)]
        [InlineDefaultData(CrabAddressPositionOrigin.DerivedFromMunicipality, GeometryMethod.DerivedFromObject, GeometrySpecification.Municipality)]
        public void WhenCrabAddressPositionOrigin(
            CrabAddressPositionOrigin crabAddressPositionOrigin,
            GeometryMethod geometryMethod,
            GeometrySpecification geometrySpecification,
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            ImportHouseNumberPositionFromCrab importHouseNumberPositionFromCrab)
        {
            importHouseNumberPositionFromCrab = importHouseNumberPositionFromCrab
                .WithCrabAddressPositionOrigin(crabAddressPositionOrigin);

            Assert(new Scenario()
                .Given(addressId, addressWasRegistered)
                .When(importHouseNumberPositionFromCrab)
                .Then(addressId,
                    new AddressWasPositioned(addressId, new AddressGeometry(geometryMethod, geometrySpecification, GeometryHelpers.CreateEwkbFrom(importHouseNumberPositionFromCrab.AddressPosition))),
                    importHouseNumberPositionFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void WhenModificationIsDelete(
            Fixture fixture,
            AddressId addressId,
            ImportHouseNumberPositionFromCrab importHouseNumberPositionFromCrab)
        {
            importHouseNumberPositionFromCrab = importHouseNumberPositionFromCrab
                .WithCrabModification(CrabModification.Delete);

            Assert(RegisteredAddressScenario(fixture)
                .When(importHouseNumberPositionFromCrab)
                .Then(addressId,
                    importHouseNumberPositionFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void ThenNoPositionChangeWhenPositionIsTheSame(
            Fixture fixture,
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            WkbGeometry wkbGeometry,
            ImportHouseNumberPositionFromCrab importHouseNumberPositionFromCrab)
        {
            var addressWasPositioned = new AddressWasPositioned(addressId, new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Lot, GeometryHelpers.CreateEwkbFrom(wkbGeometry)));
            ((ISetProvenance)addressWasPositioned).SetProvenance(fixture.Create<Provenance>());

            importHouseNumberPositionFromCrab = importHouseNumberPositionFromCrab
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromLot)
                .WithWkbGeometry(wkbGeometry);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    addressWasPositioned)
                .When(importHouseNumberPositionFromCrab)
                .Then(addressId,
                    importHouseNumberPositionFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void ThenAddressWasPositionedWhenNewerLifetimeAndHigherQuality(
            Fixture fixture,
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            WkbGeometry wkbGeometry,
            AddressHouseNumberPositionWasImportedFromCrab addressHouseNumberPositionWasImportedFromCrab,
            ImportHouseNumberPositionFromCrab importHouseNumberPositionFromCrab)
        {
            var addressWasPositioned = new AddressWasPositioned(addressId, new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Lot, GeometryHelpers.CreateEwkbFrom(wkbGeometry)));
            ((ISetProvenance)addressWasPositioned).SetProvenance(fixture.Create<Provenance>());

            addressHouseNumberPositionWasImportedFromCrab = addressHouseNumberPositionWasImportedFromCrab
                .WithWkbGeometry(wkbGeometry)
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromLot);

            importHouseNumberPositionFromCrab = importHouseNumberPositionFromCrab
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromStand)
                .WithLifetime(new CrabLifetime(addressHouseNumberPositionWasImportedFromCrab.BeginDateTime.Value.PlusDays(1),
                    addressHouseNumberPositionWasImportedFromCrab.EndDateTime));

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    addressWasPositioned,
                    addressHouseNumberPositionWasImportedFromCrab)
                .When(importHouseNumberPositionFromCrab)
                .Then(addressId,
                    new AddressWasPositioned(addressId,
                        new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Stand, GeometryHelpers.CreateEwkbFrom(importHouseNumberPositionFromCrab.AddressPosition))),
                    importHouseNumberPositionFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void NoPositionedWhenNewerLifetimeAndLowerQuality(
            Fixture fixture,
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            WkbGeometry wkbGeometry,
            AddressHouseNumberPositionWasImportedFromCrab addressHouseNumberPositionWasImportedFromCrab,
            ImportHouseNumberPositionFromCrab importHouseNumberPositionFromCrab)
        {
            var addressWasPositioned = new AddressWasPositioned(addressId, new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Stand, GeometryHelpers.CreateEwkbFrom(wkbGeometry)));
            ((ISetProvenance)addressWasPositioned).SetProvenance(fixture.Create<Provenance>());

            addressHouseNumberPositionWasImportedFromCrab = addressHouseNumberPositionWasImportedFromCrab
                .WithWkbGeometry(wkbGeometry)
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromStand);

            importHouseNumberPositionFromCrab = importHouseNumberPositionFromCrab
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromLot)
                .WithLifetime(new CrabLifetime(addressHouseNumberPositionWasImportedFromCrab.BeginDateTime.Value.PlusDays(1),
                    addressHouseNumberPositionWasImportedFromCrab.EndDateTime));

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    addressWasPositioned,
                    addressHouseNumberPositionWasImportedFromCrab)
                .When(importHouseNumberPositionFromCrab)
                .Then(addressId,
                    importHouseNumberPositionFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void ThenNoPositionChangeWhenOlderLifetimeAndLessQuality(
            Fixture fixture,
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            WkbGeometry wkbGeometry,
            AddressHouseNumberPositionWasImportedFromCrab addressHouseNumberPositionWasImportedFromCrab,
            ImportHouseNumberPositionFromCrab importHouseNumberPositionFromCrab)
        {
            var addressWasPositioned = new AddressWasPositioned(addressId, new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Stand, GeometryHelpers.CreateEwkbFrom(wkbGeometry)));
            ((ISetProvenance)addressWasPositioned).SetProvenance(fixture.Create<Provenance>());

            addressHouseNumberPositionWasImportedFromCrab = addressHouseNumberPositionWasImportedFromCrab
                .WithWkbGeometry(wkbGeometry)
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromStand);

            importHouseNumberPositionFromCrab = importHouseNumberPositionFromCrab
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromLot)
                .WithLifetime(new CrabLifetime(addressHouseNumberPositionWasImportedFromCrab.BeginDateTime.Value.PlusDays(-1),
                    addressHouseNumberPositionWasImportedFromCrab.EndDateTime));

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    addressWasPositioned,
                    addressHouseNumberPositionWasImportedFromCrab)
                .When(importHouseNumberPositionFromCrab)
                .Then(addressId,
                    importHouseNumberPositionFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void ThenPositionChangeWhenOlderLifetimeAndHigherQuality(
            Fixture fixture,
            AddressId addressId,
            WkbGeometry wkbGeometry,
            CrabLifetime lifetime,
            ImportHouseNumberPositionFromCrab importHouseNumberPositionFromCrab)
        {
            var addressGeometry = new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Lot, GeometryHelpers.CreateEwkbFrom(wkbGeometry));

            importHouseNumberPositionFromCrab = importHouseNumberPositionFromCrab
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromStand)
                .WithLifetime(lifetime);

            Assert(RegisteredAddressScenario(fixture)
                .Given<AddressWasPositioned>(addressId, e => e.WithAddressGeometry(addressGeometry))
                .Given<AddressHouseNumberPositionWasImportedFromCrab>(addressId, e => e
                    .WithWkbGeometry(wkbGeometry)
                    .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromLot)
                    .WithBeginDate(lifetime.BeginDateTime.Value.PlusDays(1)))
                .When(importHouseNumberPositionFromCrab)
                .Then(addressId,
                    new AddressWasPositioned(addressId,
                        new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Stand, GeometryHelpers.CreateEwkbFrom(importHouseNumberPositionFromCrab.AddressPosition))),
                    importHouseNumberPositionFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void WithRemovedPositionWhenSameLifetimeOfPreviouslyRemovedPosition(
            Fixture fixture,
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            WkbGeometry wkbGeometry,
            AddressHouseNumberPositionWasImportedFromCrab addressHouseNumberPositionWasImportedFromCrab,
            AddressPositionWasRemoved addressPositionWasRemoved,
            AddressHouseNumberPositionWasImportedFromCrab addressHouseNumberPositionWasImportedFromCrabDelete,
            ImportHouseNumberPositionFromCrab importHouseNumberPositionFromCrab,
            CrabLifetime lifetime)
        {
            var addressWasPositioned = new AddressWasPositioned(addressId, new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Lot, GeometryHelpers.CreateEwkbFrom(wkbGeometry)));
            ((ISetProvenance)addressWasPositioned).SetProvenance(fixture.Create<Provenance>());
            addressHouseNumberPositionWasImportedFromCrab = addressHouseNumberPositionWasImportedFromCrab
                .WithWkbGeometry(wkbGeometry)
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromLot)
                .WithBeginDate(lifetime.BeginDateTime);
            addressHouseNumberPositionWasImportedFromCrabDelete = addressHouseNumberPositionWasImportedFromCrabDelete
                .WithWkbGeometry(wkbGeometry)
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromLot)
                .WithBeginDate(lifetime.BeginDateTime)
                .WithModification(CrabModification.Delete);

            importHouseNumberPositionFromCrab = importHouseNumberPositionFromCrab
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromStand)
                .WithLifetime(lifetime);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    addressWasPositioned,
                    addressHouseNumberPositionWasImportedFromCrab,
                    addressPositionWasRemoved,
                    addressHouseNumberPositionWasImportedFromCrabDelete
                )
                .When(importHouseNumberPositionFromCrab)
                .Then(addressId,
                    new AddressWasPositioned(addressId,
                        new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Stand, GeometryHelpers.CreateEwkbFrom(importHouseNumberPositionFromCrab.AddressPosition))),
                    importHouseNumberPositionFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void WhenModificationIsCorrection(
            Fixture fixture,
            AddressId addressId,
            AddressWasRegistered addressWasRegistered,
            WkbGeometry wkbGeometry,
            ImportHouseNumberPositionFromCrab importHouseNumberPositionFromCrab)
        {
            var addressWasPositioned = new AddressWasPositioned(addressId, new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Lot, GeometryHelpers.CreateEwkbFrom(wkbGeometry)));
            ((ISetProvenance)addressWasPositioned).SetProvenance(fixture.Create<Provenance>());

            importHouseNumberPositionFromCrab = importHouseNumberPositionFromCrab
                .WithCrabModification(CrabModification.Correction)
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.DerivedFromBuilding);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    addressWasPositioned
                )
                .When(importHouseNumberPositionFromCrab)
                .Then(addressId,
                    new AddressPositionWasCorrected(addressId,
                        new AddressGeometry(GeometryMethod.DerivedFromObject, GeometrySpecification.BuildingUnit, GeometryHelpers.CreateEwkbFrom(importHouseNumberPositionFromCrab.AddressPosition))),
                    importHouseNumberPositionFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void WhenPositionQualityIsHigher(
            Fixture fixture,
            AddressId addressId,
            CrabLifetime crabLifetime,
            AddressWasRegistered addressWasRegistered,
            AddressHouseNumberPositionWasImportedFromCrab addressHouseNumberPositionWasImported,
            ImportHouseNumberPositionFromCrab importHouseNumberPositionFromCrab)
        {
            var addressWasPositioned = new AddressWasPositioned(addressId,
                new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Lot, GeometryHelpers.CreateEwkbFrom(importHouseNumberPositionFromCrab.AddressPosition)));
            ((ISetProvenance)addressWasPositioned).SetProvenance(fixture.Create<Provenance>());

            addressHouseNumberPositionWasImported = addressHouseNumberPositionWasImported
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromLot)
                .WithBeginDate(crabLifetime.BeginDateTime);

            importHouseNumberPositionFromCrab = importHouseNumberPositionFromCrab
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromEntryOfBuilding)
                .WithLifetime(crabLifetime);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    addressWasPositioned,
                    addressHouseNumberPositionWasImported)
                .When(importHouseNumberPositionFromCrab)
                .Then(addressId,
                    new AddressWasPositioned(addressId,
                        new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Entry, GeometryHelpers.CreateEwkbFrom(importHouseNumberPositionFromCrab.AddressPosition))),
                    importHouseNumberPositionFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void WhenPositionQualityIsLower(
            Fixture fixture,
            AddressId addressId,
            CrabLifetime crabLifetime,
            AddressWasRegistered addressWasRegistered,
            AddressHouseNumberPositionWasImportedFromCrab addressHouseNumberPositionWasImported,
            ImportHouseNumberPositionFromCrab importHouseNumberPositionFromCrab)
        {
            var addressWasPositioned = new AddressWasPositioned(addressId,
                new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Lot, new ExtendedWkbGeometry(addressHouseNumberPositionWasImported.AddressPosition)));
            ((ISetProvenance)addressWasPositioned).SetProvenance(fixture.Create<Provenance>());

            addressHouseNumberPositionWasImported = addressHouseNumberPositionWasImported
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromLot)
                .WithBeginDate(crabLifetime.BeginDateTime);

            importHouseNumberPositionFromCrab = importHouseNumberPositionFromCrab
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.DerivedFromParcelCadastre)
                .WithLifetime(crabLifetime);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    addressWasPositioned,
                    addressHouseNumberPositionWasImported)
                .When(importHouseNumberPositionFromCrab)
                .Then(addressId,
                    importHouseNumberPositionFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void When2PositionsAreCurrentAndHighestGetsDeleted(
            Fixture fixture,
            AddressId addressId,
            CrabLifetime crabLifetime,
            AddressWasRegistered addressWasRegistered,
            AddressHouseNumberPositionWasImportedFromCrab addressHouseNumberPositionWasImportedHigh,
            AddressHouseNumberPositionWasImportedFromCrab addressHouseNumberPositionWasImportedLow,
            ImportHouseNumberPositionFromCrab importHouseNumberPositionFromCrab)
        {
            var addressWasPositioned = new AddressWasPositioned(addressId,
                new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Lot,
                    new ExtendedWkbGeometry(addressHouseNumberPositionWasImportedHigh.AddressPosition)));
            ((ISetProvenance)addressWasPositioned).SetProvenance(fixture.Create<Provenance>());

            addressHouseNumberPositionWasImportedHigh = addressHouseNumberPositionWasImportedHigh
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromLot)
                .WithBeginDate(crabLifetime.BeginDateTime);

            addressHouseNumberPositionWasImportedLow = addressHouseNumberPositionWasImportedLow
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.DerivedFromParcelCadastre)
                .WithBeginDate(crabLifetime.BeginDateTime);

            importHouseNumberPositionFromCrab = importHouseNumberPositionFromCrab
                .WithPositionId(new CrabAddressPositionId(addressHouseNumberPositionWasImportedHigh.AddressPositionId))
                .WithCrabModification(CrabModification.Delete)
                .WithLifetime(crabLifetime);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    addressWasPositioned,
                    addressHouseNumberPositionWasImportedHigh,
                    addressHouseNumberPositionWasImportedLow)
                .When(importHouseNumberPositionFromCrab)
                .Then(addressId,
                    new AddressWasPositioned(addressId,
                        new AddressGeometry(GeometryMethod.DerivedFromObject, GeometrySpecification.Parcel,
                            new ExtendedWkbGeometry(addressHouseNumberPositionWasImportedLow.AddressPosition))),
                    importHouseNumberPositionFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void When2PositionsAreCurrentAndHighestGetsHistorized(
            Fixture fixture,
            AddressId addressId,
            CrabLifetime crabLifetime,
            AddressWasRegistered addressWasRegistered,
            AddressHouseNumberPositionWasImportedFromCrab addressHouseNumberPositionWasImportedHigh,
            AddressHouseNumberPositionWasImportedFromCrab addressHouseNumberPositionWasImportedLow,
            ImportHouseNumberPositionFromCrab importHouseNumberPositionFromCrab)
        {
            var addressWasPositioned = new AddressWasPositioned(addressId,
                new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Lot,
                    new ExtendedWkbGeometry(addressHouseNumberPositionWasImportedHigh.AddressPosition)));
            ((ISetProvenance)addressWasPositioned).SetProvenance(fixture.Create<Provenance>());

            addressHouseNumberPositionWasImportedHigh = addressHouseNumberPositionWasImportedHigh
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromLot)
                .WithBeginDate(crabLifetime.BeginDateTime);

            addressHouseNumberPositionWasImportedLow = addressHouseNumberPositionWasImportedLow
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.DerivedFromParcelCadastre)
                .WithBeginDate(crabLifetime.BeginDateTime);

            importHouseNumberPositionFromCrab = importHouseNumberPositionFromCrab
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromLot)
                .WithPositionId(new CrabAddressPositionId(addressHouseNumberPositionWasImportedHigh.AddressPositionId))
                .WithLifetime(new CrabLifetime(crabLifetime.BeginDateTime, LocalDateTime.FromDateTime(DateTime.Now)));

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    addressWasPositioned,
                    addressHouseNumberPositionWasImportedHigh,
                    addressHouseNumberPositionWasImportedLow)
                .When(importHouseNumberPositionFromCrab)
                .Then(addressId,
                    new AddressWasPositioned(addressId,
                        new AddressGeometry(GeometryMethod.DerivedFromObject, GeometrySpecification.Parcel,
                            new ExtendedWkbGeometry(addressHouseNumberPositionWasImportedLow.AddressPosition))),
                    importHouseNumberPositionFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void When2PositionsAreCurrentAndLowestGetsDeleted(
            Fixture fixture,
            AddressId addressId,
            CrabLifetime crabLifetime,
            AddressWasRegistered addressWasRegistered,
            AddressHouseNumberPositionWasImportedFromCrab addressHouseNumberPositionWasImportedHigh,
            AddressHouseNumberPositionWasImportedFromCrab addressHouseNumberPositionWasImportedLow,
            ImportHouseNumberPositionFromCrab importHouseNumberPositionFromCrab)
        {
            var addressWasPositioned = new AddressWasPositioned(addressId,
                new AddressGeometry(GeometryMethod.DerivedFromObject, GeometrySpecification.Parcel, new ExtendedWkbGeometry(addressHouseNumberPositionWasImportedHigh.AddressPosition)));
            ((ISetProvenance)addressWasPositioned).SetProvenance(fixture.Create<Provenance>());
            var addressWasPositioned2 = new AddressWasPositioned(addressId,
                new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Lot,
                    new ExtendedWkbGeometry(addressHouseNumberPositionWasImportedHigh.AddressPosition)));
            ((ISetProvenance)addressWasPositioned2).SetProvenance(fixture.Create<Provenance>());

            addressHouseNumberPositionWasImportedLow = addressHouseNumberPositionWasImportedLow
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.DerivedFromParcelCadastre)
                .WithBeginDate(crabLifetime.BeginDateTime);

            addressHouseNumberPositionWasImportedHigh = addressHouseNumberPositionWasImportedHigh
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromLot)
                .WithBeginDate(crabLifetime.BeginDateTime);

            importHouseNumberPositionFromCrab = importHouseNumberPositionFromCrab
                .WithPositionId(new CrabAddressPositionId(addressHouseNumberPositionWasImportedLow.AddressPositionId))
                .WithCrabModification(CrabModification.Delete)
                .WithLifetime(crabLifetime);

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    addressWasPositioned,
                    addressHouseNumberPositionWasImportedLow,
                    addressWasPositioned2,
                    addressHouseNumberPositionWasImportedHigh)
                .When(importHouseNumberPositionFromCrab)
                .Then(addressId,
                    importHouseNumberPositionFromCrab.ToLegacyEvent()));
        }

        [Theory]
        [DefaultData]
        public void When2PositionsAreCurrentAndLowestGetsHistorized(
            Fixture fixture,
            AddressId addressId,
            CrabLifetime crabLifetime,
            AddressWasRegistered addressWasRegistered,
            AddressHouseNumberPositionWasImportedFromCrab addressHouseNumberPositionWasImportedHigh,
            AddressHouseNumberPositionWasImportedFromCrab addressHouseNumberPositionWasImportedLow,
            ImportHouseNumberPositionFromCrab importHouseNumberPositionFromCrab)
        {
            var addressWasPositioned = new AddressWasPositioned(addressId,
                new AddressGeometry(GeometryMethod.DerivedFromObject, GeometrySpecification.Parcel, new ExtendedWkbGeometry(addressHouseNumberPositionWasImportedHigh.AddressPosition)));
            ((ISetProvenance)addressWasPositioned).SetProvenance(fixture.Create<Provenance>());
            var addressWasPositioned2 = new AddressWasPositioned(addressId,
                new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Lot,
                    new ExtendedWkbGeometry(addressHouseNumberPositionWasImportedHigh.AddressPosition)));
            ((ISetProvenance)addressWasPositioned2).SetProvenance(fixture.Create<Provenance>());

            addressHouseNumberPositionWasImportedLow = addressHouseNumberPositionWasImportedLow
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.DerivedFromParcelCadastre)
                .WithBeginDate(crabLifetime.BeginDateTime);

            addressHouseNumberPositionWasImportedHigh = addressHouseNumberPositionWasImportedHigh
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromLot)
                .WithBeginDate(crabLifetime.BeginDateTime);

            importHouseNumberPositionFromCrab = importHouseNumberPositionFromCrab
                .WithPositionId(new CrabAddressPositionId(addressHouseNumberPositionWasImportedLow.AddressPositionId))
                .WithCrabAddressPositionOrigin(CrabAddressPositionOrigin.DerivedFromParcelCadastre)
                .WithLifetime(new CrabLifetime(crabLifetime.BeginDateTime, LocalDateTime.FromDateTime(DateTime.Now)));

            Assert(new Scenario()
                .Given(addressId,
                    addressWasRegistered,
                    addressWasPositioned,
                    addressHouseNumberPositionWasImportedLow,
                    addressWasPositioned2,
                    addressHouseNumberPositionWasImportedHigh)
                .When(importHouseNumberPositionFromCrab)
                .Then(addressId,
                    importHouseNumberPositionFromCrab.ToLegacyEvent()));
        }
    }
}
