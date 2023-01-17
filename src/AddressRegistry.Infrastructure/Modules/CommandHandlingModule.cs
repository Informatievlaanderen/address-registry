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

        protected override void Load(ContainerBuilder builder)
        {
            var snapshotInterval = _configuration.GetValue<int?>(SnapshotIntervalKey) ?? 50;

            builder
                .Register(c => new StreetNameFactory(IntervalStrategy.SnapshotEvery(snapshotInterval)))
                .As<IStreetNameFactory>();

            builder
                .RegisterModule<RepositoriesModule>();

            builder
                .RegisterType<ConcurrentUnitOfWork>()
                .InstancePerLifetimeScope();

            builder.RegisterEventstreamModule(_configuration);
            builder.RegisterSnapshotModule(_configuration);

            CommandHandlerModules.Register(builder);

            builder
                .RegisterType<CommandHandlerResolver>()
                .As<ICommandHandlerResolver>();
        }
    }
}
