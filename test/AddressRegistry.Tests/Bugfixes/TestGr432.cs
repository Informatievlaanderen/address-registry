namespace AddressRegistry.Tests.Bugfixes
{
    using Address.Commands.Crab;
    using Address.Events;
    using Address.Events.Crab;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using System;
    using Address;
    using AggregateTests.Legacy.WhenImportHouseNumberMailCantonFromCrab;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    ///     https://vlaamseoverheid.atlassian.net/browse/GR-432
    /// </summary>
    public class TestGr432 : AddressRegistryTest
    {
        private class TestGr432Data
        {
            private readonly Func<EventDeserializer> _eventDeserializerFactory;

            public TestGr432Data(Func<EventDeserializer> deserializerFactory)
            {
                _eventDeserializerFactory = deserializerFactory;
            }

            private T Deserialize<T>(string value) => (T)_eventDeserializerFactory().DeserializeObject(value, typeof(T));

            public AddressWasRegistered AddressWasRegistered
                => Deserialize<AddressWasRegistered>(
                    "{\"addressId\":\"138654b7-9dab-587d-acec-7b63ebdf327c\",\"streetNameId\":\"d471092c-c1ee-5d25-bc87-13863dc3de7a\",\"houseNumber\":\"7\",\"provenance\":{\"timestamp\":\"2002-08-13T15:32:32Z\",\"application\":\"unknown\",\"modification\":\"insert\",\"operator\":\"VLM\\thijsge\",\"organisation\":\"ngi\",\"plan\":\"centralManagementCrab\"}}");

            public AddressPostalCodeWasChanged AddressPostalCodeWasChanged2910
                => Deserialize<AddressPostalCodeWasChanged>(
                    "{\"addressId\":\"138654b7-9dab-587d-acec-7b63ebdf327c\",\"postalCode\":\"2910\",\"provenance\":{\"timestamp\":\"2011-04-29T12:50:10.483Z\",\"application\":\"unknown\",\"modification\":\"update\",\"operator\":\"VLM\\thijsge\",\"organisation\":\"agiv\",\"plan\":\"centralManagementCrab\"}}");

            public AddressHouseNumberMailCantonWasImportedFromCrab AddressHouseNumberMailCantonWasImportedFromCrab2910
                => Deserialize<AddressHouseNumberMailCantonWasImportedFromCrab>(
                    "{\"houseNumberMailCantonId\":160653,\"houseNumberId\":9,\"mailCantonId\":222,\"mailCantonCode\":\"2910\",\"beginDateTime\":\"1830-01-01T00:00:00\",\"endDateTime\":null,\"timestamp\":\"2003-12-08T02:12:23.64Z\",\"operator\":\"VLM\\\\goeminnedi\",\"modification\":\"insert\",\"organisation\":\"akred\",\"key\":160653}");

            public AddressPostalCodeWasChanged AddressPostalCodeWasChanged8880
                => Deserialize<AddressPostalCodeWasChanged>(
                    "{\"addressId\":\"138654b7-9dab-587d-acec-7b63ebdf327c\",\"postalCode\":\"8880\",\"provenance\":{\"timestamp\":\"2011-04-29T12:50:10.483Z\",\"application\":\"unknown\",\"modification\":\"update\",\"operator\":\"VLM\\thijsge\",\"organisation\":\"agiv\",\"plan\":\"centralManagementCrab\"}}");

            public AddressHouseNumberMailCantonWasImportedFromCrab AddressHouseNumberMailCantonWasImportedFromCrab8880
                => Deserialize<AddressHouseNumberMailCantonWasImportedFromCrab>(
                    "{\"houseNumberMailCantonId\":2310020,\"houseNumberId\":9,\"mailCantonId\":1012,\"mailCantonCode\":\"8880\",\"beginDateTime\":\"2004-06-30T00:00:00\",\"endDateTime\":null,\"timestamp\":\"2005-12-15T14:22:03.31Z\",\"operator\":\"VLM\\thijsge\",\"modification\":\"insert\",\"organisation\":\"municipality\",\"key\":2310020}");

            public ImportHouseNumberMailCantonFromCrab ImportHouseNumberMailCantonFromCrab
                => Deserialize<AddressHouseNumberMailCantonWasImportedFromCrab>(
                    "{\"houseNumberMailCantonId\":2310020,\"houseNumberId\":9,\"mailCantonId\":1012,\"mailCantonCode\":\"8880\",\"beginDateTime\":\"2004-06-30T00:00:00\",\"endDateTime\":null,\"timestamp\":\"2005-12-15T14:22:03.34Z\",\"operator\":\"VLM\\thijsge\",\"modification\":\"delete\",\"organisation\":null,\"key\":2310020}").ToCommand();
        }

        public TestGr432(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _ = new TestGr432Data(() => Container.Resolve<EventDeserializer>());
        }

        private TestGr432Data _ { get; }

        [Fact]
        public void PreviousPostalCodeShouldBecomeActiveIfCurrentDeleted()
        {
            var addressId = new AddressId(_.AddressWasRegistered.AddressId);
            Assert(new Scenario()
                .Given(addressId,
                    _.AddressWasRegistered,
                    _.AddressPostalCodeWasChanged2910,
                    _.AddressHouseNumberMailCantonWasImportedFromCrab2910,
                    _.AddressPostalCodeWasChanged8880,
                    _.AddressHouseNumberMailCantonWasImportedFromCrab8880)
                .When(_.ImportHouseNumberMailCantonFromCrab)
                .Then(addressId,
                    new AddressPostalCodeWasChanged(addressId, new PostalCode("2910")),
                    _.ImportHouseNumberMailCantonFromCrab.ToLegacyEvent()));
        }
    }
}
