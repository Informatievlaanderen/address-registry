namespace AddressRegistry.Projections.Extract
{
    using System;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.SqlServer.MigrationExtensions;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Syndication;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class ExtractModule : Module
    {
        public ExtractModule(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            bool enableRetry = true)
        {
            var logger = loggerFactory.CreateLogger<ExtractModule>();
            var connectionString = configuration.GetConnectionString("ExtractProjections");

            var hasConnectionString = !string.IsNullOrWhiteSpace(connectionString);
            if (hasConnectionString)
                RunOnSqlServer(services, loggerFactory, connectionString, enableRetry);
            else
                RunInMemoryDb(services, loggerFactory, logger);

            logger.LogInformation(
                "Added {Context} to services:" +
                Environment.NewLine +
                "\tSchema: {Schema}" +
                Environment.NewLine +
                "\tTableName: {TableName}",
                nameof(ExtractContext), Schema.Extract, MigrationTables.Extract);
        }

        private static void RunOnSqlServer(
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            string extractConnectionString,
            bool enableRetry)
        {
            services
                .AddDbContext<ExtractContext>((provider, options) => options
                    .UseLoggerFactory(loggerFactory)
                    .UseSqlServer(extractConnectionString, sqlServerOptions =>
                    {
                        if (enableRetry)
                            sqlServerOptions.EnableRetryOnFailure();

                        sqlServerOptions.MigrationsHistoryTable(MigrationTables.Extract, Schema.Extract);
                    })
                    .UseExtendedSqlServerMigrations());
        }

        private static void RunInMemoryDb(
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            ILogger<ExtractModule> logger)
        {
            services
                .AddDbContext<ExtractContext>(options => options
                    .UseLoggerFactory(loggerFactory)
                    .UseInMemoryDatabase(Guid.NewGuid().ToString(), sqlServerOptions => { }));

            logger.LogWarning("Running InMemory for {Context}!", nameof(ExtractContext));
        }

        protected override void Load(ContainerBuilder builder)
            => builder.RegisterType<RegistryAtomFeedReader>().As<IRegistryAtomFeedReader>();
    }
}
