namespace AddressRegistry.Projections.Elastic
{
    using AddressRegistry.Infrastructure;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.SqlServer;
    using Microsoft.EntityFrameworkCore;

    public class ElasticRunnerContextMigrationFactory : SqlServerRunnerDbContextMigrationFactory<ElasticRunnerContext>
    {
        public ElasticRunnerContextMigrationFactory()
            : base("ElasticProjectionsAdmin", HistoryConfiguration) { }

        private static MigrationHistoryConfiguration HistoryConfiguration =>
            new MigrationHistoryConfiguration
            {
                Schema = Schema.Elastic,
                Table = MigrationTables.Elastic
            };

        protected override ElasticRunnerContext CreateContext(DbContextOptions<ElasticRunnerContext> migrationContextOptions)
            => new ElasticRunnerContext(migrationContextOptions);
    }
}
