namespace AddressRegistry.Consumer.Read.StreetName
{
    using System;
    using AddressRegistry.Infrastructure;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Sql.EntityFrameworkCore;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class StreetNameConsumerModule : Module
    {
        public StreetNameConsumerModule(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            var logger = loggerFactory.CreateLogger<StreetNameConsumerModule>();
            var connectionString = configuration.GetConnectionString("ConsumerStreetName");

            var hasConnectionString = !string.IsNullOrWhiteSpace(connectionString);
            if (hasConnectionString)
            {
                RunOnSqlServer(configuration, services, loggerFactory, connectionString, serviceLifetime);
            }
            else
            {
                RunInMemoryDb(services, loggerFactory, logger);
            }
        }

        private static void RunOnSqlServer(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            string backofficeProjectionsConnectionString,
            ServiceLifetime serviceLifetime)
        {
            services
                .Add(new ServiceDescriptor(
                    typeof(TraceDbConnection<StreetNameConsumerContext>),
                    _ => new TraceDbConnection<StreetNameConsumerContext>(new SqlConnection(backofficeProjectionsConnectionString), configuration["DataDog:ServiceName"]),
                    serviceLifetime));

            services.AddDbContext<StreetNameConsumerContext>((provider, options) => options
                .UseLoggerFactory(loggerFactory)
                .UseSqlServer(provider.GetRequiredService<TraceDbConnection<StreetNameConsumerContext>>(),
                    sqlServerOptions =>
                    {
                        sqlServerOptions.EnableRetryOnFailure();
                        sqlServerOptions.MigrationsHistoryTable(MigrationTables.ConsumerReadStreetName,
                            Schema.ConsumerReadStreetName);
                    }), serviceLifetime);
        }

        private static void RunInMemoryDb(
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            ILogger logger)
        {
            services
                .AddDbContext<StreetNameConsumerContext>(options => options
                    .UseLoggerFactory(loggerFactory)
                    .UseInMemoryDatabase(Guid.NewGuid().ToString(), sqlServerOptions => { }));

            logger.LogWarning("Running InMemory for {Context}!", nameof(StreetNameConsumerContext));
        }
    }
}
