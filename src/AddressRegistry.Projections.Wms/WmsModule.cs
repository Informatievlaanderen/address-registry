namespace AddressRegistry.Projections.Wms
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

    public class WmsModule : Module
    {
        public WmsModule(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<WmsModule>();
            var connectionString = configuration.GetConnectionString("WmsProjections");

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
                nameof(WmsContext), Schema.Wms, MigrationTables.Wms);
        }

        private static void RunOnSqlServer(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            string backofficeProjectionsConnectionString)
        {
            services
                .AddScoped(s => new TraceDbConnection<WmsContext>(
                    new SqlConnection(backofficeProjectionsConnectionString),
                    configuration["DataDog:ServiceName"]))
                .AddDbContext<WmsContext>((provider, options) => options
                    .UseLoggerFactory(loggerFactory)
                    .UseSqlServer(provider.GetRequiredService<TraceDbConnection<WmsContext>>(), sqlServerOptions =>
                    {
                        sqlServerOptions.EnableRetryOnFailure();
                        sqlServerOptions.MigrationsHistoryTable(MigrationTables.Wms, Schema.Wms);
                        sqlServerOptions.UseNetTopologySuite();
                    })
                    .UseExtendedSqlServerMigrations());
        }

        private static void RunInMemoryDb(
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            ILogger<WmsModule> logger)
        {
            services
                .AddDbContext<WmsContext>(options => options
                    .UseLoggerFactory(loggerFactory)
                    .UseInMemoryDatabase(Guid.NewGuid().ToString(), sqlServerOptions => { }));

            logger.LogWarning("Running InMemory for {Context}!", nameof(WmsContext));
        }

        protected override void Load(ContainerBuilder builder)
            => builder.RegisterType<RegistryAtomFeedReader>().As<IRegistryAtomFeedReader>();
    }
}
