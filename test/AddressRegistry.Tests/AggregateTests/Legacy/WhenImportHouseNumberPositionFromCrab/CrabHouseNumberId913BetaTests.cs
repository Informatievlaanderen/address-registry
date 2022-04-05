namespace AddressRegistry.Tests.AggregateTests.Legacy.WhenImportHouseNumberPositionFromCrab
{
    using Address.Commands.Crab;
    using Address.Events;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using global::AutoFixture;
    using NodaTime;
    using System;
    using Address.ValueObjects;
    using Address.ValueObjects.Crab;
    using WhenImportHouseNumberFromCrab;
    using Xunit;
    using Xunit.Abstractions;

    public class CrabHouseNumberId913BetaTests : AddressRegistryTest
    {
        private CrabStreetNameId _streetNameId;
        private readonly HouseNumber _houseNumber;

        public CrabHouseNumberId913BetaTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture fixture = new Fixture();
            _streetNameId = fixture.Create<CrabStreetNameId>();
            _houseNumber = fixture.Create<HouseNumber>();
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
            WkbGeometry geometry1,
            WkbGeometry geometry2)
        {
            Assert(Scenario2(fixture, crabHouseNumberId, geometry1, geometry2));
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
            WkbGeometry geometry2)
        {
            Assert(Scenario4(fixture, crabHouseNumberId, geometry1, geometry2));
        }

        [Theory]
        [DefaultData]
        public void TestScenario5(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry1,
            WkbGeometry geometry2)
        {
            Assert(Scenario5(fixture, crabHouseNumberId, geometry1, geometry2));
        }

        [Theory]
        [DefaultData]
        public void TestScenario6(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry1,
            WkbGeometry geometry2,
            WkbGeometry geometry3)
        {
            Assert(Scenario6(fixture, crabHouseNumberId, geometry1, geometry2));
        }

        [Theory]
        [DefaultData]
        public void TestScenario7(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry1,
            WkbGeometry geometry2,
            WkbGeometry geometry3)
        {
            Assert(Scenario7(fixture, crabHouseNumberId, geometry1, geometry2, geometry3));
        }

        [Theory]
        [DefaultData]
        public void TestScenario8(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry1,
            WkbGeometry geometry2,
            WkbGeometry geometry3)
        {
            Assert(Scenario8(fixture, crabHouseNumberId, geometry1, geometry2, geometry3));
        }

        [Theory]
        [DefaultData]
        public void TestScenario9(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry1,
            WkbGeometry geometry2,
            WkbGeometry geometry3)
        {
            Assert(Scenario9(fixture, crabHouseNumberId, geometry1, geometry2, geometry3));
        }

        [Theory]
        [DefaultData]
        public void TestScenario10(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry1,
            WkbGeometry geometry2,
            WkbGeometry geometry3)
        {
            Assert(Scenario10(fixture, crabHouseNumberId, geometry1, geometry2, geometry3));
        }

        [Theory]
        [DefaultData]
        public void TestExtraScenarioAddressGetRetired(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry1,
            WkbGeometry geometry2,
            WkbGeometry geometry3)
        {
            Assert(ExtraScenarioAddressGetRetired(fixture, crabHouseNumberId, geometry1, geometry2, geometry3));
        }

        private IEventCentricTestSpecificationBuilder Scenario1(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry)
        {
            var addressId = new AddressId(crabHouseNumberId.CreateDeterministicId());

            var addressWasRegistered = new AddressWasRegistered(
                addressId,
                new StreetNameId(_streetNameId.CreateDeterministicId()),
                _houseNumber);

            ((ISetProvenance)addressWasRegistered).SetProvenance(fixture.Create<Provenance>());

            var command = new ImportHouseNumberPositionFromCrab(
                new CrabAddressPositionId(902),
                crabHouseNumberId,
                geometry,
                new CrabAddressNature("2"),
                Enum.Parse<CrabAddressPositionOrigin>("15"),
                new CrabLifetime(LocalDateTime.FromDateTime(new DateTime(1830, 1, 1)), null),
                new CrabTimestamp(Instant.FromDateTimeOffset(new DateTimeOffset(2011, 4, 29, 14, 50, 10, 483, TimeSpan.Zero))),
                new CrabOperator("VLM\\thijsge"),
                CrabModification.Insert,
                Enum.Parse<CrabOrganisation>("5"));

            return new Scenario()
                .Given(addressId, addressWasRegistered)
                .When(command)
                .Then(addressId,
                    new AddressWasPositioned(addressId, new AddressGeometry(GeometryMethod.Interpolated, GeometrySpecification.Parcel, GeometryHelpers.CreateEwkbFrom(geometry))),
                    command.ToLegacyEvent());
        }

        private IEventCentricTestSpecificationBuilder Scenario2(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry1, WkbGeometry geometry2)
        {
            var addressId = new AddressId(crabHouseNumberId.CreateDeterministicId());

            var command = new ImportHouseNumberPositionFromCrab(
                new CrabAddressPositionId(3408534),
                crabHouseNumberId,
                geometry2,
                new CrabAddressNature("2"),
                Enum.Parse<CrabAddressPositionOrigin>("2"),
                new CrabLifetime(LocalDateTime.FromDateTime(new DateTime(1830, 1, 1)), null),
                new CrabTimestamp(Instant.FromDateTimeOffset(new DateTimeOffset(2013, 1, 23, 14, 23, 11, 877, TimeSpan.Zero))),
                new CrabOperator("13029:kenisils"),
                CrabModification.Insert,
                Enum.Parse<CrabOrganisation>("1"));

            return new AutoFixtureScenario(fixture)
                .Given(Scenario1(fixture, crabHouseNumberId, geometry1))
                .When(command)
                .Then(addressId,
                    new AddressWasPositioned(addressId, new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Parcel, GeometryHelpers.CreateEwkbFrom(geometry2))),
                    command.ToLegacyEvent());
        }

        private IEventCentricTestSpecificationBuilder Scenario3(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry1,
            WkbGeometry geometry2)
        {
            var addressId = new AddressId(crabHouseNumberId.CreateDeterministicId());

            var command = new ImportHouseNumberPositionFromCrab(
                new CrabAddressPositionId(902),
                crabHouseNumberId,
                geometry1,
                new CrabAddressNature("2"),
                Enum.Parse<CrabAddressPositionOrigin>("15"),
                new CrabLifetime(LocalDateTime.FromDateTime(new DateTime(1830, 1, 1)), null),
                new CrabTimestamp(Instant.FromDateTimeOffset(new DateTimeOffset(2013, 1, 31, 5, 38, 25, 700, TimeSpan.Zero))),
                new CrabOperator("VLM\\CRABSSISservice"),
                CrabModification.Delete,
                Enum.Parse<CrabOrganisation>("5"));

            return new AutoFixtureScenario(fixture)
                .Given(Scenario2(fixture, crabHouseNumberId, geometry1, geometry2))
                .When(command)
                .Then(addressId,
                    command.ToLegacyEvent());
        }

        private IEventCentricTestSpecificationBuilder Scenario4(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry1,
            WkbGeometry geometry2)
        {
            var addressId = new AddressId(crabHouseNumberId.CreateDeterministicId());

            var command = new ImportHouseNumberPositionFromCrab(
                new CrabAddressPositionId(3408534),
                crabHouseNumberId,
                geometry2,
                new CrabAddressNature("2"),
                Enum.Parse<CrabAddressPositionOrigin>("2"),
                new CrabLifetime(LocalDateTime.FromDateTime(new DateTime(1830, 1, 1)), LocalDateTime.FromDateTime(new DateTime(2004, 10, 3))),
                new CrabTimestamp(Instant.FromDateTimeOffset(new DateTimeOffset(2013, 5, 29, 10, 49, 44, 0, TimeSpan.Zero))),
                new CrabOperator("13029:kenisils"),
                CrabModification.Historize,
                Enum.Parse<CrabOrganisation>("1"));

            return new AutoFixtureScenario(fixture)
                .Given(Scenario3(fixture, crabHouseNumberId, geometry1, geometry2))
                .When(command)
                .Then(addressId,
                    new AddressPositionWasRemoved(addressId),
                    command.ToLegacyEvent());
        }

        private IEventCentricTestSpecificationBuilder Scenario5(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry1,
            WkbGeometry geometry2)
        {
            var addressId = new AddressId(crabHouseNumberId.CreateDeterministicId());

            var command = new ImportHouseNumberPositionFromCrab(
                new CrabAddressPositionId(3408534),
                crabHouseNumberId,
                geometry2,
                new CrabAddressNature("2"),
                Enum.Parse<CrabAddressPositionOrigin>("2"),
                new CrabLifetime(LocalDateTime.FromDateTime(new DateTime(1830, 1, 1)), LocalDateTime.FromDateTime(new DateTime(2004, 10, 3))),
                new CrabTimestamp(Instant.FromDateTimeOffset(new DateTimeOffset(2013, 5, 29, 11, 21, 34, 683, TimeSpan.Zero))),
                new CrabOperator("13029:kenisils"),
                CrabModification.Correction,
                Enum.Parse<CrabOrganisation>("1"));

            return new AutoFixtureScenario(fixture)
                .Given(Scenario4(fixture, crabHouseNumberId, geometry1, geometry2))
                .When(command)
                .Then(addressId,
                    command.ToLegacyEvent());
        }

        private IEventCentricTestSpecificationBuilder Scenario6(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry1,
            WkbGeometry geometry2)
        {
            var addressId = new AddressId(crabHouseNumberId.CreateDeterministicId());

            var command = new ImportHouseNumberPositionFromCrab(
                new CrabAddressPositionId(3408534),
                crabHouseNumberId,
                geometry2,
                new CrabAddressNature("2"),
                Enum.Parse<CrabAddressPositionOrigin>("2"),
                new CrabLifetime(LocalDateTime.FromDateTime(new DateTime(1830, 1, 1)), LocalDateTime.FromDateTime(new DateTime(2004, 10, 3))),
                new CrabTimestamp(Instant.FromDateTimeOffset(new DateTimeOffset(2013, 5, 29, 23, 36, 54, 547, TimeSpan.Zero))),
                new CrabOperator("13029:kenisils"),
                CrabModification.Correction,
                Enum.Parse<CrabOrganisation>("1"));

            return new AutoFixtureScenario(fixture)
                .Given(Scenario5(fixture, crabHouseNumberId, geometry1, geometry2))
                .When(command)
                .Then(addressId,
                    command.ToLegacyEvent());
        }

        private IEventCentricTestSpecificationBuilder Scenario7(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry1,
            WkbGeometry geometry2,
            WkbGeometry geometry3)
        {
            var addressId = new AddressId(crabHouseNumberId.CreateDeterministicId());

            var command = new ImportHouseNumberPositionFromCrab(
                new CrabAddressPositionId(3516111),
                crabHouseNumberId,
                geometry3,
                new CrabAddressNature("2"),
                Enum.Parse<CrabAddressPositionOrigin>("13"),
                new CrabLifetime(LocalDateTime.FromDateTime(new DateTime(1830, 1, 1)), null),
                new CrabTimestamp(Instant.FromDateTimeOffset(new DateTimeOffset(2013, 5, 30, 5, 40, 47, 760, TimeSpan.Zero))),
                new CrabOperator("VLM\\CRABSSISservice"),
                CrabModification.Insert,
                Enum.Parse<CrabOrganisation>("5"));

            return new AutoFixtureScenario(fixture)
                .Given(Scenario6(fixture, crabHouseNumberId, geometry1, geometry2))
                .When(command)
                .Then(addressId,
                    new AddressWasPositioned(addressId, new AddressGeometry(GeometryMethod.Interpolated, GeometrySpecification.Parcel, GeometryHelpers.CreateEwkbFrom(geometry3))),
                    command.ToLegacyEvent());
        }

        private IEventCentricTestSpecificationBuilder Scenario8(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry1,
            WkbGeometry geometry2,
            WkbGeometry geometry3)
        {
            var addressId = new AddressId(crabHouseNumberId.CreateDeterministicId());

            var command = new ImportHouseNumberPositionFromCrab(
                new CrabAddressPositionId(3516111),
                crabHouseNumberId,
                geometry3,
                new CrabAddressNature("2"),
                Enum.Parse<CrabAddressPositionOrigin>("13"),
                new CrabLifetime(LocalDateTime.FromDateTime(new DateTime(1830, 1, 1)), LocalDateTime.FromDateTime(new DateTime(2014, 12, 31))),
                new CrabTimestamp(Instant.FromDateTimeOffset(new DateTimeOffset(2013, 6, 4, 13, 1, 9, 137, TimeSpan.Zero))),
                new CrabOperator("13029:kenisils"),
                CrabModification.Historize,
                Enum.Parse<CrabOrganisation>("1"));

            return new AutoFixtureScenario(fixture)
                .Given(Scenario7(fixture, crabHouseNumberId, geometry1, geometry2, geometry3))
                .When(command)
                .Then(addressId,
                    new AddressPositionWasRemoved(addressId),
                    command.ToLegacyEvent());
        }

        private IEventCentricTestSpecificationBuilder Scenario9(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry1,
            WkbGeometry geometry2,
            WkbGeometry geometry3)
        {
            var addressId = new AddressId(crabHouseNumberId.CreateDeterministicId());

            var command = new ImportHouseNumberPositionFromCrab(
                new CrabAddressPositionId(3516111),
                crabHouseNumberId,
                geometry3,
                new CrabAddressNature("2"),
                Enum.Parse<CrabAddressPositionOrigin>("13"),
                new CrabLifetime(LocalDateTime.FromDateTime(new DateTime(1830, 1, 1)), LocalDateTime.FromDateTime(new DateTime(2014, 12, 31))),
                new CrabTimestamp(Instant.FromDateTimeOffset(new DateTimeOffset(2013, 6, 4, 13, 29, 28, 333, TimeSpan.Zero))),
                new CrabOperator("13029:kenisils"),
                CrabModification.Correction,
                Enum.Parse<CrabOrganisation>("1"));

            return new AutoFixtureScenario(fixture)
                .Given(Scenario8(fixture, crabHouseNumberId, geometry1, geometry2, geometry3))
                .When(command)
                .Then(addressId,
                    command.ToLegacyEvent());
        }

        private IEventCentricTestSpecificationBuilder Scenario10(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry1,
            WkbGeometry geometry2,
            WkbGeometry geometry3)
        {
            var addressId = new AddressId(crabHouseNumberId.CreateDeterministicId());

            var command = new ImportHouseNumberPositionFromCrab(
                new CrabAddressPositionId(3408534),
                crabHouseNumberId,
                geometry2,
                new CrabAddressNature("2"),
                Enum.Parse<CrabAddressPositionOrigin>("2"),
                new CrabLifetime(LocalDateTime.FromDateTime(new DateTime(1830, 1, 1)), LocalDateTime.FromDateTime(new DateTime(2014, 10, 3))),
                new CrabTimestamp(Instant.FromDateTimeOffset(new DateTimeOffset(2014, 10, 17, 20, 26, 30, 480, TimeSpan.Zero))),
                new CrabOperator("13029:kenisils"),
                CrabModification.Correction,
                Enum.Parse<CrabOrganisation>("1"));

            return new AutoFixtureScenario(fixture)
                .Given(Scenario9(fixture, crabHouseNumberId, geometry1, geometry2, geometry3))
                .When(command)
                .Then(addressId,
                    command.ToLegacyEvent());
        }

        private IEventCentricTestSpecificationBuilder ExtraScenarioAddressGetRetired(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry1,
            WkbGeometry geometry2,
            WkbGeometry geometry3)
        {
            var addressId = new AddressId(crabHouseNumberId.CreateDeterministicId());

            var command = new ImportHouseNumberFromCrab(
                crabHouseNumberId,
                _streetNameId,
                _houseNumber,
                fixture.Create<GrbNotation>(),
                new CrabLifetime(LocalDateTime.FromDateTime(new DateTime(1830, 1, 1)), LocalDateTime.FromDateTime(new DateTime(2014, 10, 3))),
                new CrabTimestamp(Instant.FromDateTimeOffset(new DateTimeOffset(2014, 10, 17, 20, 26, 30, 480, TimeSpan.Zero))),
                new CrabOperator("13029:kenisils"),
                CrabModification.Historize,
                Enum.Parse<CrabOrganisation>("1"));

            return new AutoFixtureScenario(fixture)
                .Given(Scenario10(fixture, crabHouseNumberId, geometry1, geometry2, geometry3))
                .When(command)
                .Then(addressId,
                    new AddressWasRetired(addressId),
                    new AddressPositionWasCorrected(addressId, new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Parcel, GeometryHelpers.CreateEwkbFrom(geometry2))),
                    command.ToLegacyEvent());
        }
    }
}
