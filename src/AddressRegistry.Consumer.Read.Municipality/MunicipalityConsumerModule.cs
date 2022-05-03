namespace AddressRegistry.Consumer.Read.Municipality
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

    public class MunicipalityConsumerModule : Module
    {
        public MunicipalityConsumerModule(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            var logger = loggerFactory.CreateLogger<MunicipalityConsumerModule>();
            var connectionString = configuration.GetConnectionString("Consumer");

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
                    typeof(TraceDbConnection<MunicipalityConsumerContext>),
                    _ => new TraceDbConnection<MunicipalityConsumerContext>(new SqlConnection(backofficeProjectionsConnectionString), configuration["DataDog:ServiceName"]),
                    serviceLifetime));

            services.AddDbContext<MunicipalityConsumerContext>((provider, options) => options
                .UseLoggerFactory(loggerFactory)
                .UseSqlServer(provider.GetRequiredService<TraceDbConnection<MunicipalityConsumerContext>>(),
                    sqlServerOptions =>
                    {
                        sqlServerOptions.EnableRetryOnFailure();
                        sqlServerOptions.MigrationsHistoryTable(MigrationTables.ConsumerReadMunicipality,
                            Schema.ConsumerReadMunicipality);
                    }), serviceLifetime);
        }

        private static void RunInMemoryDb(
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            ILogger logger)
        {
            services
                .AddDbContext<MunicipalityConsumerContext>(options => options
                    .UseLoggerFactory(loggerFactory)
                    .UseInMemoryDatabase(Guid.NewGuid().ToString(), sqlServerOptions => { }));

            logger.LogWarning("Running InMemory for {Context}!", nameof(MunicipalityConsumerContext));
        }
    }
}
