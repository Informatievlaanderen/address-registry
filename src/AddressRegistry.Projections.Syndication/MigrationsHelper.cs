namespace AddressRegistry.Projections.Syndication
{
    using System;
    using System.Data.SqlClient;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.MigrationExtensions;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Polly;

    public class MigrationsLogger { }

    public static class MigrationsHelper
    {
        public static async Task RunAsync(
            string connectionString,
            ILoggerFactory loggerFactory = null,
            CancellationToken cancellationToken = default)
        {
            var logger = loggerFactory?.CreateLogger<MigrationsLogger>();

            await Policy
                .Handle<SqlException>()
                .WaitAndRetryAsync(
                    5,
                    retryAttempt =>
                    {
                        var value = Math.Pow(2, retryAttempt) / 4;
                        var randomValue = new Random().Next((int)value * 3, (int)value * 5);
                        logger?.LogInformation("Retrying after {Seconds} seconds...", randomValue);
                        return TimeSpan.FromSeconds(randomValue);
                    })
                .ExecuteAsync(async ct =>
                    {
                        logger?.LogInformation("Running EF Migrations.");
                        await RunInternal(connectionString, loggerFactory, ct);
                    },
                    cancellationToken);
        }

        private static async Task RunInternal(string connectionString, ILoggerFactory loggerFactory, CancellationToken cancellationToken)
        {
            var migratorOptions = new DbContextOptionsBuilder<SyndicationContext>()
                .UseSqlServer(
                    connectionString,
                    sqlServerOptions =>
                    {
                        sqlServerOptions.EnableRetryOnFailure();
                        sqlServerOptions.MigrationsHistoryTable(MigrationTables.Syndication, Schema.Syndication);
                    })
                .UseExtendedSqlServerMigrations();

            if (loggerFactory != null)
                migratorOptions = migratorOptions.UseLoggerFactory(loggerFactory);

            using (var migrator = new SyndicationContext(migratorOptions.Options))
            {
                migrator.Database.SetCommandTimeout(new TimeSpan(1, 0, 0, 0));
                await migrator.Database.MigrateAsync(cancellationToken);
            }
        }
    }
}
