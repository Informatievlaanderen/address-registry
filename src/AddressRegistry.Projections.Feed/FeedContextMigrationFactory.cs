namespace AddressRegistry.Projections.Feed
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.SqlServer;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;

    public class FeedContextMigrationFactory : SqlServerRunnerDbContextMigrationFactory<FeedContext>
    {
        public FeedContextMigrationFactory()
            : base("FeedProjectionsAdmin", new MigrationHistoryConfiguration
            {
                Schema = Schema.Feed,
                Table = MigrationTables.Feed
            })
        { }

        protected override FeedContext CreateContext(DbContextOptions<FeedContext> migrationContextOptions)
            => new FeedContext(migrationContextOptions, new JsonSerializerSettings());
    }
}
