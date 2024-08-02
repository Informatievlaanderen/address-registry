namespace AddressRegistry.Projections.Elastic.Infrastructure
{
    using System;
    using AddressRegistry.Infrastructure;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.SqlServer.MigrationExtensions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class ElasticRunnerModule : Module
    {
        public ElasticRunnerModule(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<ElasticRunnerModule>();
            var connectionString = configuration.GetConnectionString("ElasticProjectionsAdmin")!;

            var hasConnectionString = !string.IsNullOrWhiteSpace(connectionString);
            if (hasConnectionString)
                RunOnSqlServer(services, loggerFactory, connectionString);
            else
                RunInMemoryDb(services, loggerFactory, logger);
            
            logger.LogInformation(
                "Added {Context} to services:" +
                Environment.NewLine +
                "\tSchema: {Schema}" +
                Environment.NewLine +
                "\tTableName: {TableName}",
                nameof(ElasticRunnerContext), Schema.Legacy, MigrationTables.Legacy);
        }

        private static void RunOnSqlServer(
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            string legacyConnectionString)
        {
            services
                .AddDbContext<ElasticRunnerContext>((_, options) => options
                    .UseLoggerFactory(loggerFactory)
                    .UseSqlServer(legacyConnectionString, sqlServerOptions =>
                    {
                        sqlServerOptions.EnableRetryOnFailure();
                        sqlServerOptions.MigrationsHistoryTable(MigrationTables.Extract, Schema.Extract);
                    })
                    .UseExtendedSqlServerMigrations());
        }

        private static void RunInMemoryDb(
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            ILogger<ElasticRunnerModule> logger)
        {
            services
                .AddDbContext<ElasticRunnerContext>(options => options
                    .UseLoggerFactory(loggerFactory)
                    .UseInMemoryDatabase(Guid.NewGuid().ToString(), _ => { }));

            logger.LogWarning("Running InMemory for {Context}!", nameof(ElasticRunnerContext));
        }
    }
}
