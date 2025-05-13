namespace AddressRegistry.Projections.Api
{
    using System;
    using Autofac;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class ApiProjectionsModule : Module
    {
        public ApiProjectionsModule(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<ApiProjectionsModule>();
            var connectionString = configuration.GetConnectionString("ApiProjections")!;

            var hasConnectionString = !string.IsNullOrWhiteSpace(connectionString);
            if (hasConnectionString)
                RunOnNpgsql(services, loggerFactory, connectionString);
            else
                RunInMemoryDb(services, loggerFactory, logger);

            logger.LogInformation(
                "Added {Context} to services:" +
                Environment.NewLine +
                "\tSchema: {Schema}" +
                Environment.NewLine +
                "\tTableName: {TableName}",
                nameof(ApiContext), Schema.Api, MigrationTables.Api);
        }

        private static void RunOnNpgsql(
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            string apiConnectionString)
        {
            services
                .AddDbContext<ApiContext>((_, options) => options
                    .UseLoggerFactory(loggerFactory)
                    .UseSqlServer(apiConnectionString, sqlServerOptions =>
                    {
                        sqlServerOptions.EnableRetryOnFailure();
                        sqlServerOptions.MigrationsHistoryTable(MigrationTables.Api, Schema.Api);
                    }));
        }

        private static void RunInMemoryDb(
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            ILogger<ApiProjectionsModule> logger)
        {
            services
                .AddDbContext<ApiContext>(options => options
                    .UseLoggerFactory(loggerFactory)
                    .UseInMemoryDatabase(Guid.NewGuid().ToString(), _ => { }));

            logger.LogWarning("Running InMemory for {Context}!", nameof(ApiContext));
        }
    }
}
