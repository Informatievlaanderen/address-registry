namespace AddressRegistry.Migrator.Address.Infrastructure.Modules
{
    using AddressRegistry.Infrastructure;
    using AddressRegistry.Infrastructure.Modules;
    using Api.BackOffice.Abstractions;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Autofac;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Autofac;
    using Consumer.Infrastructure.Modules;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

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
            var projectionsConnectionString = _configuration.GetConnectionString("BackOffice");
            _services
                .AddDbContext<BackOfficeContext>(options => options
                        .UseLoggerFactory(_loggerFactory)
                        .UseSqlServer(projectionsConnectionString, sqlServerOptions => sqlServerOptions
                            .EnableRetryOnFailure()
                            .MigrationsHistoryTable(MigrationTables.BackOffice, Schema.BackOffice))
                    , ServiceLifetime.Transient);

            builder
                .RegisterModule(new DataDogModule(_configuration))
                .RegisterModule<EnvelopeModule>()
                .RegisterModule(new ConsumerModule(_configuration, _services, _loggerFactory))
                .RegisterModule(new LegacyCommandHandlingModule(_configuration))
                .RegisterModule(new CommandHandlingModule(_configuration));

            builder.RegisterSnapshotModule(_configuration);

            builder.Populate(_services);
        }
    }
}
