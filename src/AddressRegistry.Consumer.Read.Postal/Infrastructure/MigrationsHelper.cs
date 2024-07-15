namespace AddressRegistry.Consumer.Read.Postal.Infrastructure
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Infrastructure;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Polly;

    public class MigrationsLogger { }

    public static class MigrationsHelper
    {
        public static Task RunAsync(
            string connectionString,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken = default)
        {
            var logger = loggerFactory?.CreateLogger<MigrationsLogger>();

            return Policy
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
            var migratorOptions = new DbContextOptionsBuilder<PostalConsumerContext>()
                .UseSqlServer(
                    connectionString,
                    sqlServerOptions =>
                    {
                        sqlServerOptions.EnableRetryOnFailure();
                        sqlServerOptions.MigrationsHistoryTable(MigrationTables.ConsumerReadPostal, Schema.ConsumerReadPostal);
                    });

            migratorOptions = migratorOptions.UseLoggerFactory(loggerFactory);

            await using var migrator = new PostalConsumerContext(migratorOptions.Options);
            await migrator.Database.MigrateAsync(cancellationToken);
        }
    }
}
