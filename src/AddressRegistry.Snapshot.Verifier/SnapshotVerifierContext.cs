namespace AddressRegistry.Snapshot.Verifier
{
    using System;
    using System.IO;
    using AddressRegistry.Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.Extensions.Configuration;

    public class SnapshotVerifierContext : DbContext
    {
        public DbSet<SnapshotVerificationState> VerificationStates => Set<SnapshotVerificationState>();

        // This needs to be here to please EF
        public SnapshotVerifierContext()
        {
        }

        // This needs to be DbContextOptions<T> for Autofac!
        public SnapshotVerifierContext(DbContextOptions<SnapshotVerifierContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<SnapshotVerificationState>()
                .ToTable("SnapshotVerificationStates", Schema.Default)
                .HasKey(x => x.SnapshotId);

            modelBuilder
                .Entity<SnapshotVerificationState>()
                .Property(x => x.SnapshotId)
                .ValueGeneratedNever();

            modelBuilder
                .Entity<SnapshotVerificationState>()
                .Property(x => x.Status)
                .HasConversion<string>(
                    x => x.ToString(),
                    y => Enum.Parse<SnapshotStateStatus>(y));

            modelBuilder
                .Entity<SnapshotVerificationState>()
                .HasIndex(x => x.Status);
        }
    }

    public class SnapshotVerificationState
    {
        public int SnapshotId { get; set; }
        public SnapshotStateStatus Status { get; set; }
        public string? Differences { get; set; }
    }

    public enum SnapshotStateStatus
    {
        Verified,
        Failed,
    }

    public class SnapshotVerifierContextFactory : IDesignTimeDbContextFactory<SnapshotVerifierContext>
    {
        public const string MigrationsTableName = "__EFMigrationsHistorySnapshotVerifier";
        public const string ConnectionStringName = "SnapshotVerifier";

        public SnapshotVerifierContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString(ConnectionStringName);
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException($"Could not find a connection string with name '{ConnectionStringName}'");

            var builder = new DbContextOptionsBuilder<SnapshotVerifierContext>()
                .UseSqlServer(connectionString, sqlServerOptions =>
                {
                    sqlServerOptions.EnableRetryOnFailure();
                    sqlServerOptions.MigrationsHistoryTable(MigrationsTableName, Schema.Default);
                });

            return new SnapshotVerifierContext(builder.Options);
        }
    }
}
