namespace AddressRegistry.Consumer.Infrastructure
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Polly;
    using AddressRegistry.Consumer;
    using AddressRegistry.Infrastructure;

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
                        await RunInternalConsumerContext(connectionString, loggerFactory, ct);
                        await RunInternalIdempotencyConsumerContext(connectionString, loggerFactory, ct);
                    },
                    cancellationToken);
        }

        private static async Task RunInternalConsumerContext(string connectionString, ILoggerFactory loggerFactory, CancellationToken cancellationToken)
        {
            var migratorOptions = new DbContextOptionsBuilder<ConsumerContext>()
                .UseSqlServer(
                    connectionString,
                    sqlServerOptions =>
                    {
                        sqlServerOptions.EnableRetryOnFailure();
                        sqlServerOptions.MigrationsHistoryTable(MigrationTables.ConsumerProjections, Schema.ConsumerProjections);
                    });

            migratorOptions = migratorOptions.UseLoggerFactory(loggerFactory);

            await using var migrator = new ConsumerContext(migratorOptions.Options);
            await migrator.Database.MigrateAsync(cancellationToken);
        }

        private static async Task RunInternalIdempotencyConsumerContext(string connectionString, ILoggerFactory loggerFactory, CancellationToken cancellationToken)
        {
            var migratorOptions = new DbContextOptionsBuilder<IdempotentConsumerContext>()
                .UseSqlServer(
                    connectionString,
                    sqlServerOptions =>
                    {
                        sqlServerOptions.EnableRetryOnFailure();
                        sqlServerOptions.MigrationsHistoryTable(MigrationTables.Consumer, Schema.Consumer);
                    });

            migratorOptions = migratorOptions.UseLoggerFactory(loggerFactory);

            await using var migrator = new IdempotentConsumerContext(migratorOptions.Options);
            await migrator.Database.MigrateAsync(cancellationToken);
        }
    }
}
