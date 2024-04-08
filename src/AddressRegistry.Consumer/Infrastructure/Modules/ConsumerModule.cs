namespace AddressRegistry.Consumer.Infrastructure.Modules
{
    using System;
    using AddressRegistry.Infrastructure;
    using Autofac;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class ConsumerModule : Module
    {
        public ConsumerModule(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<ConsumerModule>();
            var connectionString = configuration.GetConnectionString("Consumer");

            var hasConnectionString = !string.IsNullOrWhiteSpace(connectionString);
            if (hasConnectionString)
            {
                RunOnSqlServer(services, loggerFactory, connectionString);
            }
            else
            {
                RunInMemoryDb(services, loggerFactory, logger);
            }
        }

        private static void RunOnSqlServer(
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            string consumerConnectionString)
        {
            services
                .AddDbContext<ConsumerContext>((_, options) => options
                    .UseLoggerFactory(loggerFactory)
                    .UseSqlServer(consumerConnectionString, sqlServerOptions =>
                    {
                        sqlServerOptions.EnableRetryOnFailure();
                        sqlServerOptions.MigrationsHistoryTable(MigrationTables.ConsumerProjections, Schema.ConsumerProjections);
                    }));
        }

        private static void RunInMemoryDb(
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            ILogger logger)
        {
            services
                .AddDbContext<ConsumerContext>(options => options
                    .UseLoggerFactory(loggerFactory)
                    .UseInMemoryDatabase(Guid.NewGuid().ToString(), sqlServerOptions => { }));

            logger.LogWarning("Running InMemory for {Context}!", nameof(ConsumerContext));
        }
    }
}
