namespace AddressRegistry.Projections.Integration.Infrastructure
{
    using System;
    using AddressRegistry.Infrastructure;
    using Autofac;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Version;

    public class IntegrationModule : Module
    {
        public IntegrationModule(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<IntegrationModule>();
            services.AddScoped<IEventsRepository>(_ => new EventsRepository(configuration.GetConnectionString("events")));
            var connectionString = configuration.GetConnectionString("IntegrationProjections");

            var hasConnectionString = !string.IsNullOrWhiteSpace(connectionString);
            if (hasConnectionString)
                RunOnNpgSqlServer(services, connectionString);
            else
                RunInMemoryDb(services, loggerFactory, logger);

            logger.LogInformation(
                "Added {Context} to services:" +
                Environment.NewLine +
                "\tSchema: {Schema}" +
                Environment.NewLine +
                "\tTableName: {TableName}",
                nameof(IntegrationContext), Schema.Integration, MigrationTables.Integration);
        }

        private static void RunOnNpgSqlServer(
            IServiceCollection services,
            string backofficeProjectionsConnectionString)
        {
            services
                .AddNpgsql<IntegrationContext>(backofficeProjectionsConnectionString, sqlServerOptions =>
                {
                    sqlServerOptions.EnableRetryOnFailure();
                    sqlServerOptions.MigrationsHistoryTable(MigrationTables.Integration, Schema.Integration);
                    sqlServerOptions.UseNetTopologySuite();
                });
        }

        private static void RunInMemoryDb(
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            ILogger logger)
        {
            services
                .AddDbContext<IntegrationContext>(options => options
                    .UseLoggerFactory(loggerFactory)
                    .UseInMemoryDatabase(Guid.NewGuid().ToString(), _ => { }));

            logger.LogWarning("Running InMemory for {Context}!", nameof(IntegrationContext));
        }
    }
}
