namespace AddressRegistry.Tests.Bugfixes
{
    using Address.Commands.Crab;
    using Address.Events;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.AspNetCore.Mvc.Formatters.Json;
    using Be.Vlaanderen.Basisregisters.Crab;
    using global::AutoFixture;
    using Microsoft.AspNetCore.Mvc.Formatters;
    using Newtonsoft.Json;
    using WhenImportHouseNumberSubaddressFromCrab;
    using WhenImportSubaddressFromCrab;
    using WhenImportSubaddressMailCantonFromCrab;
    using WhenImportSubaddressPositionFromCrab;
    using WhenImportSubaddressStatusFromCrab;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Genereated test, add thens to complete
    /// </summary>
    public class TestSubaddress_426001 : AddressRegistryTest
    {
        public TestSubaddress_426001(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _ = new TestSubaddress_426001Data();
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

        [Fact]
        public void Command011Test()
        {
            Assert(Command011());
        }

        [Fact]
        public void Command012Test()
        {
            Assert(Command012());
        }

        [Fact]
        public void Command013Test()
        {
            Assert(Command013());
        }

        [Fact]
        public void Command014Test()
        {
            Assert(Command014());
        }

        [Fact]
        public void Command015Test()
        {
            Assert(Command015());
        }

        [Fact]
        public void Command016Test()
        {
            Assert(Command016());
        }

        [Fact]
        public void Command017Test()
        {
            Assert(Command017());
        }

        [Fact]
        public void Command018Test()
        {
            Assert(Command018());
        }

        [Fact]
        public void Command019Test()
        {
            Assert(Command019());
        }

        [Fact]
        public void Command020Test()
        {
            Assert(Command020());
        }

        [Fact]
        public void Command021Test()
        {
            Assert(Command021());
        }

        [Fact]
        public void Command022Test()
        {
            Assert(Command022());
        }

        [Fact]
        public void Command023Test()
        {
            Assert(Command023());
        }

        [Fact]
        public void Command024Test()
        {
            Assert(Command024());
        }

        [Fact]
        public void Command025Test()
        {
            Assert(Command025());
        }

        [Fact]
        public void Command026Test()
        {
            Assert(Command026());
        }

        [Fact]
        public void Command027Test()
        {
            Assert(Command027());
        }

        [Fact]
        public void Command028Test()
        {
            Assert(Command028());
        }

        [Fact]
        public void Command029Test()
        {
            Assert(Command029());
        }

        private class TestSubaddress_426001Data
        {
            public string Id => new AddressId(new CrabSubaddressId(426001).CreateDeterministicId());
            public AddressId AddressId => new AddressId(new CrabSubaddressId(426001).CreateDeterministicId());

            public ImportHouseNumberSubaddressFromCrab Command001 =>
                JsonConvert.DeserializeObject<ImportHouseNumberSubaddressFromCrab>(
                    "{\"houseNumberId\":{\"value\":2525586},\"subaddressId\":{\"value\":426001},\"streetNameId\":{\"value\":8550},\"houseNumber\":{\"value\":\"27B\"},\"grbNotation\":{\"value\":null},\"lifetime\":{\"beginDateTime\":\"1830-01-01T00:00:00\",\"endDateTime\":null},\"timestamp\":{\"value\":\"2007-11-09T12:10:45.237Z\"},\"operator\":{\"value\":\"VLM\\\\dalewynsa\"},\"modification\":0,\"organisation\":4}");

            public ImportHouseNumberSubaddressFromCrab Command002 =>
                JsonConvert.DeserializeObject<ImportHouseNumberSubaddressFromCrab>(
                    "{\"houseNumberId\":{\"value\":2343242},\"subaddressId\":{\"value\":426001},\"streetNameId\":{\"value\":8550},\"houseNumber\":{\"value\":\"27\"},\"grbNotation\":{\"value\":\"27\"},\"lifetime\":{\"beginDateTime\":\"2005-01-01T00:00:00\",\"endDateTime\":null},\"timestamp\":{\"value\":\"2006-07-13T00:00:08.603Z\"},\"operator\":{\"value\":\"VLM\\\\thijsge\"},\"modification\":0,\"organisation\":2}");

            public ImportSubaddressMailCantonFromCrab Command003 =>
                JsonConvert.DeserializeObject<ImportSubaddressMailCantonFromCrab>(
                    "{\"houseNumberMailCantonId\":{\"value\":2314772},\"houseNumberId\":{\"value\":2343242},\"subaddressId\":{\"value\":426001},\"mailCantonId\":{\"value\":136},\"mailCantonCode\":{\"value\":\"2070\"},\"lifetime\":{\"beginDateTime\":\"2005-01-01T00:00:00\",\"endDateTime\":null},\"timestamp\":{\"value\":\"2006-07-13T01:17:18.617Z\"},\"operator\":{\"value\":\"VLM\\\\thijsge\"},\"modification\":0,\"organisation\":2}");

            public ImportSubaddressFromCrab Command004 =>
                JsonConvert.DeserializeObject<ImportSubaddressFromCrab>(
                    "{\"subaddressId\":{\"value\":426001},\"houseNumberId\":{\"value\":2525586},\"boxNumber\":{\"value\":\"5\"},\"boxNumberType\":{\"value\":\"2\"},\"lifetime\":{\"beginDateTime\":\"1830-01-01T00:00:00\",\"endDateTime\":null},\"timestamp\":{\"value\":\"2007-11-09T19:53:16.033Z\"},\"operator\":{\"value\":\"VLM\\\\dalewynsa\"},\"modification\":0,\"organisation\":4}");

            public ImportSubaddressMailCantonFromCrab Command005 =>
                JsonConvert.DeserializeObject<ImportSubaddressMailCantonFromCrab>(
                    "{\"houseNumberMailCantonId\":{\"value\":2496457},\"houseNumberId\":{\"value\":2525586},\"subaddressId\":{\"value\":426001},\"mailCantonId\":{\"value\":136},\"mailCantonCode\":{\"value\":\"2070\"},\"lifetime\":{\"beginDateTime\":\"1830-01-01T00:00:00\",\"endDateTime\":null},\"timestamp\":{\"value\":\"2007-11-09T21:25:55.15Z\"},\"operator\":{\"value\":\"VLM\\\\dalewynsa\"},\"modification\":0,\"organisation\":4}");

            public ImportHouseNumberSubaddressFromCrab Command006 =>
                JsonConvert.DeserializeObject<ImportHouseNumberSubaddressFromCrab>(
                    "{\"houseNumberId\":{\"value\":2343242},\"subaddressId\":{\"value\":426001},\"streetNameId\":{\"value\":8550},\"houseNumber\":{\"value\":\"27\"},\"grbNotation\":{\"value\":\"27\"},\"lifetime\":{\"beginDateTime\":\"2005-01-01T00:00:00\",\"endDateTime\":null},\"timestamp\":{\"value\":\"2008-06-11T08:45:31.25Z\"},\"operator\":{\"value\":\"VLM\\\\thijsge\"},\"modification\":2,\"organisation\":0}");

            public ImportHouseNumberSubaddressFromCrab Command007 =>
                JsonConvert.DeserializeObject<ImportHouseNumberSubaddressFromCrab>(
                    "{\"houseNumberId\":{\"value\":2525586},\"subaddressId\":{\"value\":426001},\"streetNameId\":{\"value\":8550},\"houseNumber\":{\"value\":\"27B\"},\"grbNotation\":{\"value\":null},\"lifetime\":{\"beginDateTime\":\"1830-01-01T00:00:00\",\"endDateTime\":null},\"timestamp\":{\"value\":\"2008-06-11T08:45:31.25Z\"},\"operator\":{\"value\":\"VLM\\\\thijsge\"},\"modification\":2,\"organisation\":0}");

            public ImportSubaddressMailCantonFromCrab Command008 =>
                JsonConvert.DeserializeObject<ImportSubaddressMailCantonFromCrab>(
                    "{\"houseNumberMailCantonId\":{\"value\":2314772},\"houseNumberId\":{\"value\":2343242},\"subaddressId\":{\"value\":426001},\"mailCantonId\":{\"value\":136},\"mailCantonCode\":{\"value\":\"2070\"},\"lifetime\":{\"beginDateTime\":\"2005-01-01T00:00:00\",\"endDateTime\":null},\"timestamp\":{\"value\":\"2008-06-11T08:45:45.61Z\"},\"operator\":{\"value\":\"VLM\\\\thijsge\"},\"modification\":2,\"organisation\":6}");

            public ImportSubaddressMailCantonFromCrab Command009 =>
                JsonConvert.DeserializeObject<ImportSubaddressMailCantonFromCrab>(
                    "{\"houseNumberMailCantonId\":{\"value\":2496457},\"houseNumberId\":{\"value\":2525586},\"subaddressId\":{\"value\":426001},\"mailCantonId\":{\"value\":136},\"mailCantonCode\":{\"value\":\"2070\"},\"lifetime\":{\"beginDateTime\":\"1830-01-01T00:00:00\",\"endDateTime\":null},\"timestamp\":{\"value\":\"2008-06-11T08:45:45.61Z\"},\"operator\":{\"value\":\"VLM\\\\thijsge\"},\"modification\":2,\"organisation\":6}");

            public ImportHouseNumberSubaddressFromCrab Command010 =>
                JsonConvert.DeserializeObject<ImportHouseNumberSubaddressFromCrab>(
                    "{\"houseNumberId\":{\"value\":2343242},\"subaddressId\":{\"value\":426001},\"streetNameId\":{\"value\":8550},\"houseNumber\":{\"value\":\"27\"},\"grbNotation\":{\"value\":\"27\"},\"lifetime\":{\"beginDateTime\":\"2005-01-01T00:00:00\",\"endDateTime\":null},\"timestamp\":{\"value\":\"2008-06-11T09:22:41.967Z\"},\"operator\":{\"value\":\"VLM\\\\thijsge\"},\"modification\":2,\"organisation\":4}");

            public ImportHouseNumberSubaddressFromCrab Command011 =>
                JsonConvert.DeserializeObject<ImportHouseNumberSubaddressFromCrab>(
                    "{\"houseNumberId\":{\"value\":2525586},\"subaddressId\":{\"value\":426001},\"streetNameId\":{\"value\":8550},\"houseNumber\":{\"value\":\"27B\"},\"grbNotation\":{\"value\":null},\"lifetime\":{\"beginDateTime\":\"1830-01-01T00:00:00\",\"endDateTime\":null},\"timestamp\":{\"value\":\"2008-06-11T09:22:41.967Z\"},\"operator\":{\"value\":\"VLM\\\\thijsge\"},\"modification\":2,\"organisation\":4}");

            public ImportSubaddressMailCantonFromCrab Command012 =>
                JsonConvert.DeserializeObject<ImportSubaddressMailCantonFromCrab>(
                    "{\"houseNumberMailCantonId\":{\"value\":2314772},\"houseNumberId\":{\"value\":2343242},\"subaddressId\":{\"value\":426001},\"mailCantonId\":{\"value\":136},\"mailCantonCode\":{\"value\":\"2070\"},\"lifetime\":{\"beginDateTime\":\"2005-01-01T00:00:00\",\"endDateTime\":null},\"timestamp\":{\"value\":\"2008-06-11T09:22:52.827Z\"},\"operator\":{\"value\":\"VLM\\\\thijsge\"},\"modification\":2,\"organisation\":4}");

            public ImportSubaddressMailCantonFromCrab Command013 =>
                JsonConvert.DeserializeObject<ImportSubaddressMailCantonFromCrab>(
                    "{\"houseNumberMailCantonId\":{\"value\":2496457},\"houseNumberId\":{\"value\":2525586},\"subaddressId\":{\"value\":426001},\"mailCantonId\":{\"value\":136},\"mailCantonCode\":{\"value\":\"2070\"},\"lifetime\":{\"beginDateTime\":\"1830-01-01T00:00:00\",\"endDateTime\":null},\"timestamp\":{\"value\":\"2008-06-11T09:22:52.827Z\"},\"operator\":{\"value\":\"VLM\\\\thijsge\"},\"modification\":2,\"organisation\":4}");

            public ImportHouseNumberSubaddressFromCrab Command014 =>
                JsonConvert.DeserializeObject<ImportHouseNumberSubaddressFromCrab>(
                    "{\"houseNumberId\":{\"value\":2343242},\"subaddressId\":{\"value\":426001},\"streetNameId\":{\"value\":8550},\"houseNumber\":{\"value\":\"27\"},\"grbNotation\":{\"value\":null},\"lifetime\":{\"beginDateTime\":\"1830-01-01T00:00:00\",\"endDateTime\":null},\"timestamp\":{\"value\":\"2010-06-11T11:25:02.417Z\"},\"operator\":{\"value\":\"gis@zwijndrecht.be\"},\"modification\":2,\"organisation\":0}");

            public ImportSubaddressFromCrab Command015 =>
                JsonConvert.DeserializeObject<ImportSubaddressFromCrab>(
                    "{\"subaddressId\":{\"value\":426001},\"houseNumberId\":{\"value\":2343242},\"boxNumber\":{\"value\":\"5\"},\"boxNumberType\":{\"value\":\"2\"},\"lifetime\":{\"beginDateTime\":\"1830-01-01T00:00:00\",\"endDateTime\":null},\"timestamp\":{\"value\":\"2010-06-11T11:25:03.39Z\"},\"operator\":{\"value\":\"gis@zwijndrecht.be\"},\"modification\":2,\"organisation\":0}");

            public ImportSubaddressStatusFromCrab Command016 =>
                JsonConvert.DeserializeObject<ImportSubaddressStatusFromCrab>(
                    "{\"subaddressStatusId\":{\"value\":105865},\"subaddressId\":{\"value\":426001},\"addressStatus\":2,\"lifetime\":{\"beginDateTime\":\"1830-01-01T00:00:00\",\"endDateTime\":null},\"timestamp\":{\"value\":\"2011-04-29T11:23:17.667Z\"},\"operator\":{\"value\":\"VLM\\\\thijsge\"},\"modification\":0,\"organisation\":4}");

            public ImportSubaddressMailCantonFromCrab Command017 =>
                JsonConvert.DeserializeObject<ImportSubaddressMailCantonFromCrab>(
                    "{\"houseNumberMailCantonId\":{\"value\":2496457},\"houseNumberId\":{\"value\":2525586},\"subaddressId\":{\"value\":426001},\"mailCantonId\":{\"value\":136},\"mailCantonCode\":{\"value\":\"2070\"},\"lifetime\":{\"beginDateTime\":\"1830-01-01T00:00:00\",\"endDateTime\":null},\"timestamp\":{\"value\":\"2012-03-05T10:30:15.983Z\"},\"operator\":{\"value\":\"Zwijndrecht:gis@zwijndrecht.be\"},\"modification\":1,\"organisation\":0}");

            public ImportHouseNumberSubaddressFromCrab Command018 =>
                JsonConvert.DeserializeObject<ImportHouseNumberSubaddressFromCrab>(
                    "{\"houseNumberId\":{\"value\":2525586},\"subaddressId\":{\"value\":426001},\"streetNameId\":{\"value\":8550},\"houseNumber\":{\"value\":\"27B\"},\"grbNotation\":{\"value\":null},\"lifetime\":{\"beginDateTime\":\"1830-01-01T00:00:00\",\"endDateTime\":null},\"timestamp\":{\"value\":\"2012-03-05T10:30:17.737Z\"},\"operator\":{\"value\":\"Zwijndrecht:gis@zwijndrecht.be\"},\"modification\":1,\"organisation\":0}");

            public ImportSubaddressPositionFromCrab Command019 =>
                JsonConvert.DeserializeObject<ImportSubaddressPositionFromCrab>(
                    "{\"addressPositionId\":{\"value\":3048114},\"subaddressId\":{\"value\":426001},\"addressPosition\":{\"value\":\"AQEAAAAUrkfhGhgCQcP1KFxHswlB\"},\"addressNature\":{\"value\":\"1\"},\"addressPositionOrigin\":10,\"lifetime\":{\"beginDateTime\":\"1830-01-01T00:00:00\",\"endDateTime\":null},\"timestamp\":{\"value\":\"2012-03-27T23:00:18.67Z\"},\"operator\":{\"value\":\"VLM\\\\thijsge\"},\"modification\":2,\"organisation\":4}");

            public ImportSubaddressPositionFromCrab Command020 =>
                JsonConvert.DeserializeObject<ImportSubaddressPositionFromCrab>(
                    "{\"addressPositionId\":{\"value\":3048114},\"subaddressId\":{\"value\":426001},\"addressPosition\":{\"value\":\"AQEAAAAUrkfhGhgCQcP1KFxHswlB\"},\"addressNature\":{\"value\":\"1\"},\"addressPositionOrigin\":10,\"lifetime\":{\"beginDateTime\":\"1830-01-01T00:00:00\",\"endDateTime\":null},\"timestamp\":{\"value\":\"2012-03-27T23:00:18.67Z\"},\"operator\":{\"value\":\"VLM\\\\thijsge\"},\"modification\":2,\"organisation\":4}");

            public ImportHouseNumberSubaddressFromCrab Command021 =>
                JsonConvert.DeserializeObject<ImportHouseNumberSubaddressFromCrab>(
                    "{\"houseNumberId\":{\"value\":2343242},\"subaddressId\":{\"value\":426001},\"streetNameId\":{\"value\":8550},\"houseNumber\":{\"value\":\"27\"},\"grbNotation\":{\"value\":null},\"lifetime\":{\"beginDateTime\":\"1830-01-01T00:00:00\",\"endDateTime\":null},\"timestamp\":{\"value\":\"2012-09-25T15:47:36.523Z\"},\"operator\":{\"value\":\"VLM\\\\thijsge\"},\"modification\":2,\"organisation\":0}");

            public ImportSubaddressMailCantonFromCrab Command022 =>
                JsonConvert.DeserializeObject<ImportSubaddressMailCantonFromCrab>(
                    "{\"houseNumberMailCantonId\":{\"value\":2314772},\"houseNumberId\":{\"value\":2343242},\"subaddressId\":{\"value\":426001},\"mailCantonId\":{\"value\":136},\"mailCantonCode\":{\"value\":\"2070\"},\"lifetime\":{\"beginDateTime\":\"2005-01-01T00:00:00\",\"endDateTime\":null},\"timestamp\":{\"value\":\"2012-09-25T15:48:03.847Z\"},\"operator\":{\"value\":\"VLM\\\\thijsge\"},\"modification\":2,\"organisation\":0}");

            public ImportSubaddressFromCrab Command023 =>
                JsonConvert.DeserializeObject<ImportSubaddressFromCrab>(
                    "{\"subaddressId\":{\"value\":426001},\"houseNumberId\":{\"value\":2343242},\"boxNumber\":{\"value\":\"5\"},\"boxNumberType\":{\"value\":\"2\"},\"lifetime\":{\"beginDateTime\":\"1830-01-01T00:00:00\",\"endDateTime\":null},\"timestamp\":{\"value\":\"2012-09-25T15:53:11.64Z\"},\"operator\":{\"value\":\"VLM\\\\thijsge\"},\"modification\":2,\"organisation\":0}");

            public ImportHouseNumberSubaddressFromCrab Command024 =>
                JsonConvert.DeserializeObject<ImportHouseNumberSubaddressFromCrab>(
                    "{\"houseNumberId\":{\"value\":2343242},\"subaddressId\":{\"value\":426001},\"streetNameId\":{\"value\":8550},\"houseNumber\":{\"value\":\"27\"},\"grbNotation\":{\"value\":\"27\"},\"lifetime\":{\"beginDateTime\":\"1830-01-01T00:00:00\",\"endDateTime\":null},\"timestamp\":{\"value\":\"2013-04-10T16:52:32.763Z\"},\"operator\":{\"value\":\"VLM\\\\CRABSSISservice\"},\"modification\":2,\"organisation\":0}");

            public ImportSubaddressMailCantonFromCrab Command025 =>
                JsonConvert.DeserializeObject<ImportSubaddressMailCantonFromCrab>(
                    "{\"houseNumberMailCantonId\":{\"value\":2314772},\"houseNumberId\":{\"value\":2343242},\"subaddressId\":{\"value\":426001},\"mailCantonId\":{\"value\":136},\"mailCantonCode\":{\"value\":\"2070\"},\"lifetime\":{\"beginDateTime\":\"2005-01-01T00:00:00\",\"endDateTime\":null},\"timestamp\":{\"value\":\"2013-04-10T16:53:06.937Z\"},\"operator\":{\"value\":\"VLM\\\\CRABSSISservice\"},\"modification\":2,\"organisation\":0}");

            public ImportSubaddressFromCrab Command026 =>
                JsonConvert.DeserializeObject<ImportSubaddressFromCrab>(
                    "{\"subaddressId\":{\"value\":426001},\"houseNumberId\":{\"value\":2343242},\"boxNumber\":{\"value\":\"5\"},\"boxNumberType\":{\"value\":\"2\"},\"lifetime\":{\"beginDateTime\":\"1830-01-01T00:00:00\",\"endDateTime\":null},\"timestamp\":{\"value\":\"2013-04-10T16:53:40.993Z\"},\"operator\":{\"value\":\"VLM\\\\CRABSSISservice\"},\"modification\":2,\"organisation\":0}");

            public ImportSubaddressStatusFromCrab Command027 =>
                JsonConvert.DeserializeObject<ImportSubaddressStatusFromCrab>(
                    "{\"subaddressStatusId\":{\"value\":105865},\"subaddressId\":{\"value\":426001},\"addressStatus\":2,\"lifetime\":{\"beginDateTime\":\"1830-01-01T00:00:00\",\"endDateTime\":null},\"timestamp\":{\"value\":\"2013-04-10T16:53:47.483Z\"},\"operator\":{\"value\":\"VLM\\\\CRABSSISservice\"},\"modification\":2,\"organisation\":0}");

            public ImportSubaddressFromCrab Command028 =>
                JsonConvert.DeserializeObject<ImportSubaddressFromCrab>(
                    "{\"subaddressId\":{\"value\":426001},\"houseNumberId\":{\"value\":2343242},\"boxNumber\":{\"value\":\"5\"},\"boxNumberType\":{\"value\":\"2\"},\"lifetime\":{\"beginDateTime\":\"1830-01-01T00:00:00\",\"endDateTime\":null},\"timestamp\":{\"value\":\"2017-10-18T12:11:21.03Z\"},\"operator\":{\"value\":\"VLM\\\\godderisdr\"},\"modification\":2,\"organisation\":0}");

            public AssignPersistentLocalIdForCrabSubaddressId Command029 =>
                JsonConvert.DeserializeObject<AssignPersistentLocalIdForCrabSubaddressId>(
                    "{\"subaddressId\":{\"value\":426001},\"persistentLocalId\":{\"value\":\"4298295\"},\"assignmentDate\":{\"value\":\"2017-10-10T14:39:22.2052275Z\"}}");
        }

        private TestSubaddress_426001Data _ { get; }

        public IEventCentricTestSpecificationBuilder Command001() => new AutoFixtureScenario(new Fixture())
            .GivenNone()
            .When(_.Command001)
            .Then(_.Id,
                new AddressWasRegistered(_.AddressId, new StreetNameId(_.Command001.StreetNameId.CreateDeterministicId()), _.Command001.HouseNumber),
                _.Command001.ToLegacyEvent());

        public IEventCentricTestSpecificationBuilder Command002() => new AutoFixtureScenario(new Fixture())
            .Given(Command001())
            .When(_.Command002)
            .Then(_.Id,
                _.Command002.ToLegacyEvent());

        public IEventCentricTestSpecificationBuilder Command003() => new AutoFixtureScenario(new Fixture())
            .Given(Command002())
            .When(_.Command003)
            .Then(_.Id,
                _.Command003.ToLegacyEvent());

        public IEventCentricTestSpecificationBuilder Command004() => new AutoFixtureScenario(new Fixture())
            .Given(Command003())
            .When(_.Command004)
            .Then(_.Id,
                new AddressBoxNumberWasChanged(_.AddressId, _.Command004.BoxNumber),
                _.Command004.ToLegacyEvent());

        public IEventCentricTestSpecificationBuilder Command005() => new AutoFixtureScenario(new Fixture())
            .Given(Command004())
            .When(_.Command005)
            .Then(_.Id,
                new AddressPostalCodeWasChanged(_.AddressId, new PostalCode(_.Command003.MailCantonCode)),
                _.Command005.ToLegacyEvent());

        public IEventCentricTestSpecificationBuilder Command006() => new AutoFixtureScenario(new Fixture())
            .Given(Command005())
            .When(_.Command006)
            .Then(_.Id,
                _.Command006.ToLegacyEvent());

        public IEventCentricTestSpecificationBuilder Command007() => new AutoFixtureScenario(new Fixture())
            .Given(Command006())
            .When(_.Command007)
            .Then(_.Id,
                _.Command007.ToLegacyEvent());

        public IEventCentricTestSpecificationBuilder Command008() => new AutoFixtureScenario(new Fixture())
            .Given(Command007())
            .When(_.Command008)
            .Then(_.Id,
                _.Command008.ToLegacyEvent());

        public IEventCentricTestSpecificationBuilder Command009() => new AutoFixtureScenario(new Fixture())
            .Given(Command008())
            .When(_.Command009)
            .Then(_.Id,
                _.Command009.ToLegacyEvent());

        public IEventCentricTestSpecificationBuilder Command010() => new AutoFixtureScenario(new Fixture())
            .Given(Command009())
            .When(_.Command010)
            .Then(_.Id,
                _.Command010.ToLegacyEvent());

        public IEventCentricTestSpecificationBuilder Command011() => new AutoFixtureScenario(new Fixture())
            .Given(Command010())
            .When(_.Command011)
            .Then(_.Id,
                _.Command011.ToLegacyEvent());

        public IEventCentricTestSpecificationBuilder Command012() => new AutoFixtureScenario(new Fixture())
            .Given(Command011())
            .When(_.Command012)
            .Then(_.Id,
                _.Command012.ToLegacyEvent());

        public IEventCentricTestSpecificationBuilder Command013() => new AutoFixtureScenario(new Fixture())
            .Given(Command012())
            .When(_.Command013)
            .Then(_.Id,
                _.Command013.ToLegacyEvent());

        public IEventCentricTestSpecificationBuilder Command014() => new AutoFixtureScenario(new Fixture())
            .Given(Command013())
            .When(_.Command014)
            .Then(_.Id,
                _.Command014.ToLegacyEvent());

        public IEventCentricTestSpecificationBuilder Command015() => new AutoFixtureScenario(new Fixture())
            .Given(Command014())
            .When(_.Command015)
            .Then(_.Id,
                new AddressHouseNumberWasCorrected(_.AddressId, _.Command014.HouseNumber),
                _.Command015.ToLegacyEvent());

        public IEventCentricTestSpecificationBuilder Command016() => new AutoFixtureScenario(new Fixture())
            .Given(Command015())
            .When(_.Command016)
            .Then(_.Id,
                new AddressBecameCurrent(_.AddressId),
                new AddressWasOfficiallyAssigned(_.AddressId),
                _.Command016.ToLegacyEvent());

        public IEventCentricTestSpecificationBuilder Command017() => new AutoFixtureScenario(new Fixture())
            .Given(Command016())
            .When(_.Command017)
            .Then(_.Id,
                _.Command017.ToLegacyEvent());

        public IEventCentricTestSpecificationBuilder Command018() => new AutoFixtureScenario(new Fixture())
            .Given(Command017())
            .When(_.Command018)
            .Then(_.Id,
                _.Command018.ToLegacyEvent());

        public IEventCentricTestSpecificationBuilder Command019() => new AutoFixtureScenario(new Fixture())
            .Given(Command018())
            .When(_.Command019)
            .Then(_.Id,
                new AddressPositionWasCorrected(_.AddressId, new AddressGeometry(GeometryMethod.DerivedFromObject, GeometrySpecification.BuildingUnit, GeometryHelpers.CreateEwkbFrom(_.Command019.AddressPosition))),
                new AddressBecameComplete(_.AddressId),
                _.Command019.ToLegacyEvent());

        public IEventCentricTestSpecificationBuilder Command020() => new AutoFixtureScenario(new Fixture())
            .Given(Command019())
            .When(_.Command020)
            .Then(_.Id,
                _.Command020.ToLegacyEvent());

        public IEventCentricTestSpecificationBuilder Command021() => new AutoFixtureScenario(new Fixture())
            .Given(Command020())
            .When(_.Command021)
            .Then(_.Id,
                _.Command021.ToLegacyEvent());

        public IEventCentricTestSpecificationBuilder Command022() => new AutoFixtureScenario(new Fixture())
            .Given(Command021())
            .When(_.Command022)
            .Then(_.Id,
                _.Command022.ToLegacyEvent());

        public IEventCentricTestSpecificationBuilder Command023() => new AutoFixtureScenario(new Fixture())
            .Given(Command022())
            .When(_.Command023)
            .Then(_.Id,
                _.Command023.ToLegacyEvent());

        public IEventCentricTestSpecificationBuilder Command024() => new AutoFixtureScenario(new Fixture())
            .Given(Command023())
            .When(_.Command024)
            .Then(_.Id,
                _.Command024.ToLegacyEvent());

        public IEventCentricTestSpecificationBuilder Command025() => new AutoFixtureScenario(new Fixture())
            .Given(Command024())
            .When(_.Command025)
            .Then(_.Id,
                _.Command025.ToLegacyEvent());

        public IEventCentricTestSpecificationBuilder Command026() => new AutoFixtureScenario(new Fixture())
            .Given(Command025())
            .When(_.Command026)
            .Then(_.Id,
                _.Command026.ToLegacyEvent());

        public IEventCentricTestSpecificationBuilder Command027() => new AutoFixtureScenario(new Fixture())
            .Given(Command026())
            .When(_.Command027)
            .Then(_.Id,
                _.Command027.ToLegacyEvent());

        public IEventCentricTestSpecificationBuilder Command028() => new AutoFixtureScenario(new Fixture())
            .Given(Command027())
            .When(_.Command028)
            .Then(_.Id,
                _.Command028.ToLegacyEvent());

        public IEventCentricTestSpecificationBuilder Command029() => new AutoFixtureScenario(new Fixture())
            .Given(Command028())
            .When(_.Command029)
            .Then(_.Id,
                //add thens here
                _.Command029.ToLegacyEvent());
    }
}
