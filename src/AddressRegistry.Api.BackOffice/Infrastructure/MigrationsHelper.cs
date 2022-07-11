namespace AddressRegistry.Api.BackOffice.Infrastructure
{
    using System;
    using Abstractions;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Polly;
    using AddressRegistry.Infrastructure;

    public class MigrationsLogger { }

    public static class MigrationsHelper
    {
        public static void Run(
            string sequenceConnectionString,
            string backOfficeConnectionString,
            ILoggerFactory loggerFactory = null)
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
                    RunInternalSequence(sequenceConnectionString, loggerFactory);
                    RunInternalBackOffice(backOfficeConnectionString, loggerFactory);
                });
        }

        private static void RunInternalSequence(string connectionString, ILoggerFactory? loggerFactory)
        {
            var migratorOptions = new DbContextOptionsBuilder<SequenceContext>()
                .UseSqlServer(
                    connectionString,
                    sqlServerOptions =>
                    {
                        sqlServerOptions.EnableRetryOnFailure();
                        sqlServerOptions.MigrationsHistoryTable(MigrationTables.Sequence, Schema.Sequence);
                    });

            if (loggerFactory != null)
            {
                migratorOptions = migratorOptions.UseLoggerFactory(loggerFactory);
            }

            using var migrator = new SequenceContext(migratorOptions.Options);
            migrator.Database.Migrate();
        }

        private static void RunInternalBackOffice(string connectionString, ILoggerFactory? loggerFactory)
        {
            var migratorOptions = new DbContextOptionsBuilder<BackOfficeContext>()
                .UseSqlServer(
                    connectionString,
                    sqlServerOptions =>
                    {
                        sqlServerOptions.EnableRetryOnFailure();
                        sqlServerOptions.MigrationsHistoryTable(MigrationTables.BackOffice, Schema.BackOffice);
                    });

            if (loggerFactory != null)
            {
                migratorOptions = migratorOptions.UseLoggerFactory(loggerFactory);
            }

            using var migrator = new BackOfficeContext(migratorOptions.Options);
            migrator.Database.Migrate();
        }
    }
}
