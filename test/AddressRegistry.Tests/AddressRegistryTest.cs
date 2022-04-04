namespace AddressRegistry.Tests
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.SqlStreamStore.Autofac;
    using global::AutoFixture;
    using Infrastructure.Modules;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using Xunit.Abstractions;

    public class AddressRegistryTest : AutofacBasedTest
    {
        protected Fixture Fixture { get; }
        protected JsonSerializerSettings EventSerializerSettings { get; } = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

        public AddressRegistryTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture = new Fixture();
            Fixture.Register(() => (ISnapshotStrategy)IntervalStrategy.Default);
        }

        protected override void ConfigureCommandHandling(ContainerBuilder builder)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string> { { "ConnectionStrings:Events", "x" } })
                .Build();

            builder
                .RegisterModule(new CommandHandlingModule(configuration))
                .RegisterModule(new SqlStreamStoreModule());
        }

        protected override void ConfigureEventHandling(ContainerBuilder builder)
        {
            var eventSerializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();
            builder.RegisterModule(new EventHandlingModule(typeof(DomainAssemblyMarker).Assembly, eventSerializerSettings));
        }
        public string GetSnapshotIdentifier(string identifier) => $"{identifier}-snapshots";
    }
}
