namespace AddressRegistry.Api.BackOffice
{
    using System;
    using System.IO;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.Extensions.Configuration;
    using AddressRegistry.Infrastructure;

    public class BackOfficeContext : DbContext
    {
        public BackOfficeContext() { }

        public BackOfficeContext(DbContextOptions<BackOfficeContext> options)
            : base(options) { }

        public DbSet<AddressPersistentIdStreetNamePersistentId> AddressPersistentIdStreetNamePersistentIds { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AddressPersistentIdStreetNamePersistentId>()
                .ToTable("AddressPersistentIdStreetNamePersistentId", Schema.BackOffice)
                .HasKey(x => x.AddressPersistentLocalId)
                .IsClustered();

            modelBuilder.Entity<AddressPersistentIdStreetNamePersistentId>()
                .Property(x => x.AddressPersistentLocalId)
                .ValueGeneratedNever();

            modelBuilder.Entity<AddressPersistentIdStreetNamePersistentId>()
                .Property(x => x.StreetNamePersistentLocalId);
        }
    }

    public class AddressPersistentIdStreetNamePersistentId
    {
        public int AddressPersistentLocalId { get; set; }
        public int StreetNamePersistentLocalId { get; set; }

        private AddressPersistentIdStreetNamePersistentId()
        { }

        public AddressPersistentIdStreetNamePersistentId(int addressPersistentLocalId, int streetNamePersistentLocalId)
        {
            AddressPersistentLocalId = addressPersistentLocalId;
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
        }
    }

    public class ConfigBasedSequenceContextFactory : IDesignTimeDbContextFactory<BackOfficeContext>
    {
        public BackOfficeContext CreateDbContext(string[] args)
        {
            var migrationConnectionStringName = "BackOffice";

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{Environment.MachineName}.json", true)
                .AddEnvironmentVariables()
                .Build();

            var builder = new DbContextOptionsBuilder<BackOfficeContext>();

            var connectionString = configuration.GetConnectionString(migrationConnectionStringName);
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException(
                    $"Could not find a connection string with name '{migrationConnectionStringName}'");

            builder
                .UseSqlServer(connectionString, sqlServerOptions =>
                {
                    sqlServerOptions.EnableRetryOnFailure();
                    sqlServerOptions.MigrationsHistoryTable(MigrationTables.BackOffice, Schema.BackOffice);
                });

            return new BackOfficeContext(builder.Options);
        }
    }
}
