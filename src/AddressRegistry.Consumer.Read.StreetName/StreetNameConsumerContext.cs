namespace AddressRegistry.Consumer.Read.StreetName
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Infrastructure;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer.SqlServer;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.ProjectionStates;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.SqlServer.MigrationExtensions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.Extensions.Configuration;
    using Projections;

    public class StreetNameConsumerContext : SqlServerConsumerDbContext<StreetNameConsumerContext>
    {
        public string ProjectionStateSchema => Schema.ConsumerReadStreetName;

        public DbSet<StreetNameLatestItem> StreetNameLatestItems { get; set; }

        public DbSet<ProjectionStateItem> ProjectionStates => Set<ProjectionStateItem>();

        // This needs to be here to please EF
        public StreetNameConsumerContext()
        { }

        // This needs to be DbContextOptions<T> for Autofac!
        public StreetNameConsumerContext(DbContextOptions<StreetNameConsumerContext> options)
            : base(options)
        { }

        public override string ProcessedMessagesSchema => Schema.ConsumerReadStreetName;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new ProjectionStatesConfiguration(ProjectionStateSchema));
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(StreetNameConsumerContext).GetTypeInfo().Assembly);
        }

        public virtual async Task UpdateProjectionState(string? projectionName, long position, CancellationToken cancellationToken)
        {
            if (projectionName is null)
            {
                return;
            }

            var projectionStateItem = await ProjectionStates.SingleOrDefaultAsync(item => item.Name == projectionName, cancellationToken).ConfigureAwait(false);

            if (projectionStateItem == null)
            {
                projectionStateItem = new ProjectionStateItem { Name = projectionName };
                await ProjectionStates.AddAsync(projectionStateItem, cancellationToken).ConfigureAwait(false);
            }

            projectionStateItem.Position = position;
        }
    }

    //Classed used when running dotnet ef migrations
    public class ConsumerContextFactory : IDesignTimeDbContextFactory<StreetNameConsumerContext>
    {
        public StreetNameConsumerContext CreateDbContext(string[] args)
        {
            const string migrationConnectionStringName = "ConsumerStreetNameAdmin";

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            var builder = new DbContextOptionsBuilder<StreetNameConsumerContext>();

            var connectionString = configuration.GetConnectionString(migrationConnectionStringName);
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException($"Could not find a connection string with name '{migrationConnectionStringName}'");

            builder
                .UseSqlServer(connectionString, sqlServerOptions =>
                {
                    sqlServerOptions.EnableRetryOnFailure();
                    sqlServerOptions.MigrationsHistoryTable(MigrationTables.ConsumerReadStreetName, Schema.ConsumerReadStreetName);
                })
                .UseExtendedSqlServerMigrations();

            return new StreetNameConsumerContext(builder.Options);
        }
    }

    public static class ContextExtensions
    {
        public static async Task<StreetNameLatestItem> FindAndUpdateLatestItem(
            this StreetNameConsumerContext context,
            int streetNamePersistentLocalId,
            Action<StreetNameLatestItem> updateFunc,
            CancellationToken ct)
        {
            var latestItem = await context
                .StreetNameLatestItems
                .FindAsync(streetNamePersistentLocalId, cancellationToken: ct);

            if (latestItem == null)
                throw DatabaseItemNotFound(streetNamePersistentLocalId);

            updateFunc(latestItem);

            return latestItem;
        }

        private static ProjectionItemNotFoundException<StreetNameLatestItemProjections> DatabaseItemNotFound(int streetNamePersistentLocalId)
            => new ProjectionItemNotFoundException<StreetNameLatestItemProjections>(streetNamePersistentLocalId.ToString());
    }
}
