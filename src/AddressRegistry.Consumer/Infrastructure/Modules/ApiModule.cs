namespace AddressRegistry.Consumer.Infrastructure.Modules
{
    using Projections;
    using AddressRegistry.Infrastructure.Modules;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Autofac;
    using Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
    using Be.Vlaanderen.Basisregisters.Projector;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Be.Vlaanderen.Basisregisters.Projector.Modules;
    using AddressRegistry.Infrastructure;

    public class ApiModule : Module
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceCollection _services;
        private readonly ILoggerFactory _loggerFactory;

        public ApiModule(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _services = services;
            _loggerFactory = loggerFactory;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterModule<EnvelopeModule>()
                .RegisterModule(new CommandHandlingModule(_configuration))
                .RegisterModule(new SequenceModule(_configuration, _services, _loggerFactory))
                .RegisterModule(new ProjectorModule(_configuration));

            builder.RegisterSnapshotModule(_configuration);

            builder
                .RegisterProjectionMigrator<ConsumerContextFactory>(_configuration, _loggerFactory)
                .RegisterProjections<StreetNameConsumerProjection, ConsumerContext>(
                    context => new StreetNameConsumerProjection(context.Resolve<ILoggerFactory>().CreateLogger<StreetNameConsumerProjection>()),
                    ConnectedProjectionSettings.Default);

            builder.Populate(_services);
        }
    }
}
