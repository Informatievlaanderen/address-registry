namespace AddressRegistry.Consumer.Read.Municipality
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Infrastructure;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.SqlServer.MigrationExtensions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.Extensions.Configuration;
    using Projections;

    public class MunicipalityConsumerContext : RunnerDbContext<MunicipalityConsumerContext>
    {
        public DbSet<MunicipalityLatestItem> MunicipalityLatestItems { get; set; }

        // This needs to be here to please EF
        public MunicipalityConsumerContext()
        { }

        // This needs to be DbContextOptions<T> for Autofac!
        public MunicipalityConsumerContext(DbContextOptions<MunicipalityConsumerContext> options)
            : base(options)
        { }

        public override string ProjectionStateSchema => Schema.ConsumerReadMunicipality;
    }

    public class ConsumerContextFactory : IDesignTimeDbContextFactory<MunicipalityConsumerContext>
    {
        public MunicipalityConsumerContext CreateDbContext(string[] args)
        {
            const string migrationConnectionStringName = "ConsumerMunicipalityAdmin";

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            var builder = new DbContextOptionsBuilder<MunicipalityConsumerContext>();

            var connectionString = configuration.GetConnectionString(migrationConnectionStringName);
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException($"Could not find a connection string with name '{migrationConnectionStringName}'");

            builder
                .UseSqlServer(connectionString, sqlServerOptions =>
                {
                    sqlServerOptions.EnableRetryOnFailure();
                    sqlServerOptions.MigrationsHistoryTable(MigrationTables.ConsumerReadMunicipality, Schema.ConsumerReadMunicipality);
                })
                .UseExtendedSqlServerMigrations();

            return new MunicipalityConsumerContext(builder.Options);
        }
    }

    public static class AddressDetailExtensions
    {
        public static async Task<MunicipalityLatestItem> FindAndUpdate(
            this MunicipalityConsumerContext context,
            Guid municipalityId,
            Action<MunicipalityLatestItem> updateFunc,
            CancellationToken ct)
        {
            var municipality = await context
                .MunicipalityLatestItems
                .FindAsync(municipalityId, cancellationToken: ct);

            if (municipality == null)
                throw DatabaseItemNotFound(municipalityId);

            updateFunc(municipality);

            return municipality;
        }

        private static ProjectionItemNotFoundException<MunicipalityLatestItemProjections> DatabaseItemNotFound(Guid municipalityId)
            => new ProjectionItemNotFoundException<MunicipalityLatestItemProjections>(municipalityId.ToString("D"));
    }
}
