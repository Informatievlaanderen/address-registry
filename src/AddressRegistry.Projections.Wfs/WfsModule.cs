namespace AddressRegistry.Projections.Wfs
{
    using System;
    using Microsoft.Data.SqlClient;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Sql.EntityFrameworkCore;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.MigrationExtensions;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Syndication;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class WfsModule : Module
    {
        public WfsModule(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<WfsModule>();
            var connectionString = configuration.GetConnectionString("WfsProjections");

            var hasConnectionString = !string.IsNullOrWhiteSpace(connectionString);
            if (hasConnectionString)
                RunOnSqlServer(configuration, services, loggerFactory, connectionString);
            else
                RunInMemoryDb(services, loggerFactory, logger);

            logger.LogInformation(
                "Added {Context} to services:" +
                Environment.NewLine +
                "\tSchema: {Schema}" +
                Environment.NewLine +
                "\tTableName: {TableName}",
                nameof(WfsContext), Schema.Wfs, MigrationTables.Wfs);
        }

        private static void RunOnSqlServer(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            string backofficeProjectionsConnectionString)
        {
            services
                .AddScoped(s => new TraceDbConnection<WfsContext>(
                    new SqlConnection(backofficeProjectionsConnectionString),
                    configuration["DataDog:ServiceName"]))
                .AddDbContext<WfsContext>((provider, options) => options
                    .UseLoggerFactory(loggerFactory)
                    .UseSqlServer(provider.GetRequiredService<TraceDbConnection<WfsContext>>(), sqlServerOptions =>
                    {
                        sqlServerOptions.EnableRetryOnFailure();
                        sqlServerOptions.MigrationsHistoryTable(MigrationTables.Wfs, Schema.Wfs);
                        sqlServerOptions.UseNetTopologySuite();
                    })
                    .UseExtendedSqlServerMigrations());
        }

        private static void RunInMemoryDb(
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            ILogger<WfsModule> logger)
        {
            services
                .AddDbContext<WfsContext>(options => options
                    .UseLoggerFactory(loggerFactory)
                    .UseInMemoryDatabase(Guid.NewGuid().ToString(), sqlServerOptions => { }));

            logger.LogWarning("Running InMemory for {Context}!", nameof(WfsContext));
        }

        protected override void Load(ContainerBuilder builder)
            => builder.RegisterType<RegistryAtomFeedReader>().As<IRegistryAtomFeedReader>();
    }
}
