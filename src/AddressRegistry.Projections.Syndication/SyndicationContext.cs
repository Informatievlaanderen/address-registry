namespace AddressRegistry.Projections.Syndication
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.Extensions.Configuration;
    using Municipality;
    using StreetName;
    using System;
    using System.IO;

    public class SyndicationContext : RunnerDbContext<SyndicationContext>
    {
        public override string ProjectionStateSchema => Schema.Syndication;

        public DbSet<MunicipalitySyndicationItem> MunicipalitySyndicationItems { get; set; }
        public DbSet<MunicipalityLatestItem> MunicipalityLatestItems { get; set; }
        public DbSet<StreetNameLatestItem> StreetNameLatestItems { get; set; }
        public DbSet<StreetNameSyndicationItem> StreetNameSyndicationItems { get; set; }

        public DbSet<MunicipalityBosaItem> MunicipalityBosaItems { get; set; }
        public DbSet<StreetNameBosaItem> StreetNameBosaItems { get; set; }

        // This needs to be here to please EF
        public SyndicationContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public SyndicationContext(DbContextOptions<SyndicationContext> options)
            : base(options) { }
    }

    public class ConfigBasedContextFactory : IDesignTimeDbContextFactory<SyndicationContext>
    {
        public SyndicationContext CreateDbContext(string[] args)
        {
            const string migrationConnectionStringName = "SyndicationProjectionsAdmin";

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString(migrationConnectionStringName);
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException($"Could not find a connection string with name '{migrationConnectionStringName}'");

            var builder = new DbContextOptionsBuilder<SyndicationContext>()
                .UseSqlServer(connectionString, sqlServerOptions =>
                {
                    sqlServerOptions.EnableRetryOnFailure();
                    sqlServerOptions.MigrationsHistoryTable(MigrationTables.Syndication, Schema.Syndication);
                });

            return new SyndicationContext(builder.Options);
        }
    }
}
