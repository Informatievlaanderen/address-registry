namespace AddressRegistry.Consumer.Read.StreetName
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Infrastructure;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Microsoft.EntityFrameworkCore;
    using Projections;

    public class StreetNameConsumerContext : RunnerDbContext<StreetNameConsumerContext>
    {
        public DbSet<StreetNameLatestItem> StreetNameLatestItems { get; set; }
        public DbSet<StreetNameBosaItem> StreetNameBosaItems { get; set; }

        // This needs to be here to please EF
        public StreetNameConsumerContext()
        { }

        // This needs to be DbContextOptions<T> for Autofac!
        public StreetNameConsumerContext(DbContextOptions<StreetNameConsumerContext> options)
            : base(options)
        { }

        public override string ProjectionStateSchema => Schema.ConsumerReadStreetName;
    }

    //Classed used when running dotnet ef migrations
    public class ConsumerContextFactory : RunnerDbContextMigrationFactory<StreetNameConsumerContext>
    {
        public ConsumerContextFactory()
            : this("ConsumerAdmin")
        { }

        public ConsumerContextFactory(string connectionStringName)
            : base(connectionStringName, new MigrationHistoryConfiguration
            {
                Schema = Schema.ConsumerReadStreetName,
                Table = MigrationTables.ConsumerReadStreetName
            })
        { }

        protected override StreetNameConsumerContext CreateContext(DbContextOptions<StreetNameConsumerContext> migrationContextOptions) => new StreetNameConsumerContext(migrationContextOptions);

        public StreetNameConsumerContext Create(DbContextOptions<StreetNameConsumerContext> options) => CreateContext(options);
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

            await context.SaveChangesAsync(ct);

            return latestItem;
        }

        public static async Task<StreetNameBosaItem> FindAndUpdateBosaItem(
            this StreetNameConsumerContext context,
            int streetNamePersistentLocalId,
            Action<StreetNameBosaItem> updateFunc,
            CancellationToken ct)
        {
            var latestItem = await context
                .StreetNameBosaItems
                .FindAsync(streetNamePersistentLocalId, cancellationToken: ct);

            if (latestItem == null)
                throw DatabaseItemNotFound(streetNamePersistentLocalId);

            updateFunc(latestItem);

            await context.SaveChangesAsync(ct);

            return latestItem;
        }

        private static ProjectionItemNotFoundException<StreetNameLatestItemProjections> DatabaseItemNotFound(int streetNamePersistentLocalId)
            => new ProjectionItemNotFoundException<StreetNameLatestItemProjections>(streetNamePersistentLocalId.ToString());
    }
}
