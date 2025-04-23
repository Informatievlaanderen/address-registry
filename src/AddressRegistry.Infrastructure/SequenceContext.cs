namespace AddressRegistry.Infrastructure
{
    using System;
    using System.IO;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.Extensions.Configuration;

    public class SequenceContext : DbContext
    {
        public const string AddressPersistentLocalIdSequenceName = "AddressPersistentLocalIds";

        public SequenceContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public SequenceContext(DbContextOptions<SequenceContext> options)
            : base(options) { }
    }

    public class ConfigBasedSequenceContextFactory : IDesignTimeDbContextFactory<SequenceContext>
    {
        public SequenceContext CreateDbContext(string[] args)
        {
            var migrationConnectionStringName = "Sequences";

            var configuration = new ConfigurationBuilder()
                // .SetBasePath(Directory.GetCurrentDirectory())
                // .AddJsonFile("appsettings.json")
                // .AddJsonFile($"appsettings.{Environment.MachineName}.json")
                // .AddEnvironmentVariables()
                .Build();

            var builder = new DbContextOptionsBuilder<SequenceContext>();

            var connectionString = configuration.GetConnectionString(migrationConnectionStringName);
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException($"Could not find a connection string with name '{migrationConnectionStringName}'");

            builder
                .UseSqlServer(connectionString, sqlServerOptions =>
                {
                    sqlServerOptions.EnableRetryOnFailure();
                    sqlServerOptions.MigrationsHistoryTable(MigrationTables.Sequence, Schema.Sequence);
                });

            return new SequenceContext(builder.Options);
        }
    }
}
