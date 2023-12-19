namespace AddressRegistry.Projections.LastChangedList
{
    using System;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Sql.EntityFrameworkCore;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.SqlServer.MigrationExtensions;
    using Infrastructure;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class AddressLastChangedListModule : LastChangedListModule
    {
        public AddressLastChangedListModule(
            string connectionString,
            string datadogServiceName,
            IServiceCollection services,
            ILoggerFactory loggerFactory)
            : base(connectionString, datadogServiceName, services, loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<AddressLastChangedListModule>();

            var hasConnectionString = !string.IsNullOrWhiteSpace(connectionString);
            if (hasConnectionString)
            {
                RunOnSqlServer(datadogServiceName, services, loggerFactory, connectionString);
            }
            else
            {
                RunInMemoryDb(services, loggerFactory, logger);
            }
        }

        private static void RunOnSqlServer(
            string datadogServiceName,
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            string connectionString)
        {
            services
                .AddScoped(s => new TraceDbConnection<DataMigrationsContext>(
                    new SqlConnection(connectionString),
                    datadogServiceName))
                .AddDbContext<DataMigrationsContext>((provider, options) => options
                    .UseLoggerFactory(loggerFactory)
                    .UseSqlServer(provider.GetRequiredService<TraceDbConnection<DataMigrationsContext>>(), sqlServerOptions =>
                    {
                        sqlServerOptions.EnableRetryOnFailure();
                        sqlServerOptions.MigrationsHistoryTable(MigrationTables.RedisDataMigration, LastChangedListContext.Schema);
                    })
                    .UseExtendedSqlServerMigrations());
        }

        private static void RunInMemoryDb(
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            ILogger logger)
        {
            services
                .AddDbContext<DataMigrationsContext>(options => options
                    .UseLoggerFactory(loggerFactory)
                    .UseInMemoryDatabase(Guid.NewGuid().ToString(), sqlServerOptions => { }));

            logger.LogWarning("Running InMemory for {Context}!", nameof(DataMigrationsContext));
        }
    }
}
