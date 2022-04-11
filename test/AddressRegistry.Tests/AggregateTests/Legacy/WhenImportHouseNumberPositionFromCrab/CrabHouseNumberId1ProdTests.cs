namespace AddressRegistry.Tests.AggregateTests.Legacy.WhenImportHouseNumberPositionFromCrab
{
    using System;
    using Address;
    using Address.Commands.Crab;
    using Address.Crab;
    using Address.Events;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using global::AutoFixture;
    using NodaTime;
    using Xunit;
    using Xunit.Abstractions;

    public class CrabHouseNumberId1ProdTests : AddressRegistryTest
    {
        public CrabHouseNumberId1ProdTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Theory]
        [DefaultData]
        public void TestScenario1(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry)
        {
            Assert(Scenario1(fixture, crabHouseNumberId, geometry));
        }

        [Theory]
        [DefaultData]
        public void TestScenario2(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry)
        {
            Assert(Scenario2(fixture, crabHouseNumberId, geometry));
        }

        [Theory]
        [DefaultData]
        public void TestScenario3(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry1,
            WkbGeometry geometry2)
        {
            Assert(Scenario3(fixture, crabHouseNumberId, geometry1, geometry2));
        }

        [Theory]
        [DefaultData]
        public void TestScenario4(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry1,
            WkbGeometry geometry2,
            WkbGeometry geometry3)
        {
            Assert(Scenario4(fixture, crabHouseNumberId, geometry1, geometry2, geometry3));
        }

        [Theory]
        [DefaultData]
        public void TestScenario5(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry1,
            WkbGeometry geometry2,
            WkbGeometry geometry3)
        {
            Assert(Scenario5(fixture, crabHouseNumberId, geometry1, geometry2, geometry3));
        }

        [Theory]
        [DefaultData]
        public void TestScenario6(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry1,
            WkbGeometry geometry2,
            WkbGeometry geometry3,
            WkbGeometry geometry4)
        {
            Assert(Scenario6(fixture, crabHouseNumberId, geometry1, geometry2, geometry3, geometry4));
        }

        [Theory]
        [DefaultData]
        public void TestScenario7(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry1,
            WkbGeometry geometry2,
            WkbGeometry geometry3,
            WkbGeometry geometry4)
        {
            Assert(Scenario7(fixture, crabHouseNumberId, geometry1, geometry2, geometry3, geometry4));
        }

        [Theory]
        [DefaultData]
        public void TestScenario8(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry1,
            WkbGeometry geometry2,
            WkbGeometry geometry3,
            WkbGeometry geometry4,
            WkbGeometry geometry5)
        {
            Assert(Scenario8(fixture, crabHouseNumberId, geometry1, geometry2, geometry3, geometry4, geometry5));
        }

        private IEventCentricTestSpecificationBuilder Scenario1(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry)
        {
            var addressId = new AddressId(crabHouseNumberId.CreateDeterministicId());

            var addressWasRegistered = new AddressWasRegistered(
                addressId,
                fixture.Create<StreetNameId>(),
                fixture.Create<HouseNumber>());

            ((ISetProvenance)addressWasRegistered).SetProvenance(fixture.Create<Provenance>());

            var command = new ImportHouseNumberPositionFromCrab(
                new CrabAddressPositionId(1),
                crabHouseNumberId,
                geometry,
                new CrabAddressNature("2"),
                Enum.Parse<CrabAddressPositionOrigin>("10"),
                new CrabLifetime(LocalDateTime.FromDateTime(new DateTime(1830, 1, 1)), null),
                new CrabTimestamp(Instant.FromDateTimeOffset(new DateTimeOffset(2011, 4, 29, 14, 50, 10, 483, TimeSpan.Zero))),
                new CrabOperator("VLM\\thijsge"),
                CrabModification.Insert,
                Enum.Parse<CrabOrganisation>("5"));

            return new Scenario()
                .Given(addressId, addressWasRegistered)
                .When(command)
                .Then(addressId,
                    new AddressWasPositioned(addressId, new AddressGeometry(GeometryMethod.DerivedFromObject, GeometrySpecification.BuildingUnit, GeometryHelpers.CreateEwkbFrom(geometry))),
                    command.ToLegacyEvent());
        }

        private IEventCentricTestSpecificationBuilder Scenario2(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry)
        {
            var addressId = new AddressId(crabHouseNumberId.CreateDeterministicId());

            var command = new ImportHouseNumberPositionFromCrab(
                new CrabAddressPositionId(1),
                crabHouseNumberId,
                geometry,
                new CrabAddressNature("2"),
                Enum.Parse<CrabAddressPositionOrigin>("10"),
                new CrabLifetime(LocalDateTime.FromDateTime(new DateTime(1830, 1, 1)), null),
                new CrabTimestamp(Instant.FromDateTimeOffset(new DateTimeOffset(2014, 2, 2, 20, 43, 51, 373, TimeSpan.Zero))),
                new CrabOperator("VLM\\CRABSSISservice"),
                CrabModification.Delete,
                Enum.Parse<CrabOrganisation>("1"));

            return new AutoFixtureScenario(fixture)
                .Given(Scenario1(fixture, crabHouseNumberId, geometry))
                .When(command)
                .Then(addressId,
                    new AddressPositionWasRemoved(addressId),
                    command.ToLegacyEvent());
        }

        private IEventCentricTestSpecificationBuilder Scenario3(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry1,
            WkbGeometry geometry2)
        {
            var addressId = new AddressId(crabHouseNumberId.CreateDeterministicId());

            var command = new ImportHouseNumberPositionFromCrab(
                new CrabAddressPositionId(4087928),
                crabHouseNumberId,
                geometry2,
                new CrabAddressNature("2"),
                Enum.Parse<CrabAddressPositionOrigin>("2"),
                new CrabLifetime(LocalDateTime.FromDateTime(new DateTime(1830, 1, 1)), null),
                new CrabTimestamp(Instant.FromDateTimeOffset(new DateTimeOffset(2014, 2, 2, 22, 21, 24, 997, TimeSpan.Zero))),
                new CrabOperator("VLM\\CRABSSISservice"),
                CrabModification.Insert,
                Enum.Parse<CrabOrganisation>("1"));

            return new AutoFixtureScenario(fixture)
                .Given(Scenario2(fixture, crabHouseNumberId, geometry1))
                .When(command)
                .Then(addressId,
                    new AddressWasPositioned(addressId, new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Parcel, GeometryHelpers.CreateEwkbFrom(geometry2))),
                    command.ToLegacyEvent());
        }

        private IEventCentricTestSpecificationBuilder Scenario4(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry1,
            WkbGeometry geometry2,
            WkbGeometry geometry3)
        {
            var addressId = new AddressId(crabHouseNumberId.CreateDeterministicId());

            var command = new ImportHouseNumberPositionFromCrab(
                new CrabAddressPositionId(4087928),
                crabHouseNumberId,
                geometry3,
                new CrabAddressNature("2"),
                Enum.Parse<CrabAddressPositionOrigin>("2"),
                new CrabLifetime(LocalDateTime.FromDateTime(new DateTime(1830, 1, 1)), null),
                new CrabTimestamp(Instant.FromDateTimeOffset(new DateTimeOffset(2014, 2, 11, 17, 26, 44, 190, TimeSpan.Zero))),
                new CrabOperator("VLM\\CRABSSISservice"),
                CrabModification.Correction,
                Enum.Parse<CrabOrganisation>("1"));

            return new AutoFixtureScenario(fixture)
                .Given(Scenario3(fixture, crabHouseNumberId, geometry1, geometry2))
                .When(command)
                .Then(addressId,
                    new AddressPositionWasCorrected(addressId, new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Parcel, GeometryHelpers.CreateEwkbFrom(geometry3))),
                    command.ToLegacyEvent());
        }

        private IEventCentricTestSpecificationBuilder Scenario5(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry1,
            WkbGeometry geometry2,
            WkbGeometry geometry3)
        {
            var addressId = new AddressId(crabHouseNumberId.CreateDeterministicId());

            var command = new ImportHouseNumberPositionFromCrab(
                new CrabAddressPositionId(4087928),
                crabHouseNumberId,
                geometry3,
                new CrabAddressNature("2"),
                Enum.Parse<CrabAddressPositionOrigin>("2"),
                new CrabLifetime(LocalDateTime.FromDateTime(new DateTime(1830, 1, 1)), null),
                new CrabTimestamp(Instant.FromDateTimeOffset(new DateTimeOffset(2014, 3, 19, 16, 56, 36, 90, TimeSpan.Zero))),
                new CrabOperator("VLM\\CRABSSISservice"),
                CrabModification.Correction,
                Enum.Parse<CrabOrganisation>("1"));

            return new AutoFixtureScenario(fixture)
                .Given(Scenario4(fixture, crabHouseNumberId, geometry1, geometry2, geometry3))
                .When(command)
                .Then(addressId,
                    command.ToLegacyEvent());
        }

        private IEventCentricTestSpecificationBuilder Scenario6(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry1,
            WkbGeometry geometry2,
            WkbGeometry geometry3,
            WkbGeometry geometry4)
        {
            var addressId = new AddressId(crabHouseNumberId.CreateDeterministicId());

            var command = new ImportHouseNumberPositionFromCrab(
                new CrabAddressPositionId(5790888),
                crabHouseNumberId,
                geometry4,
                new CrabAddressNature("2"),
                Enum.Parse<CrabAddressPositionOrigin>("10"),
                new CrabLifetime(LocalDateTime.FromDateTime(new DateTime(1830, 1, 1)), null),
                new CrabTimestamp(Instant.FromDateTimeOffset(new DateTimeOffset(2015, 7, 30, 10, 57, 33, 273, TimeSpan.Zero))),
                new CrabOperator("13040:7405:DAERO"),
                CrabModification.Insert,
                Enum.Parse<CrabOrganisation>("5"));

            return new AutoFixtureScenario(fixture)
                .Given(Scenario5(fixture, crabHouseNumberId, geometry1, geometry2, geometry3))
                .When(command)
                .Then(addressId,
                    command.ToLegacyEvent());
        }

        private IEventCentricTestSpecificationBuilder Scenario7(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry1,
            WkbGeometry geometry2,
            WkbGeometry geometry3,
            WkbGeometry geometry4)
        {
            var addressId = new AddressId(crabHouseNumberId.CreateDeterministicId());

            var command = new ImportHouseNumberPositionFromCrab(
                new CrabAddressPositionId(4087928),
                crabHouseNumberId,
                geometry3,
                new CrabAddressNature("2"),
                Enum.Parse<CrabAddressPositionOrigin>("2"),
                new CrabLifetime(LocalDateTime.FromDateTime(new DateTime(1830, 1, 1)), null),
                new CrabTimestamp(Instant.FromDateTimeOffset(new DateTimeOffset(2015, 7, 30, 10, 57, 33, 920, TimeSpan.Zero))),
                new CrabOperator("13040:7405:DAERO"),
                CrabModification.Delete,
                Enum.Parse<CrabOrganisation>("1"));

            return new AutoFixtureScenario(fixture)
                .Given(Scenario6(fixture, crabHouseNumberId, geometry1, geometry2, geometry3, geometry4))
                .When(command)
                .Then(addressId,
                    new AddressWasPositioned(addressId, new AddressGeometry(GeometryMethod.DerivedFromObject, GeometrySpecification.BuildingUnit, GeometryHelpers.CreateEwkbFrom(geometry4))),
                    command.ToLegacyEvent());
        }

        private IEventCentricTestSpecificationBuilder Scenario8(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry1,
            WkbGeometry geometry2,
            WkbGeometry geometry3,
            WkbGeometry geometry4,
            WkbGeometry geometry5)
        {
            var addressId = new AddressId(crabHouseNumberId.CreateDeterministicId());

            var command = new ImportHouseNumberPositionFromCrab(
                new CrabAddressPositionId(5790888),
                crabHouseNumberId,
                geometry5,
                new CrabAddressNature("2"),
                Enum.Parse<CrabAddressPositionOrigin>("10"),
                new CrabLifetime(LocalDateTime.FromDateTime(new DateTime(1830, 1, 1)), null),
                new CrabTimestamp(Instant.FromDateTimeOffset(new DateTimeOffset(2016, 2, 1, 10, 24, 37, 907, TimeSpan.Zero))),
                new CrabOperator("VLM\\daemsgl"),
                CrabModification.Correction,
                Enum.Parse<CrabOrganisation>("5"));

            return new AutoFixtureScenario(fixture)
                .Given(Scenario7(fixture, crabHouseNumberId, geometry1, geometry2, geometry3, geometry4))
                .When(command)
                .Then(addressId,
                    new AddressPositionWasCorrected(addressId, new AddressGeometry(GeometryMethod.DerivedFromObject, GeometrySpecification.BuildingUnit, GeometryHelpers.CreateEwkbFrom(geometry5))),
                    command.ToLegacyEvent());
        }
    }
}
