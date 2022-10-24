namespace AddressRegistry.Producer.Snapshot.Oslo.Infrastructure.Modules
{
    using System;
    using System.Net.Http;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Autofac;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Autofac;
    using Be.Vlaanderen.Basisregisters.Projector;
    using Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
    using Be.Vlaanderen.Basisregisters.Projector.Modules;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using AddressRegistry.Infrastructure;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.SnapshotProducer;

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
            builder.RegisterModule(new DataDogModule(_configuration));

            RegisterProjectionSetup(builder);

            builder
                .RegisterType<ProblemDetailsHelper>()
                .AsSelf();

            builder.Populate(_services);
        }

        private void RegisterProjectionSetup(ContainerBuilder builder)
        {
            builder
                .RegisterModule(
                    new EventHandlingModule(
                        typeof(DomainAssemblyMarker).Assembly,
                        EventsJsonSerializerSettingsProvider.CreateSerializerSettings()))

                .RegisterModule<EnvelopeModule>()

                .RegisterEventstreamModule(_configuration)

                .RegisterModule(new ProjectorModule(_configuration));

            RegisterProjections(builder);
        }

        private void RegisterProjections(ContainerBuilder builder)
        {
            builder
                .RegisterModule(
                    new ProducerModule(
                        _configuration,
                        _services,
                        _loggerFactory));

            var connectedProjectionSettings = ConnectedProjectionSettings.Configure(x =>
            {
                x.ConfigureCatchUpPageSize(ConnectedProjectionSettings.Default.CatchUpPageSize);
                x.ConfigureCatchUpUpdatePositionMessageInterval(Convert.ToInt32(_configuration["CatchUpSaveInterval"]));
            });

            builder
                .RegisterProjectionMigrator<ProducerContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjections<ProducerProjections, ProducerContext>(c =>
                        new ProducerProjections(_configuration,
                            new SnapshotManager(
                                c.Resolve<ILoggerFactory>(),
                                c.Resolve<IOsloProxy>(),
                                    SnapshotManagerOptions.Create(
                                        _configuration["RetryPolicy:MaxRetryWaitIntervalSeconds"],
                                        _configuration["RetryPolicy:RetryBackoffFactor"]))),
                    connectedProjectionSettings);
        }
    }
}
