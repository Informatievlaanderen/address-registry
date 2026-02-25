namespace AddressRegistry.Projections.Feed
{
    using System;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.SqlServer.MigrationExtensions;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    public class FeedModule : Module
    {
        public FeedModule(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            JsonSerializerSettings jsonSerializerSettings)
        {
            var logger = loggerFactory.CreateLogger<FeedModule>();
            var connectionString = configuration.GetConnectionString("FeedProjections");

            var hasConnectionString = !string.IsNullOrWhiteSpace(connectionString);
            if (hasConnectionString)
                RunOnSqlServer(services, loggerFactory, connectionString!, jsonSerializerSettings);
            else
                RunInMemoryDb(services, loggerFactory, logger, jsonSerializerSettings);

            logger.LogInformation(
                "Added {Context} to services:" +
                Environment.NewLine +
                "\tSchema: {Schema}" +
                Environment.NewLine +
                "\tTableName: {TableName}",
                nameof(FeedContext), Schema.Feed, MigrationTables.Feed);
        }

        private static void RunOnSqlServer(
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            string connectionString,
            JsonSerializerSettings jsonSerializerSettings)
        {
            services
                .AddDbContext<FeedContext>((_, options) => options
                    .UseLoggerFactory(loggerFactory)
                    .UseSqlServer(connectionString, sqlServerOptions =>
                    {
                        sqlServerOptions.EnableRetryOnFailure();
                        sqlServerOptions.MigrationsHistoryTable(MigrationTables.Feed, Schema.Feed);
                    })
                    .UseExtendedSqlServerMigrations());

            services.AddScoped(provider =>
            {
                var options = provider.GetRequiredService<DbContextOptions<FeedContext>>();
                return new FeedContext(options, jsonSerializerSettings);
            });
        }

        private static void RunInMemoryDb(
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            ILogger logger,
            JsonSerializerSettings jsonSerializerSettings)
        {
            services
                .AddDbContext<FeedContext>(options => options
                    .UseLoggerFactory(loggerFactory)
                    .UseInMemoryDatabase(Guid.NewGuid().ToString(), _ => { }));

            services.AddScoped(provider =>
            {
                var options = provider.GetRequiredService<DbContextOptions<FeedContext>>();
                return new FeedContext(options, jsonSerializerSettings);
            });

            logger.LogWarning("Running InMemory for {Context}!", nameof(FeedContext));
        }
    }
}
