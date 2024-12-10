namespace AddressRegistry.Api.BackOffice.Abstractions
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Infrastructure;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.Extensions.Configuration;

    public class BackOfficeContext : DbContext
    {
        public DbSet<AddressPersistentIdStreetNamePersistentId> AddressPersistentIdStreetNamePersistentIds
            => Set<AddressPersistentIdStreetNamePersistentId>();
        public DbSet<MunicipalityMergerAddress> MunicipalityMergerAddresses
            => Set<MunicipalityMergerAddress>();

        public BackOfficeContext() { }

        public BackOfficeContext(DbContextOptions<BackOfficeContext> options)
            : base(options) { }

        public async Task<AddressPersistentIdStreetNamePersistentId> AddIdempotentAddressStreetNameIdRelation(
            int addressPersistentLocalId,
            int streetNamePersistentLocalId,
            CancellationToken cancellationToken)
        {
            var relation = await FindRelationAsync(addressPersistentLocalId, cancellationToken);
            if (relation is not null)
            {
                return relation;
            }

            try
            {
                relation = new AddressPersistentIdStreetNamePersistentId(addressPersistentLocalId, streetNamePersistentLocalId);
                await AddressPersistentIdStreetNamePersistentIds.AddAsync(relation, cancellationToken);

                await SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception)
            {
                // It can happen that the back office projections were faster adding the relation than the executor (or vice versa).
                if (exception.InnerException is not SqlException { Number: 2627 })
                {
                    throw;
                }

                relation = await AddressPersistentIdStreetNamePersistentIds.FirstOrDefaultAsync(
                    x => x.AddressPersistentLocalId == addressPersistentLocalId, cancellationToken);

                if (relation is null)
                {
                    throw;
                }
            }

            return relation;
        }

        public async Task<AddressPersistentIdStreetNamePersistentId?> FindRelationAsync(
            int addressPersistentLocalId,
            CancellationToken cancellationToken)
        {
            var relation = await AddressPersistentIdStreetNamePersistentIds.FindAsync(
                [addressPersistentLocalId],
                cancellationToken);

            return relation;
        }

        public async Task RemoveIdempotentMunicipalityMergerAddress(int newAddressPersistentLocalId, CancellationToken cancellationToken)
        {
            var relation = await MunicipalityMergerAddresses.SingleOrDefaultAsync(
                x => x.NewAddressPersistentLocalId == newAddressPersistentLocalId, cancellationToken: cancellationToken);
            if (relation is null)
            {
                return;
            }

            MunicipalityMergerAddresses.Remove(relation);
            await SaveChangesAsync(cancellationToken);
        }

        public async Task<MunicipalityMergerAddress> AddIdempotentMunicipalityMergerAddress(
            int oldAddressPersistentLocalId,
            int newStreetNamePersistentLocalId,
            int newAddressPersistentLocalId,
            CancellationToken cancellationToken)
        {
            var relation = await MunicipalityMergerAddresses.FindAsync([oldAddressPersistentLocalId], cancellationToken);
            if (relation is not null)
            {
                return relation;
            }

            try
            {
                var oldStreetNamePersistentLocalId = await FindRelationAsync(oldAddressPersistentLocalId, cancellationToken);

                relation = new MunicipalityMergerAddress(
                    oldStreetNamePersistentLocalId!.StreetNamePersistentLocalId,
                    oldAddressPersistentLocalId,
                    newStreetNamePersistentLocalId,
                    newAddressPersistentLocalId);
                await MunicipalityMergerAddresses.AddAsync(relation, cancellationToken);

                await SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception)
            {
                // It can happen that the back office projections were faster adding the relation than the executor (or vice versa).
                if (exception.InnerException is not SqlException { Number: 2627 })
                {
                    throw;
                }

                relation = await MunicipalityMergerAddresses.FirstOrDefaultAsync(
                    x => x.OldAddressPersistentLocalId == oldAddressPersistentLocalId, cancellationToken);

                if (relation is null)
                {
                    throw;
                }
            }

            return relation;
        }

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

            modelBuilder.Entity<MunicipalityMergerAddress>()
                .ToTable("MunicipalityMergerAddresses", Schema.BackOffice)
                .HasKey(x => x.OldAddressPersistentLocalId)
                .IsClustered();

            modelBuilder.Entity<MunicipalityMergerAddress>()
                .Property(x => x.OldAddressPersistentLocalId)
                .ValueGeneratedNever();

            modelBuilder.Entity<MunicipalityMergerAddress>()
                .HasIndex(x => x.OldStreetNamePersistentLocalId);
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
