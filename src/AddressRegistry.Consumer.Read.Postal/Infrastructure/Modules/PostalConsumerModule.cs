namespace AddressRegistry.Consumer.Read.Postal.Infrastructure.Modules
{
    using System;
    using AddressRegistry.Infrastructure;
    using Autofac;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class PostalConsumerModule : Module
    {
        public PostalConsumerModule(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            var logger = loggerFactory.CreateLogger<PostalConsumerModule>();
            var connectionString = configuration.GetConnectionString("ConsumerPostal");

            var hasConnectionString = !string.IsNullOrWhiteSpace(connectionString);
            if (hasConnectionString)
            {
                RunOnSqlServer(services, loggerFactory, connectionString, serviceLifetime);
            }
            else
            {
                RunInMemoryDb(services, loggerFactory, logger);
            }
        }

        private static void RunOnSqlServer(
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            string consumerConnectionString,
            ServiceLifetime serviceLifetime)
        {
            services.AddDbContext<PostalConsumerContext>((_, options) => options
                .UseLoggerFactory(loggerFactory)
                .UseSqlServer(consumerConnectionString,
                    sqlServerOptions =>
                    {
                        sqlServerOptions.EnableRetryOnFailure();
                        sqlServerOptions.MigrationsHistoryTable(MigrationTables.ConsumerReadPostal,
                            Schema.ConsumerReadPostal);
                    }), serviceLifetime);
        }

        private static void RunInMemoryDb(
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            ILogger logger)
        {
            services
                .AddDbContext<PostalConsumerContext>(options => options
                    .UseLoggerFactory(loggerFactory)
                    .UseInMemoryDatabase(Guid.NewGuid().ToString(), sqlServerOptions => { }));

            logger.LogWarning("Running InMemory for {Context}!", nameof(PostalConsumerContext));
        }
    }
}
