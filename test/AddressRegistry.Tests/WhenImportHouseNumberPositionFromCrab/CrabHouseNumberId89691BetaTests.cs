namespace AddressRegistry.Tests.WhenImportHouseNumberPositionFromCrab
{
    using Address.Commands.Crab;
    using Address.Events;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Crab;
    using global::AutoFixture;
    using NodaTime;
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Xunit;
    using Xunit.Abstractions;

    public class CrabHouseNumberId89691BetaTests : AddressRegistryTest
    {
        public CrabHouseNumberId89691BetaTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
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
            WkbGeometry geometry2,
            WkbGeometry geometry3)
        {
            Assert(Scenario3(fixture, crabHouseNumberId, geometry1, geometry2, geometry3));
        }

        [Theory]
        [DefaultData]
        public void TestScenario4(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry1,
            WkbGeometry geometry2,
            WkbGeometry geometry3,
            WkbGeometry geometry4)
        {
            Assert(Scenario4(fixture, crabHouseNumberId, geometry1, geometry2, geometry3, geometry4));
        }

        [Theory]
        [DefaultData]
        public void TestScenario5(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry1,
            WkbGeometry geometry2,
            WkbGeometry geometry3,
            WkbGeometry geometry4,
            WkbGeometry geometry5)
        {
            Assert(Scenario5(fixture, crabHouseNumberId, geometry1, geometry2, geometry3, geometry4, geometry5));
        }

        [Theory]
        [DefaultData]
        public void TestScenario6(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry1,
            WkbGeometry geometry2,
            WkbGeometry geometry3,
            WkbGeometry geometry4,
            WkbGeometry geometry5,
            WkbGeometry geometry6)
        {
            Assert(Scenario6(fixture, crabHouseNumberId, geometry1, geometry2, geometry3, geometry4, geometry5, geometry6));
        }

        [Theory]
        [DefaultData]
        public void TestScenario7(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry1,
            WkbGeometry geometry2,
            WkbGeometry geometry3,
            WkbGeometry geometry4,
            WkbGeometry geometry5,
            WkbGeometry geometry6)
        {
            Assert(Scenario7(fixture, crabHouseNumberId, geometry1, geometry2, geometry3, geometry4, geometry5, geometry6));
        }

        [Theory]
        [DefaultData]
        public void TestScenario8(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry1,
            WkbGeometry geometry2,
            WkbGeometry geometry3,
            WkbGeometry geometry4,
            WkbGeometry geometry5,
            WkbGeometry geometry6,
            WkbGeometry geometry7)
        {
            Assert(Scenario8(fixture, crabHouseNumberId, geometry1, geometry2, geometry3, geometry4, geometry5, geometry6, geometry7));
        }

        [Theory]
        [DefaultData]
        public void TestScenario9(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry1,
            WkbGeometry geometry2,
            WkbGeometry geometry3,
            WkbGeometry geometry4,
            WkbGeometry geometry5,
            WkbGeometry geometry6,
            WkbGeometry geometry7,
            WkbGeometry geometry8)
        {
            Assert(Scenario9(fixture, crabHouseNumberId, geometry1, geometry2, geometry3, geometry4, geometry5, geometry6, geometry7, geometry8));
        }

        [Theory]
        [DefaultData]
        public void TestScenario10(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry1,
            WkbGeometry geometry2,
            WkbGeometry geometry3,
            WkbGeometry geometry4,
            WkbGeometry geometry5,
            WkbGeometry geometry6,
            WkbGeometry geometry7,
            WkbGeometry geometry8,
            WkbGeometry geometry9)
        {
            Assert(Scenario10(fixture, crabHouseNumberId, geometry1, geometry2, geometry3, geometry4, geometry5, geometry6, geometry7, geometry8, geometry9));
        }

        [Theory]
        [DefaultData]
        public void TestScenario11(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry1,
            WkbGeometry geometry2,
            WkbGeometry geometry3,
            WkbGeometry geometry4,
            WkbGeometry geometry5,
            WkbGeometry geometry6,
            WkbGeometry geometry7,
            WkbGeometry geometry8,
            WkbGeometry geometry9)
        {
            Assert(Scenario11(fixture, crabHouseNumberId, geometry1, geometry2, geometry3, geometry4, geometry5, geometry6, geometry7, geometry8, geometry9));
        }

        [Theory]
        [DefaultData]
        public void TestScenario12(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry1,
            WkbGeometry geometry2,
            WkbGeometry geometry3,
            WkbGeometry geometry4,
            WkbGeometry geometry5,
            WkbGeometry geometry6,
            WkbGeometry geometry7,
            WkbGeometry geometry8,
            WkbGeometry geometry9)
        {
            Assert(Scenario12(fixture, crabHouseNumberId, geometry1, geometry2, geometry3, geometry4, geometry5, geometry6, geometry7, geometry8, geometry9));
        }

        [Theory]
        [DefaultData]
        public void TestScenario13(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry1,
            WkbGeometry geometry2,
            WkbGeometry geometry3,
            WkbGeometry geometry4,
            WkbGeometry geometry5,
            WkbGeometry geometry6,
            WkbGeometry geometry7,
            WkbGeometry geometry8,
            WkbGeometry geometry9)
        {
            Assert(Scenario13(fixture, crabHouseNumberId, geometry1, geometry2, geometry3, geometry4, geometry5, geometry6, geometry7, geometry8, geometry9));
        }

        [Theory]
        [DefaultData]
        public void TestScenario14(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry1,
            WkbGeometry geometry2,
            WkbGeometry geometry3,
            WkbGeometry geometry4,
            WkbGeometry geometry5,
            WkbGeometry geometry6,
            WkbGeometry geometry7,
            WkbGeometry geometry8,
            WkbGeometry geometry9)
        {
            Assert(Scenario14(fixture, crabHouseNumberId, geometry1, geometry2, geometry3, geometry4, geometry5, geometry6, geometry7, geometry8, geometry9));
        }

        [Theory]
        [DefaultData]
        public void TestScenario15(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry1,
            WkbGeometry geometry2,
            WkbGeometry geometry3,
            WkbGeometry geometry4,
            WkbGeometry geometry5,
            WkbGeometry geometry6,
            WkbGeometry geometry7,
            WkbGeometry geometry8,
            WkbGeometry geometry9)
        {
            Assert(Scenario15(fixture, crabHouseNumberId, geometry1, geometry2, geometry3, geometry4, geometry5, geometry6, geometry7, geometry8, geometry9));
        }

        [Theory]
        [DefaultData]
        public void TestScenario16(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry1,
            WkbGeometry geometry2,
            WkbGeometry geometry3,
            WkbGeometry geometry4,
            WkbGeometry geometry5,
            WkbGeometry geometry6,
            WkbGeometry geometry7,
            WkbGeometry geometry8,
            WkbGeometry geometry9)
        {
            Assert(Scenario16(fixture, crabHouseNumberId, geometry1, geometry2, geometry3, geometry4, geometry5, geometry6, geometry7, geometry8, geometry9));
        }

        [Theory]
        [DefaultData]
        public void TestScenario17(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry1,
            WkbGeometry geometry2,
            WkbGeometry geometry3,
            WkbGeometry geometry4,
            WkbGeometry geometry5,
            WkbGeometry geometry6,
            WkbGeometry geometry7,
            WkbGeometry geometry8,
            WkbGeometry geometry9)
        {
            Assert(Scenario17(fixture, crabHouseNumberId, geometry1, geometry2, geometry3, geometry4, geometry5, geometry6, geometry7, geometry8, geometry9));
        }

        [Theory]
        [DefaultData]
        public void TestScenario18(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry1,
            WkbGeometry geometry2,
            WkbGeometry geometry3,
            WkbGeometry geometry4,
            WkbGeometry geometry5,
            WkbGeometry geometry6,
            WkbGeometry geometry7,
            WkbGeometry geometry8,
            WkbGeometry geometry9)
        {
            Assert(Scenario18(fixture, crabHouseNumberId, geometry1, geometry2, geometry3, geometry4, geometry5, geometry6, geometry7, geometry8, geometry9));
        }

        [Theory]
        [DefaultData]
        public void TestScenario19(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry1,
            WkbGeometry geometry2,
            WkbGeometry geometry3,
            WkbGeometry geometry4,
            WkbGeometry geometry5,
            WkbGeometry geometry6,
            WkbGeometry geometry7,
            WkbGeometry geometry8,
            WkbGeometry geometry9)
        {
            Assert(Scenario19(fixture, crabHouseNumberId, geometry1, geometry2, geometry3, geometry4, geometry5, geometry6, geometry7, geometry8, geometry9));
        }

        [Theory]
        [DefaultData]
        public void TestExtraScenarioRemoveMiddleQualitative(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry1,
            WkbGeometry geometry2,
            WkbGeometry geometry3,
            WkbGeometry geometry4,
            WkbGeometry geometry5,
            WkbGeometry geometry6,
            WkbGeometry geometry7,
            WkbGeometry geometry8,
            WkbGeometry geometry9)
        {
            Assert(ExtraScenarioRemoveMiddleQualitative(fixture, crabHouseNumberId, geometry1, geometry2, geometry3, geometry4, geometry5, geometry6, geometry7, geometry8, geometry9));
        }

        [Theory]
        [DefaultData]
        public void TestExtraScenarioRemoveMostQualitative(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry1,
            WkbGeometry geometry2,
            WkbGeometry geometry3,
            WkbGeometry geometry4,
            WkbGeometry geometry5,
            WkbGeometry geometry6,
            WkbGeometry geometry7,
            WkbGeometry geometry8,
            WkbGeometry geometry9)
        {
            Assert(ExtraScenarioRemoveMostQualitative(fixture, crabHouseNumberId, geometry1, geometry2, geometry3, geometry4, geometry5, geometry6, geometry7, geometry8, geometry9));
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
                new CrabAddressPositionId(212713),
                crabHouseNumberId, geometry,
                new CrabAddressNature("2"),
                Enum.Parse<CrabAddressPositionOrigin>("10"),
                new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Parse("1830-01-01T00:00:00")), null),
                new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Parse("2011-04-29T14:51:53.840"))),
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
            WkbGeometry geometry,
            WkbGeometry geometry2
        )
        {
            var addressId = new AddressId(crabHouseNumberId.CreateDeterministicId());

            var command = new ImportHouseNumberPositionFromCrab(
                new CrabAddressPositionId(212713),
                crabHouseNumberId,
                geometry2,
                new CrabAddressNature("2"),
                Enum.Parse<CrabAddressPositionOrigin>("11"),
                new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Parse("1830-01-01T00:00:00")), null),
                new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Parse("2011-08-05T17:04:10.447"))),
                new CrabOperator("VLM\\thijsge"),
                CrabModification.Correction,
                Enum.Parse<CrabOrganisation>("5"));

            return new AutoFixtureScenario(fixture)
                .Given(Scenario1(fixture, crabHouseNumberId, geometry))
                .When(command)
                .Then(addressId,
                    new AddressPositionWasCorrected(addressId, new AddressGeometry(GeometryMethod.DerivedFromObject, GeometrySpecification.Parcel, GeometryHelpers.CreateEwkbFrom(geometry2))),
                    command.ToLegacyEvent());
        }

        private IEventCentricTestSpecificationBuilder Scenario3(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry,
            WkbGeometry geometry2,
            WkbGeometry geometry3
        )
        {
            var addressId = new AddressId(crabHouseNumberId.CreateDeterministicId());

            var command = new ImportHouseNumberPositionFromCrab(
                new CrabAddressPositionId(212713),
                crabHouseNumberId,
                geometry3,
                new CrabAddressNature("2"),
                Enum.Parse<CrabAddressPositionOrigin>("12"),
                new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Parse("1830-01-01T00:00:00")), null),
                new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Parse("2011-09-15T04:47:56.527"))),
                new CrabOperator("VLM\\CRABSSISservice"),
                CrabModification.Correction,
                Enum.Parse<CrabOrganisation>("5"));

            return new AutoFixtureScenario(fixture)
                .Given(Scenario2(fixture, crabHouseNumberId, geometry, geometry2))
                .When(command)
                .Then(addressId,
                    new AddressPositionWasCorrected(addressId, new AddressGeometry(GeometryMethod.DerivedFromObject, GeometrySpecification.Parcel, GeometryHelpers.CreateEwkbFrom(geometry3))),
                    command.ToLegacyEvent());
        }

        private IEventCentricTestSpecificationBuilder Scenario4(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry,
            WkbGeometry geometry2,
            WkbGeometry geometry3,
            WkbGeometry geometry4
        )
        {
            var addressId = new AddressId(crabHouseNumberId.CreateDeterministicId());

            var command = new ImportHouseNumberPositionFromCrab(
                new CrabAddressPositionId(212713),
                crabHouseNumberId,
                geometry4,
                new CrabAddressNature("2"),
                Enum.Parse<CrabAddressPositionOrigin>("11"),
                new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Parse("1830-01-01T00:00:00")), null),
                new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Parse("2011-11-29T11:31:05.077"))),
                new CrabOperator("VLM\\thijsge"),
                CrabModification.Correction,
                Enum.Parse<CrabOrganisation>("5"));

            return new AutoFixtureScenario(fixture)
                .Given(Scenario3(fixture, crabHouseNumberId, geometry, geometry2, geometry3))
                .When(command)
                .Then(addressId,
                    new AddressPositionWasCorrected(addressId, new AddressGeometry(GeometryMethod.DerivedFromObject, GeometrySpecification.Parcel, GeometryHelpers.CreateEwkbFrom(geometry4))),
                    command.ToLegacyEvent());
        }

        private IEventCentricTestSpecificationBuilder Scenario5(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry,
            WkbGeometry geometry2,
            WkbGeometry geometry3,
            WkbGeometry geometry4,
            WkbGeometry geometry5
        )
        {
            var addressId = new AddressId(crabHouseNumberId.CreateDeterministicId());

            var command = new ImportHouseNumberPositionFromCrab(
                new CrabAddressPositionId(212713),
                crabHouseNumberId,
                geometry5,
                new CrabAddressNature("2"),
                Enum.Parse<CrabAddressPositionOrigin>("12"),
                new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Parse("1830-01-01T00:00:00")), null),
                new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Parse("2014-09-24T04:24:35.043"))),
                new CrabOperator("VLM\\CRABSSISservice"),
                CrabModification.Correction,
                Enum.Parse<CrabOrganisation>("5"));

            return new AutoFixtureScenario(fixture)
                .Given(Scenario4(fixture, crabHouseNumberId, geometry, geometry2, geometry3, geometry4))
                .When(command)
                .Then(addressId,
                    new AddressPositionWasCorrected(addressId, new AddressGeometry(GeometryMethod.DerivedFromObject, GeometrySpecification.Parcel, GeometryHelpers.CreateEwkbFrom(geometry5))),
                    command.ToLegacyEvent());
        }

        private IEventCentricTestSpecificationBuilder Scenario6(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry,
            WkbGeometry geometry2,
            WkbGeometry geometry3,
            WkbGeometry geometry4,
            WkbGeometry geometry5,
            WkbGeometry geometry6
        )
        {
            var addressId = new AddressId(crabHouseNumberId.CreateDeterministicId());

            var command = new ImportHouseNumberPositionFromCrab(
                new CrabAddressPositionId(212713),
                crabHouseNumberId,
                geometry6,
                new CrabAddressNature("2"),
                Enum.Parse<CrabAddressPositionOrigin>("11"),
                new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Parse("1830-01-01T00:00:00")), null),
                new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Parse("2014-09-27T04:08:38.163"))),
                new CrabOperator("VLM\\CRABSSISservice"),
                CrabModification.Correction,
                Enum.Parse<CrabOrganisation>("5"));

            return new AutoFixtureScenario(fixture)
                .Given(Scenario5(fixture, crabHouseNumberId, geometry, geometry2, geometry3, geometry4, geometry5))
                .When(command)
                .Then(addressId,
                    new AddressPositionWasCorrected(addressId, new AddressGeometry(GeometryMethod.DerivedFromObject, GeometrySpecification.Parcel, GeometryHelpers.CreateEwkbFrom(geometry6))),
                    command.ToLegacyEvent());
        }

        private IEventCentricTestSpecificationBuilder Scenario7(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry,
            WkbGeometry geometry2,
            WkbGeometry geometry3,
            WkbGeometry geometry4,
            WkbGeometry geometry5,
            WkbGeometry geometry6
        )
        {
            var addressId = new AddressId(crabHouseNumberId.CreateDeterministicId());

            var command = new ImportHouseNumberPositionFromCrab(
                new CrabAddressPositionId(212713),
                crabHouseNumberId,
                geometry6,
                new CrabAddressNature("2"),
                Enum.Parse<CrabAddressPositionOrigin>("11"),
                new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Parse("1830-01-01T00:00:00")), null),
                new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Parse("2015-05-29T23:23:17.450"))),
                new CrabOperator("46021:496:Els Podevyn"),
                CrabModification.Delete,
                Enum.Parse<CrabOrganisation>("5"));

            return new AutoFixtureScenario(fixture)
                .Given(Scenario6(fixture, crabHouseNumberId, geometry, geometry2, geometry3, geometry4, geometry5, geometry6))
                .When(command)
                .Then(addressId,
                    new AddressPositionWasRemoved(addressId),
                    command.ToLegacyEvent());
        }

        private IEventCentricTestSpecificationBuilder Scenario8(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry,
            WkbGeometry geometry2,
            WkbGeometry geometry3,
            WkbGeometry geometry4,
            WkbGeometry geometry5,
            WkbGeometry geometry6,
            WkbGeometry geometry7
        )
        {
            var addressId = new AddressId(crabHouseNumberId.CreateDeterministicId());

            var command = new ImportHouseNumberPositionFromCrab(
                new CrabAddressPositionId(5625208),
                crabHouseNumberId,
                geometry7,
                new CrabAddressNature("2"),
                Enum.Parse<CrabAddressPositionOrigin>("3"),
                new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Parse("2013-07-31T00:00:00")), null),
                new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Parse("2015-05-30T08:21:05.760"))),
                new CrabOperator("46021:496:Els Podevyn"),
                CrabModification.Insert,
                Enum.Parse<CrabOrganisation>("1"));

            return new AutoFixtureScenario(fixture)
                .Given(Scenario7(fixture, crabHouseNumberId, geometry, geometry2, geometry3, geometry4, geometry5, geometry6))
                .When(command)
                .Then(addressId,
                    new AddressWasPositioned(addressId, new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.BuildingUnit, GeometryHelpers.CreateEwkbFrom(geometry7))),
                    command.ToLegacyEvent());
        }

        private IEventCentricTestSpecificationBuilder Scenario9(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry,
            WkbGeometry geometry2,
            WkbGeometry geometry3,
            WkbGeometry geometry4,
            WkbGeometry geometry5,
            WkbGeometry geometry6,
            WkbGeometry geometry7,
            WkbGeometry geometry8
        )
        {
            var addressId = new AddressId(crabHouseNumberId.CreateDeterministicId());

            var command = new ImportHouseNumberPositionFromCrab(
                new CrabAddressPositionId(5637110),
                crabHouseNumberId,
                geometry8,
                new CrabAddressNature("2"),
                Enum.Parse<CrabAddressPositionOrigin>("6"),
                new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Parse("2013-07-31T00:00:00")), null),
                new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Parse("2015-05-30T08:57:05.443"))),
                new CrabOperator("46021:496:Els Podevyn"),
                CrabModification.Insert,
                Enum.Parse<CrabOrganisation>("1"));

            return new AutoFixtureScenario(fixture)
                .Given(Scenario8(fixture, crabHouseNumberId, geometry, geometry2, geometry3, geometry4, geometry5, geometry6, geometry7))
                .When(command)
                .Then(addressId,
                    command.ToLegacyEvent());
        }

        private IEventCentricTestSpecificationBuilder Scenario10(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry,
            WkbGeometry geometry2,
            WkbGeometry geometry3,
            WkbGeometry geometry4,
            WkbGeometry geometry5,
            WkbGeometry geometry6,
            WkbGeometry geometry7,
            WkbGeometry geometry8,
            WkbGeometry geometry9
        )
        {
            var addressId = new AddressId(crabHouseNumberId.CreateDeterministicId());

            var command = new ImportHouseNumberPositionFromCrab(
                new CrabAddressPositionId(5647106),
                crabHouseNumberId,
                geometry9,
                new CrabAddressNature("2"),
                Enum.Parse<CrabAddressPositionOrigin>("7"),
                new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Parse("2013-07-31T00:00:00")), null),
                new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Parse("2015-05-30T09:27:20.750"))),
                new CrabOperator("46021:496:Els Podevyn"),
                CrabModification.Insert,
                Enum.Parse<CrabOrganisation>("1"));

            return new AutoFixtureScenario(fixture)
                .Given(Scenario9(fixture, crabHouseNumberId, geometry, geometry2, geometry3, geometry4, geometry5, geometry6, geometry7, geometry8))
                .When(command)
                .Then(addressId,
                    new AddressWasPositioned(addressId, new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Entry, GeometryHelpers.CreateEwkbFrom(geometry9))),
                    command.ToLegacyEvent());
        }

        private IEventCentricTestSpecificationBuilder Scenario11(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry,
            WkbGeometry geometry2,
            WkbGeometry geometry3,
            WkbGeometry geometry4,
            WkbGeometry geometry5,
            WkbGeometry geometry6,
            WkbGeometry geometry7,
            WkbGeometry geometry8,
            WkbGeometry geometry9
        )
        {
            var addressId = new AddressId(crabHouseNumberId.CreateDeterministicId());

            var command = new ImportHouseNumberPositionFromCrab(
                new CrabAddressPositionId(5647106),
                crabHouseNumberId,
                geometry9,
                new CrabAddressNature("2"),
                Enum.Parse<CrabAddressPositionOrigin>("7"),
                new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Parse("2013-07-31T00:00:00")), null),
                new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Parse("2015-06-01T18:10:18.960"))),
                new CrabOperator("VLM\\CRABSSISservice"),
                CrabModification.Correction,
                Enum.Parse<CrabOrganisation>("1"));

            return new AutoFixtureScenario(fixture)
                .Given(Scenario10(fixture, crabHouseNumberId, geometry, geometry2, geometry3, geometry4, geometry5, geometry6, geometry7, geometry8, geometry9))
                .When(command)
                .Then(addressId,
                    command.ToLegacyEvent());
        }

        private IEventCentricTestSpecificationBuilder Scenario12(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry,
            WkbGeometry geometry2,
            WkbGeometry geometry3,
            WkbGeometry geometry4,
            WkbGeometry geometry5,
            WkbGeometry geometry6,
            WkbGeometry geometry7,
            WkbGeometry geometry8,
            WkbGeometry geometry9
        )
        {
            var addressId = new AddressId(crabHouseNumberId.CreateDeterministicId());

            var command = new ImportHouseNumberPositionFromCrab(
                new CrabAddressPositionId(5637110),
                crabHouseNumberId,
                geometry8,
                new CrabAddressNature("2"),
                Enum.Parse<CrabAddressPositionOrigin>("6"),
                new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Parse("2013-07-31T00:00:00")), null),
                new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Parse("2015-06-01T18:10:18.960"))),
                new CrabOperator("VLM\\CRABSSISservice"),
                CrabModification.Correction,
                Enum.Parse<CrabOrganisation>("1"));

            return new AutoFixtureScenario(fixture)
                .Given(Scenario11(fixture, crabHouseNumberId, geometry, geometry2, geometry3, geometry4, geometry5, geometry6, geometry7, geometry8, geometry9))
                .When(command)
                .Then(addressId,
                    command.ToLegacyEvent());
        }

        private IEventCentricTestSpecificationBuilder Scenario13(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry,
            WkbGeometry geometry2,
            WkbGeometry geometry3,
            WkbGeometry geometry4,
            WkbGeometry geometry5,
            WkbGeometry geometry6,
            WkbGeometry geometry7,
            WkbGeometry geometry8,
            WkbGeometry geometry9
        )
        {
            var addressId = new AddressId(crabHouseNumberId.CreateDeterministicId());

            var command = new ImportHouseNumberPositionFromCrab(
                new CrabAddressPositionId(5625208),
                crabHouseNumberId,
                geometry7,
                new CrabAddressNature("2"),
                Enum.Parse<CrabAddressPositionOrigin>("3"),
                new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Parse("2013-07-31T00:00:00")), null),
                new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Parse("2015-06-01T18:10:18.960"))),
                new CrabOperator("VLM\\CRABSSISservice"),
                CrabModification.Correction,
                Enum.Parse<CrabOrganisation>("1"));

            return new AutoFixtureScenario(fixture)
                .Given(Scenario12(fixture, crabHouseNumberId, geometry, geometry2, geometry3, geometry4, geometry5, geometry6, geometry7, geometry8, geometry9))
                .When(command)
                .Then(addressId,
                    command.ToLegacyEvent());
        }

        private IEventCentricTestSpecificationBuilder Scenario14(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry,
            WkbGeometry geometry2,
            WkbGeometry geometry3,
            WkbGeometry geometry4,
            WkbGeometry geometry5,
            WkbGeometry geometry6,
            WkbGeometry geometry7,
            WkbGeometry geometry8,
            WkbGeometry geometry9
        )
        {
            var addressId = new AddressId(crabHouseNumberId.CreateDeterministicId());

            var command = new ImportHouseNumberPositionFromCrab(
                new CrabAddressPositionId(5647106),
                crabHouseNumberId,
                geometry9,
                new CrabAddressNature("2"),
                Enum.Parse<CrabAddressPositionOrigin>("7"),
                new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Parse("2013-07-31T00:00:00")), null),
                new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Parse("2015-06-08T18:11:49.423"))),
                new CrabOperator("VLM\\CRABSSISservice"),
                CrabModification.Correction,
                Enum.Parse<CrabOrganisation>("1"));

            return new AutoFixtureScenario(fixture)
                .Given(Scenario13(fixture, crabHouseNumberId, geometry, geometry2, geometry3, geometry4, geometry5, geometry6, geometry7, geometry8, geometry9))
                .When(command)
                .Then(addressId,
                    command.ToLegacyEvent());
        }

        private IEventCentricTestSpecificationBuilder Scenario15(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry,
            WkbGeometry geometry2,
            WkbGeometry geometry3,
            WkbGeometry geometry4,
            WkbGeometry geometry5,
            WkbGeometry geometry6,
            WkbGeometry geometry7,
            WkbGeometry geometry8,
            WkbGeometry geometry9
        )
        {
            var addressId = new AddressId(crabHouseNumberId.CreateDeterministicId());

            var command = new ImportHouseNumberPositionFromCrab(
                new CrabAddressPositionId(5637110),
                crabHouseNumberId,
                geometry8,
                new CrabAddressNature("2"),
                Enum.Parse<CrabAddressPositionOrigin>("6"),
                new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Parse("2013-07-31T00:00:00")), null),
                new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Parse("2015-06-08T18:11:49.423"))),
                new CrabOperator("VLM\\CRABSSISservice"),
                CrabModification.Correction,
                Enum.Parse<CrabOrganisation>("1"));

            return new AutoFixtureScenario(fixture)
                .Given(Scenario14(fixture, crabHouseNumberId, geometry, geometry2, geometry3, geometry4, geometry5, geometry6, geometry7, geometry8, geometry9))
                .When(command)
                .Then(addressId,
                    command.ToLegacyEvent());
        }

        private IEventCentricTestSpecificationBuilder Scenario16(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry,
            WkbGeometry geometry2,
            WkbGeometry geometry3,
            WkbGeometry geometry4,
            WkbGeometry geometry5,
            WkbGeometry geometry6,
            WkbGeometry geometry7,
            WkbGeometry geometry8,
            WkbGeometry geometry9
        )
        {
            var addressId = new AddressId(crabHouseNumberId.CreateDeterministicId());

            var command = new ImportHouseNumberPositionFromCrab(
                new CrabAddressPositionId(5625208),
                crabHouseNumberId,
                geometry7,
                new CrabAddressNature("2"),
                Enum.Parse<CrabAddressPositionOrigin>("3"),
                new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Parse("2013-07-31T00:00:00")), null),
                new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Parse("2015-06-08T18:11:49.423"))),
                new CrabOperator("VLM\\CRABSSISservice"),
                CrabModification.Correction,
                Enum.Parse<CrabOrganisation>("1"));

            return new AutoFixtureScenario(fixture)
                .Given(Scenario15(fixture, crabHouseNumberId, geometry, geometry2, geometry3, geometry4, geometry5, geometry6, geometry7, geometry8, geometry9))
                .When(command)
                .Then(addressId,
                    command.ToLegacyEvent());
        }

        private IEventCentricTestSpecificationBuilder Scenario17(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry,
            WkbGeometry geometry2,
            WkbGeometry geometry3,
            WkbGeometry geometry4,
            WkbGeometry geometry5,
            WkbGeometry geometry6,
            WkbGeometry geometry7,
            WkbGeometry geometry8,
            WkbGeometry geometry9
        )
        {
            var addressId = new AddressId(crabHouseNumberId.CreateDeterministicId());

            var command = new ImportHouseNumberPositionFromCrab(
                new CrabAddressPositionId(5625208),
                crabHouseNumberId,
                geometry7,
                new CrabAddressNature("2"),
                Enum.Parse<CrabAddressPositionOrigin>("3"),
                new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Parse("2013-07-31T00:00:00")), null),
                new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Parse("2015-06-09T17:28:26.697"))),
                new CrabOperator("VLM\\CRABSSISservice"),
                CrabModification.Correction,
                Enum.Parse<CrabOrganisation>("1"));

            return new AutoFixtureScenario(fixture)
                .Given(Scenario16(fixture, crabHouseNumberId, geometry, geometry2, geometry3, geometry4, geometry5, geometry6, geometry7, geometry8, geometry9))
                .When(command)
                .Then(addressId,
                    command.ToLegacyEvent());
        }

        private IEventCentricTestSpecificationBuilder Scenario18(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry,
            WkbGeometry geometry2,
            WkbGeometry geometry3,
            WkbGeometry geometry4,
            WkbGeometry geometry5,
            WkbGeometry geometry6,
            WkbGeometry geometry7,
            WkbGeometry geometry8,
            WkbGeometry geometry9
        )
        {
            var addressId = new AddressId(crabHouseNumberId.CreateDeterministicId());

            var command = new ImportHouseNumberPositionFromCrab(
                new CrabAddressPositionId(5637110),
                crabHouseNumberId,
                geometry8,
                new CrabAddressNature("2"),
                Enum.Parse<CrabAddressPositionOrigin>("6"),
                new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Parse("2013-07-31T00:00:00")), null),
                new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Parse("2015-06-09T17:28:26.697"))),
                new CrabOperator("VLM\\CRABSSISservice"),
                CrabModification.Correction,
                Enum.Parse<CrabOrganisation>("1"));

            return new AutoFixtureScenario(fixture)
                .Given(Scenario17(fixture, crabHouseNumberId, geometry, geometry2, geometry3, geometry4, geometry5, geometry6, geometry7, geometry8, geometry9))
                .When(command)
                .Then(addressId,
                    command.ToLegacyEvent());
        }

        private IEventCentricTestSpecificationBuilder Scenario19(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry,
            WkbGeometry geometry2,
            WkbGeometry geometry3,
            WkbGeometry geometry4,
            WkbGeometry geometry5,
            WkbGeometry geometry6,
            WkbGeometry geometry7,
            WkbGeometry geometry8,
            WkbGeometry geometry9
        )
        {
            var addressId = new AddressId(crabHouseNumberId.CreateDeterministicId());

            var command = new ImportHouseNumberPositionFromCrab(
                new CrabAddressPositionId(5647106),
                crabHouseNumberId,
                geometry9,
                new CrabAddressNature("2"),
                Enum.Parse<CrabAddressPositionOrigin>("7"),
                new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Parse("2013-07-31T00:00:00")), null),
                new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Parse("2015-06-09T17:28:26.697"))),
                new CrabOperator("VLM\\CRABSSISservice"),
                CrabModification.Correction,
                Enum.Parse<CrabOrganisation>("1"));

            return new AutoFixtureScenario(fixture)
                .Given(Scenario18(fixture, crabHouseNumberId, geometry, geometry2, geometry3, geometry4, geometry5, geometry6, geometry7, geometry8, geometry9))
                .When(command)
                .Then(addressId,
                    command.ToLegacyEvent());
        }

        private IEventCentricTestSpecificationBuilder ExtraScenarioRemoveMiddleQualitative(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry,
            WkbGeometry geometry2,
            WkbGeometry geometry3,
            WkbGeometry geometry4,
            WkbGeometry geometry5,
            WkbGeometry geometry6,
            WkbGeometry geometry7,
            WkbGeometry geometry8,
            WkbGeometry geometry9
        )
        {
            var addressId = new AddressId(crabHouseNumberId.CreateDeterministicId());

            var command = new ImportHouseNumberPositionFromCrab(
                new CrabAddressPositionId(5625208),
                crabHouseNumberId,
                geometry7,
                new CrabAddressNature("2"),
                Enum.Parse<CrabAddressPositionOrigin>("3"),
                new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Parse("2013-07-31T00:00:00")), null),
                new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Now)),
                new CrabOperator("AdresRegistryUnitTests"),
                CrabModification.Delete,
                Enum.Parse<CrabOrganisation>("1"));

            return new AutoFixtureScenario(fixture)
                .Given(Scenario19(fixture, crabHouseNumberId, geometry, geometry2, geometry3, geometry4, geometry5, geometry6, geometry7, geometry8, geometry9))
                .When(command)
                .Then(addressId,
                    command.ToLegacyEvent());
        }

        private IEventCentricTestSpecificationBuilder ExtraScenarioRemoveMostQualitative(Fixture fixture,
            CrabHouseNumberId crabHouseNumberId,
            WkbGeometry geometry,
            WkbGeometry geometry2,
            WkbGeometry geometry3,
            WkbGeometry geometry4,
            WkbGeometry geometry5,
            WkbGeometry geometry6,
            WkbGeometry geometry7,
            WkbGeometry geometry8,
            WkbGeometry geometry9
        )
        {
            var addressId = new AddressId(crabHouseNumberId.CreateDeterministicId());

            var command = new ImportHouseNumberPositionFromCrab(
                new CrabAddressPositionId(5647106),
                crabHouseNumberId,
                geometry9,
                new CrabAddressNature("2"),
                Enum.Parse<CrabAddressPositionOrigin>("7"),
                new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Parse("2013-07-31T00:00:00")), null),
                new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Now)),
                new CrabOperator("AdresRegistryUnitTests"),
                CrabModification.Delete,
                Enum.Parse<CrabOrganisation>("1"));

            return new AutoFixtureScenario(fixture)
                .Given(ExtraScenarioRemoveMiddleQualitative(fixture, crabHouseNumberId, geometry, geometry2, geometry3, geometry4, geometry5, geometry6, geometry7, geometry8, geometry9))
                .When(command)
                .Then(addressId,
                    new AddressPositionWasCorrected(addressId, new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.Parcel, GeometryHelpers.CreateEwkbFrom(geometry8))),
                    command.ToLegacyEvent());
        }
    }
}
