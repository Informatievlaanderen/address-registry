namespace AddressRegistry.Projections.BackOffice.Infrastructure
{
    using System;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Sql.EntityFrameworkCore;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.SqlServer.MigrationExtensions;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using AddressRegistry.Infrastructure;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureBackOfficeProjectionsContext(
            this IServiceCollection services,
            IConfiguration configuration,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<BackOfficeProjectionsContext>();
            var connectionString = configuration.GetConnectionString("BackOfficeProjections");

            var hasConnectionString = !string.IsNullOrWhiteSpace(connectionString);
            if (hasConnectionString)
            {
                RunOnSqlServer(configuration, services, loggerFactory, connectionString);
            }
            else
            {
                RunInMemoryDb(services, loggerFactory, logger);
            }

            logger.LogInformation(
                "Added {Context} to services:" +
                Environment.NewLine +
                "\tSchema: {Schema}" +
                Environment.NewLine +
                "\tTableName: {TableName}",
                nameof(ConfigureBackOfficeProjectionsContext), Schema.BackOfficeProjections, MigrationTables.BackOfficeProjections);

            return services;
        }

        private static void RunOnSqlServer(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            string backOfficeProjectionsConnectionString)
        {
            services
                .AddScoped(s => new TraceDbConnection<BackOfficeProjectionsContext>(
                    new SqlConnection(backOfficeProjectionsConnectionString),
                    configuration["DataDog:ServiceName"]))
                .AddDbContext<BackOfficeProjectionsContext>((provider, options) => options
                    .UseLoggerFactory(loggerFactory)
                    .UseSqlServer(provider.GetRequiredService<TraceDbConnection<BackOfficeProjectionsContext>>(), sqlServerOptions =>
                    {
                        sqlServerOptions.EnableRetryOnFailure();
                        sqlServerOptions.MigrationsHistoryTable(MigrationTables.BackOfficeProjections, Schema.BackOfficeProjections);
                    })
                    .UseExtendedSqlServerMigrations());
        }

        private static void RunInMemoryDb(
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            ILogger logger)
        {
            services
                .AddDbContext<BackOfficeProjectionsContext>(options => options
                    .UseLoggerFactory(loggerFactory)
                    .UseInMemoryDatabase(Guid.NewGuid().ToString(), sqlServerOptions => { }));

            logger.LogWarning("Running InMemory for {Context}!", nameof(ConfigureBackOfficeProjectionsContext));
        }
    }
}
