namespace AddressRegistry.Projections.Legacy
{
    using System;
    using System.IO;
    using AddressDetail;
    using AddressList;
    using AddressMatch;
    using AddressSyndication;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using CrabIdToPersistentLocalId;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.Extensions.Configuration;

    public class LegacyContext : RunnerDbContext<LegacyContext>
    {
        public override string ProjectionStateSchema => Schema.Legacy;

        public DbSet<CrabIdToPersistentLocalIdItem> CrabIdToPersistentLocalIds { get; set; }

        public DbSet<AddressVersion.AddressVersion> AddressVersions { get; set; }
        public DbSet<AddressDetailItem> AddressDetail { get; set; }
        public DbSet<AddressListItem> AddressList { get; set; }
        public DbSet<AddressSyndicationItem> AddressSyndication { get; set; }
        public DbQuery<RRStreetName> RRStreetNames { get; set; }
        public DbQuery<KadStreetName> KadStreetNames { get; set; }
        public DbQuery<RRAddress> RRAddresses { get; set; }

        public LegacyContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public LegacyContext(DbContextOptions<LegacyContext> options)
            : base(options) { }
    }

    public class ConfigBasedContextFactory : IDesignTimeDbContextFactory<LegacyContext>
    {
        public LegacyContext CreateDbContext(string[] args)
        {
            const string migrationConnectionStringName = "LegacyProjectionsAdmin";

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString(migrationConnectionStringName);
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException($"Could not find a connection string with name '{migrationConnectionStringName}'");

            var builder = new DbContextOptionsBuilder<LegacyContext>()
                .UseSqlServer(connectionString, sqlServerOptions =>
                {
                    sqlServerOptions.EnableRetryOnFailure();
                    sqlServerOptions.MigrationsHistoryTable(MigrationTables.Legacy, Schema.Legacy);
                    //sqlServerOptions.UseNetTopologySuite();
                });

            return new LegacyContext(builder.Options);
        }
    }
}
