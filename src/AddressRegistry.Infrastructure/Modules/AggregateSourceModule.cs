namespace AddressRegistry.Infrastructure.Modules
{
    using System;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
    using Microsoft.Extensions.Configuration;
    using StreetName;

    public class AggregateSourceModule : Module
    {
        public const string SnapshotIntervalKey = "SnapshotInterval";
        private readonly IConfiguration _configuration;

        public AggregateSourceModule(IConfiguration configuration)
            => _configuration = configuration;

        protected override void Load(ContainerBuilder builder)
        {
            var eventSerializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

            var value = _configuration[SnapshotIntervalKey] ?? "50";
            var snapshotInterval = Convert.ToInt32(value);

            ISnapshotStrategy snapshotStrategy = NoSnapshotStrategy.Instance;
            if (snapshotInterval > 0)
            {
                snapshotStrategy = IntervalStrategy.SnapshotEvery(snapshotInterval);
            }

            builder
                .Register(c => new StreetNameFactory(snapshotStrategy))
                .As<IStreetNameFactory>();

            builder
                .RegisterModule<RepositoriesModule>()
                .RegisterModule(new EventHandlingModule(typeof(DomainAssemblyMarker).Assembly, eventSerializerSettings))
                .RegisterEventstreamModule(_configuration);

            builder
                .RegisterType<ConcurrentUnitOfWork>()
                .InstancePerLifetimeScope();
        }
    }
}
