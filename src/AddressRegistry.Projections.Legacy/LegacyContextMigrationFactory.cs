namespace AddressRegistry.Projections.Legacy
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.SqlServer;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;

    public class LegacyContextMigrationFactory : SqlServerRunnerDbContextMigrationFactory<LegacyContext>
    {
        public LegacyContextMigrationFactory()
            : base("LegacyProjectionsAdmin", HistoryConfiguration) { }

        private static MigrationHistoryConfiguration HistoryConfiguration =>
            new MigrationHistoryConfiguration
            {
                Schema = Schema.Legacy,
                Table = MigrationTables.Legacy
            };

        protected override LegacyContext CreateContext(DbContextOptions<LegacyContext> migrationContextOptions)
            => new LegacyContext(migrationContextOptions);
    }
}
