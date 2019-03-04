namespace AddressRegistry.Tests.Bugfixes
{
    using Address.Events;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.AspNetCore.Mvc.Formatters.Json;
    using Be.Vlaanderen.Basisregisters.Crab;
    using global::AutoFixture;
    using Microsoft.AspNetCore.Mvc.Formatters;
    using Newtonsoft.Json;
    using WhenImportHouseNumberFromCrab;
    using WhenImportHouseNumberMailCantonFromCrab;
    using WhenImportHouseNumberPositionFromCrab;
    using WhenImportHouseNumberStatusFromCrab;
    using Xunit;
    using Xunit.Abstractions;

    public class TestHouseNumber_4165810 : AddressRegistryTest
    {
        public TestHouseNumber_4165810(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _ = new TestHouseNumber_4165810Data();
            JsonConvert.DefaultSettings =
                () => JsonSerializerSettingsProvider.CreateSerializerSettings().ConfigureDefaultForApi();
        }

        [Fact]
        public void Command001Test()
        {
            Assert(Command001());
        }

        [Fact]
        public void Command002Test()
        {
            Assert(Command002());
        }

        [Fact]
        public void Command003Test()
        {
            Assert(Command003());
        }

        [Fact]
        public void Command004Test()
        {
            Assert(Command004());
        }

        [Fact]
        public void Command005Test()
        {
            Assert(Command005());
        }

        [Fact]
        public void Command006Test()
        {
            Assert(Command006());
        }

        [Fact]
        public void Command007Test()
        {
            Assert(Command007());
        }

        [Fact]
        public void Command008Test()
        {
            Assert(Command008());
        }

        [Fact]
        public void Command009Test()
        {
            Assert(Command009());
        }

        [Fact]
        public void Command010Test()
        {
            Assert(Command010());
        }

        /// <summary>
        ///Generated test, add thens to complete
        /// </summary>
        private class TestHouseNumber_4165810Data
        {
            public string Id => new AddressId(new CrabHouseNumberId(4165810).CreateDeterministicId());
            public AddressId AddressId => new AddressId(new CrabHouseNumberId(4165810).CreateDeterministicId());

            public AddressRegistry.Address.Commands.Crab.ImportHouseNumberFromCrab Command001 =>
                JsonConvert.DeserializeObject<Address.Commands.Crab.ImportHouseNumberFromCrab>(
                    "{\"houseNumberId\":{\"value\":4165810},\"streetNameId\":{\"value\":185338},\"houseNumber\":{\"value\":\"2\"},\"grbNotation\":{\"value\":null},\"lifetime\":{\"beginDateTime\":\"2017-03-03T00:00:00\",\"endDateTime\":null},\"timestamp\":{\"value\":\"2017-03-14T08:55:49.377Z\"},\"operator\":{\"value\":\"Crabadmin\"},\"modification\":0,\"organisation\":0}");

            public AddressRegistry.Address.Commands.Crab.ImportHouseNumberStatusFromCrab Command002 =>
                JsonConvert.DeserializeObject<Address.Commands.Crab.ImportHouseNumberStatusFromCrab>(
                    "{\"houseNumberStatusId\":{\"value\":3659041},\"houseNumberId\":{\"value\":4165810},\"addressStatus\":2,\"lifetime\":{\"beginDateTime\":\"2017-03-03T00:00:00\",\"endDateTime\":null},\"timestamp\":{\"value\":\"2017-03-14T08:55:49.377Z\"},\"operator\":{\"value\":\"Crabadmin\"},\"modification\":0,\"organisation\":0}");

            public AddressRegistry.Address.Commands.Crab.ImportHouseNumberMailCantonFromCrab Command003 =>
                JsonConvert.DeserializeObject<Address.Commands.Crab.ImportHouseNumberMailCantonFromCrab>(
                    "{\"houseNumberMailCantonId\":{\"value\":3763844},\"houseNumberId\":{\"value\":4165810},\"mailCantonId\":{\"value\":1077},\"mailCantonCode\":{\"value\":\"9402\"},\"lifetime\":{\"beginDateTime\":\"2017-03-03T00:00:00\",\"endDateTime\":null},\"timestamp\":{\"value\":\"2017-03-14T08:55:49.377Z\"},\"operator\":{\"value\":\"Crabadmin\"},\"modification\":0,\"organisation\":0}");

            public AddressRegistry.Address.Commands.Crab.ImportHouseNumberPositionFromCrab Command004 =>
                JsonConvert.DeserializeObject<Address.Commands.Crab.ImportHouseNumberPositionFromCrab>(
                    "{\"addressPositionId\":{\"value\":5978897},\"houseNumberId\":{\"value\":4165810},\"addressPosition\":{\"value\":\"AQEAAAD2KFyPYv3+QAAAAAA6fQRB\"},\"addressNature\":{\"value\":\"2\"},\"addressPositionOrigin\":4,\"lifetime\":{\"beginDateTime\":\"2017-03-03T00:00:00\",\"endDateTime\":null},\"timestamp\":{\"value\":\"2017-03-14T08:55:49.39Z\"},\"operator\":{\"value\":\"Crabadmin\"},\"modification\":0,\"organisation\":0}");

            public AddressRegistry.Address.Commands.Crab.ImportHouseNumberPositionFromCrab Command005 =>
                JsonConvert.DeserializeObject<Address.Commands.Crab.ImportHouseNumberPositionFromCrab>(
                    "{\"addressPositionId\":{\"value\":5978897},\"houseNumberId\":{\"value\":4165810},\"addressPosition\":{\"value\":\"AQEAAAD2KFyPYv3+QAAAAAA6fQRB\"},\"addressNature\":{\"value\":\"2\"},\"addressPositionOrigin\":4,\"lifetime\":{\"beginDateTime\":\"2017-03-03T00:00:00\",\"endDateTime\":\"2017-03-14T00:00:00\"},\"timestamp\":{\"value\":\"2017-03-14T08:57:36Z\"},\"operator\":{\"value\":\"Crabadmin\"},\"modification\":3,\"organisation\":0}");

            public AddressRegistry.Address.Commands.Crab.ImportHouseNumberStatusFromCrab Command006 =>
                JsonConvert.DeserializeObject<Address.Commands.Crab.ImportHouseNumberStatusFromCrab>(
                    "{\"houseNumberStatusId\":{\"value\":3659041},\"houseNumberId\":{\"value\":4165810},\"addressStatus\":2,\"lifetime\":{\"beginDateTime\":\"2017-03-03T00:00:00\",\"endDateTime\":null},\"timestamp\":{\"value\":\"2017-03-14T08:57:36.073Z\"},\"operator\":{\"value\":\"Crabadmin\"},\"modification\":3,\"organisation\":0}");

            public AddressRegistry.Address.Commands.Crab.ImportHouseNumberMailCantonFromCrab Command007 =>
                JsonConvert.DeserializeObject<Address.Commands.Crab.ImportHouseNumberMailCantonFromCrab>(
                    "{\"houseNumberMailCantonId\":{\"value\":3763844},\"houseNumberId\":{\"value\":4165810},\"mailCantonId\":{\"value\":1077},\"mailCantonCode\":{\"value\":\"9402\"},\"lifetime\":{\"beginDateTime\":\"2017-03-03T00:00:00\",\"endDateTime\":\"2017-03-14T00:00:00\"},\"timestamp\":{\"value\":\"2017-03-14T08:57:36.167Z\"},\"operator\":{\"value\":\"Crabadmin\"},\"modification\":3,\"organisation\":0}");

            public AddressRegistry.Address.Commands.Crab.ImportHouseNumberFromCrab Command008 =>
                JsonConvert.DeserializeObject<Address.Commands.Crab.ImportHouseNumberFromCrab>(
                    "{\"houseNumberId\":{\"value\":4165810},\"streetNameId\":{\"value\":185338},\"houseNumber\":{\"value\":\"2\"},\"grbNotation\":{\"value\":null},\"lifetime\":{\"beginDateTime\":\"2017-03-03T00:00:00\",\"endDateTime\":\"2017-03-14T00:00:00\"},\"timestamp\":{\"value\":\"2017-03-14T08:57:36.533Z\"},\"operator\":{\"value\":\"Crabadmin\"},\"modification\":3,\"organisation\":0}");

            public AddressRegistry.Address.Commands.Crab.ImportHouseNumberStatusFromCrab Command009 =>
                JsonConvert.DeserializeObject<Address.Commands.Crab.ImportHouseNumberStatusFromCrab>(
                    "{\"houseNumberStatusId\":{\"value\":3659041},\"houseNumberId\":{\"value\":4165810},\"addressStatus\":2,\"lifetime\":{\"beginDateTime\":\"2017-03-03T00:00:00\",\"endDateTime\":\"2017-03-14T00:00:00\"},\"timestamp\":{\"value\":\"2017-03-14T09:00:40.223Z\"},\"operator\":{\"value\":\"VLM\\\\vermandebe\"},\"modification\":1,\"organisation\":null}");

            public AddressRegistry.Address.Commands.Crab.AssignOsloIdForCrabHouseNumberId Command010 =>
                JsonConvert.DeserializeObject<Address.Commands.Crab.AssignOsloIdForCrabHouseNumberId>(
                    "{\"houseNumberId\":{\"value\":4165810},\"osloId\":{\"value\":\"3856411\"},\"assignmentDate\":{\"value\":\"2017-10-10T13:32:41.3233537Z\"}}");
        }
        private TestHouseNumber_4165810Data _ { get; }

        public IEventCentricTestSpecificationBuilder Command001()
        {
            return new AutoFixtureScenario(new Fixture())
                .GivenNone()
                .When(_.Command001)
                .Then(_.Id,
                    new AddressWasRegistered(_.AddressId, new StreetNameId(_.Command001.StreetNameId.CreateDeterministicId()), new HouseNumber(_.Command001.HouseNumber)),
                    _.Command001.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder Command002()
        {
            return new AutoFixtureScenario(new Fixture())
                .Given(Command001())
                .When(_.Command002)
                .Then(_.Id,
                    new AddressBecameCurrent(_.AddressId),
                    new AddressWasOfficiallyAssigned(_.AddressId),
                    _.Command002.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder Command003()
        {
            return new AutoFixtureScenario(new Fixture())
                .Given(Command002())
                .When(_.Command003)
                .Then(_.Id,
                    new AddressPostalCodeWasChanged(_.AddressId, new PostalCode(_.Command003.MailCantonCode)),
                    _.Command003.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder Command004()
        {
            return new AutoFixtureScenario(new Fixture())
                .Given(Command003())
                .When(_.Command004)
                .Then(_.Id,
                    new AddressWasPositioned(_.AddressId, new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.BuildingUnit, GeometryHelpers.CreateEwkbFrom(_.Command004.AddressPosition))),
                    new AddressBecameComplete(_.AddressId),
                    _.Command004.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder Command005()
        {
            return new AutoFixtureScenario(new Fixture())
                .Given(Command004())
                .When(_.Command005)
                .Then(_.Id,
                    new AddressPositionWasRemoved(_.AddressId),
                    new AddressBecameIncomplete(_.AddressId),
                    _.Command005.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder Command006()
        {
            return new AutoFixtureScenario(new Fixture())
                .Given(Command005())
                .When(_.Command006)
                .Then(_.Id,
                    _.Command006.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder Command007()
        {
            return new AutoFixtureScenario(new Fixture())
                .Given(Command006())
                .When(_.Command007)
                .Then(_.Id,
                    _.Command007.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder Command008()
        {
            return new AutoFixtureScenario(new Fixture())
                .Given(Command007())
                .When(_.Command008)
                .Then(_.Id,
                    new AddressWasRetired(_.AddressId),
                    new AddressWasPositioned(_.AddressId, new AddressGeometry(GeometryMethod.AppointedByAdministrator, GeometrySpecification.BuildingUnit, GeometryHelpers.CreateEwkbFrom(_.Command005.AddressPosition))),
                    new AddressBecameComplete(_.AddressId),
                    _.Command008.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder Command009()
        {
            return new AutoFixtureScenario(new Fixture())
                .Given(Command008())
                .When(_.Command009)
                .Then(_.Id,
                    new AddressOfficialAssignmentWasRemoved(_.AddressId),
                    new AddressBecameIncomplete(_.AddressId),
                    _.Command009.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder Command010()
        {
            return new AutoFixtureScenario(new Fixture())
                .Given(Command009())
                .When(_.Command010)
                .Then(_.Id,
                    new AddressOsloIdWasAssigned(_.AddressId, new OsloId(_.Command010.OsloId), new OsloAssignmentDate(_.Command010.AssignmentDate))
                    );
        }

    }
}
