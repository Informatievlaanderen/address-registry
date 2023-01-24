namespace AddressRegistry.Infrastructure
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Polly;
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource.SqlStreamStore;
    using Microsoft.Data.SqlClient;
    using SqlStreamStore;

    public class MigrationsLogger { }

    public static class MigrationsHelper
    {
        public static void Run(
            string connectionString,
            ILoggerFactory? loggerFactory = null)
        {
            var logger = loggerFactory?.CreateLogger<MigrationsLogger>();

            Policy
                .Handle<SqlException>()
                .WaitAndRetry(
                    5,
                    retryAttempt =>
                    {
                        var value = Math.Pow(2, retryAttempt) / 4;
                        var randomValue = new Random().Next((int)value * 3, (int)value * 5);
                        logger?.LogInformation("Retrying after {Seconds} seconds...", randomValue);
                        return TimeSpan.FromSeconds(randomValue);
                    })
                .Execute(() =>
                    {
                        logger?.LogInformation("Running EF Migrations.");
                        RunInternal(connectionString, loggerFactory);
                    });
        }

        private static void RunInternal(string connectionString, ILoggerFactory? loggerFactory)
        {
            var migratorOptions = new DbContextOptionsBuilder<SequenceContext>()
                .UseSqlServer(
                    connectionString,
                    sqlServerOptions =>
                    {
                        sqlServerOptions.EnableRetryOnFailure();
                        sqlServerOptions.MigrationsHistoryTable(MigrationTables.Sequence, Schema.Sequence);
                    });

            if (loggerFactory is not null)
            {
                migratorOptions = migratorOptions.UseLoggerFactory(loggerFactory);
            }

            using var migrator = new SequenceContext(migratorOptions.Options);
            migrator.Database.Migrate();
        }

        public static void EnsureSqlStreamStoreSchema<T>(
            MsSqlStreamStore streamStore,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<T>();

            Policy
                .Handle<SqlException>()
                .WaitAndRetry(
                    5,
                    retryAttempt =>
                    {
                        var value = Math.Pow(2, retryAttempt) / 4;
                        var randomValue = new Random().Next((int)value * 3, (int)value * 5);
                        logger.LogInformation("Retrying after {Seconds} seconds...", randomValue);
                        return TimeSpan.FromSeconds(randomValue);
                    })
                .Execute(() =>
                {
                    logger.LogInformation("Ensuring the sql stream store schema.");

                    var checkSchemaResult = streamStore.CheckSchema().GetAwaiter().GetResult();
                    if (!checkSchemaResult.IsMatch())
                        streamStore.CreateSchema().GetAwaiter().GetResult();
                });
        }

        public static void EnsureSqlSnapshotStoreSchema<T>(
            MsSqlSnapshotStore streamStore,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<T>();

            Policy
                .Handle<SqlException>()
                .WaitAndRetry(
                    5,
                    retryAttempt =>
                    {
                        var value = Math.Pow(2, retryAttempt) / 4;
                        var randomValue = new Random().Next((int)value * 3, (int)value * 5);
                        logger.LogInformation("Retrying after {Seconds} seconds...", randomValue);
                        return TimeSpan.FromSeconds(randomValue);
                    })
                .Execute(() =>
                {
                    logger.LogInformation("Ensuring the sql snapshot store schema.");

                    streamStore.CreateTable().GetAwaiter().GetResult();
                });
        }
    }
}
