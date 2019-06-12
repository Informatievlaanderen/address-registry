namespace AddressRegistry.Projections.Legacy
{
    using AddressDetail;
    using AddressList;
    using AddressMatch;
    using AddressSyndication;
    using Be.Vlaanderen.Basisregisters.EntityFrameworkCore.EntityTypeConfiguration;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using CrabIdToOsloId;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.Extensions.Configuration;
    using System;
    using System.IO;
    using System.Reflection;

    public class LegacyContext : RunnerDbContext<LegacyContext>
    {
        public override string ProjectionStateSchema => Schema.Legacy;

        public DbSet<CrabIdToOsloIdItem> CrabIdToOsloIds { get; set; }

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

        protected override void OnConfiguringOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=EFProviders.InMemory.AddressRegistry.AddressRegistryLegacyContext;Trusted_Connection=True;");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.AddEntityConfigurationsFromAssembly(Assembly.GetAssembly(typeof(AddressDetailItem)));
        }
    }

    public class ConfigBasedLegacyContextFactory : IDesignTimeDbContextFactory<LegacyContext>
    {
        public LegacyContext CreateDbContext(string[] args)
        {
            var migrationConnectionStringName = "LegacyProjectionsAdmin";

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{Environment.MachineName}.json")
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
                });

            return new LegacyContext(builder.Options);
        }
    }
}
