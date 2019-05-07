namespace AddressRegistry.Api.CrabImport.Infrastructure.Modules
{
    using Address;
    using AddressRegistry.Address;
    using AddressRegistry.Infrastructure;
    using AddressRegistry.Infrastructure.Modules;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Autofac;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Api;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Autofac;
    using CrabImport;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Projections.Legacy;

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

        protected override void Load(ContainerBuilder containerBuilder)
        {
            var eventSerializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

            containerBuilder
                .RegisterModule(new DataDogModule(_configuration))
                .RegisterModule(new LegacyModule(_configuration, _services, _loggerFactory));

            containerBuilder
                .RegisterModule(new IdempotencyModule(
                    _services,
                    _configuration.GetSection(IdempotencyConfiguration.Section).Get<IdempotencyConfiguration>().ConnectionString,
                    new IdempotencyMigrationsTableInfo(Schema.Default),
                    new IdempotencyTableInfo(Schema.Default),
                    _loggerFactory));

            containerBuilder
                .RegisterType<IdempotentCommandHandlerModule>()
                .AsSelf();

            containerBuilder
                .RegisterType<IdempotentCommandHandlerModuleProcessor>()
                .As<IIdempotentCommandHandlerModuleProcessor>();

            containerBuilder
                .RegisterModule(new EventHandlingModule(typeof(DomainAssemblyMarker).Assembly, eventSerializerSettings));

            containerBuilder
                .RegisterModule(new EnvelopeModule());

            containerBuilder
                .RegisterModule(new CommandHandlingModule(_configuration));

            containerBuilder
                .RegisterType<IdempotentCommandHandlerModule>()
                .AsSelf();

            containerBuilder
                .RegisterType<IdempotentCommandHandlerModuleProcessor>()
                .As<IIdempotentCommandHandlerModuleProcessor>();

            var projectionsConnectionString = _configuration.GetConnectionString("Sequences");

            _services
                .AddDbContext<SequenceContext>(options => options
                    .UseLoggerFactory(_loggerFactory)
                    .UseSqlServer(projectionsConnectionString, sqlServerOptions =>
                    {
                        sqlServerOptions.EnableRetryOnFailure();
                        sqlServerOptions.MigrationsHistoryTable(MigrationTables.Sequence, Schema.Sequence);
                    }));

            containerBuilder
                .RegisterType<SqlOsloIdGenerator>()
                .As<IOsloIdGenerator>()
                .SingleInstance();

            containerBuilder.Populate(_services);
        }
    }
}
