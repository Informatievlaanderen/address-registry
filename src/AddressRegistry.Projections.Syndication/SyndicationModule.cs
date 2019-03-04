namespace AddressRegistry.Projections.Syndication
{
    using System;
    using System.Data.SqlClient;
    using System.Net.Http;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Http;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Sql.EntityFrameworkCore;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Syndication;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Polly;

    public class SyndicationModule : Module
    {
        public SyndicationModule(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<SyndicationModule>();
            var connectionString = configuration.GetConnectionString("SyndicationProjections");

            var hasConnectionString = !string.IsNullOrWhiteSpace(connectionString);
            if (hasConnectionString)
                RunOnSqlServer(configuration, services, loggerFactory, connectionString);
            else
                RunInMemoryDb(services, loggerFactory, logger);

            RegisterHttpClient(configuration, services);
        }

        private static void RunOnSqlServer(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            string backofficeProjectionsConnectionString)
        {
            services
                .AddScoped(s => new TraceDbConnection(
                    new SqlConnection(backofficeProjectionsConnectionString),
                    configuration["DataDog:ServiceName"]))
                .AddDbContext<SyndicationContext>((provider, options) => options
                    .UseLoggerFactory(loggerFactory)
                    .UseSqlServer(provider.GetRequiredService<TraceDbConnection>(), sqlServerOptions =>
                    {
                        sqlServerOptions.EnableRetryOnFailure();
                        sqlServerOptions.MigrationsHistoryTable(MigrationTables.Syndication, Schema.Syndication);
                    }));
        }

        private static void RunInMemoryDb(
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            ILogger<SyndicationModule> logger)
        {
            services
                .AddDbContext<SyndicationContext>(options => options
                    .UseLoggerFactory(loggerFactory)
                    .UseInMemoryDatabase(Guid.NewGuid().ToString(), sqlServerOptions => { }));

            logger.LogWarning("Running InMemory for {Context}!", nameof(SyndicationContext));
        }

        private static void RegisterHttpClient(IConfiguration configuration, IServiceCollection services)
        {
            services
                .AddHttpClient(
                    RegistryAtomFeedReader.HttpClientName,
                    client => { client.DefaultRequestHeaders.Add("Accept", "application/atom+xml"); })
                .ConfigurePrimaryHttpMessageHandler(c => new TraceHttpMessageHandler(
                    new HttpClientHandler(),
                    configuration["DataDog:ServiceName"]))
                .AddTransientHttpErrorPolicy(policyBuilder => policyBuilder
                    .WaitAndRetryAsync(
                        5,
                        retryAttempt =>
                        {
                            var value = Math.Pow(2, retryAttempt) / 4;
                            var randomValue = new Random().Next((int) value * 3, (int) value * 5);
                            return TimeSpan.FromSeconds(randomValue);
                        }));
        }

        protected override void Load(ContainerBuilder builder)
            => builder.RegisterType<RegistryAtomFeedReader>().As<IRegistryAtomFeedReader>();
    }
}
