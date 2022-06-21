namespace AddressRegistry.Infrastructure.Modules
{
    using Autofac;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Infrastructure;
    using Microsoft.Extensions.Configuration;
    using StreetName;

    public class CommandHandlingModule : Module
    {
        public const string SnapshotIntervalKey = "SnapshotInterval";
        private readonly IConfiguration _configuration;

        public CommandHandlingModule(IConfiguration configuration)
            => _configuration = configuration;

        protected override void Load(ContainerBuilder containerBuilder)
        {
            var snapshotInterval = _configuration.GetValue<int?>(SnapshotIntervalKey) ?? 50;

            containerBuilder
                .Register(c => new StreetNameFactory(IntervalStrategy.SnapshotEvery(snapshotInterval)))
                .As<IStreetNameFactory>();

            containerBuilder
                .RegisterModule<RepositoriesModule>();

            containerBuilder
                .RegisterType<ConcurrentUnitOfWork>()
                .InstancePerLifetimeScope();

            containerBuilder
                .RegisterEventstreamModule(_configuration);

            CommandHandlerModules.Register(containerBuilder);

            containerBuilder
                .RegisterType<CommandHandlerResolver>()
                .As<ICommandHandlerResolver>();
        }
    }
}
