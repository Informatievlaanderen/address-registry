namespace AddressRegistry.Producer
{
    using System;
    using AddressRegistry.Infrastructure;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.SqlServer.MigrationExtensions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class ProducerModule : Module
    {
        public ProducerModule(
             IConfiguration configuration,
             IServiceCollection services,
             ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<ProducerModule>();
            var connectionString = configuration.GetConnectionString("ProducerProjections");

            var hasConnectionString = !string.IsNullOrWhiteSpace(connectionString);
            if (hasConnectionString)
            {
                RunOnSqlServer(services, loggerFactory, connectionString);
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
                nameof(ProducerContext), Schema.Producer, MigrationTables.Producer);
        }

        private static void RunOnSqlServer(
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            string producerConnectionString)
        {
            services
                .AddDbContext<ProducerContext>((_, options) => options
                    .UseLoggerFactory(loggerFactory)
                    .UseSqlServer(producerConnectionString, sqlServerOptions =>
                    {
                        sqlServerOptions.EnableRetryOnFailure();
                        sqlServerOptions.MigrationsHistoryTable(MigrationTables.Producer, Schema.Producer);
                    })
                    .UseExtendedSqlServerMigrations());
        }

        private static void RunInMemoryDb(
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            ILogger logger)
        {
            services
                .AddDbContext<ProducerContext>(options => options
                    .UseLoggerFactory(loggerFactory)
                    .UseInMemoryDatabase(Guid.NewGuid().ToString(), sqlServerOptions => { }));

            logger.LogWarning("Running InMemory for {Context}!", nameof(ProducerContext));
        }
    }
}
