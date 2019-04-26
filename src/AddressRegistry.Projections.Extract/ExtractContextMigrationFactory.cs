namespace AddressRegistry.Projections.Extract
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;

    public class ExtractContextMigrationFactory : RunnerDbContextMigrationFactory<ExtractContext>
    {
        public ExtractContextMigrationFactory() :
            base("ExtractProjectionsAdmin", HistoryConfiguration)
        { }

        private static MigrationHistoryConfiguration HistoryConfiguration =>
            new MigrationHistoryConfiguration
            {
                Schema = Schema.Extract,
                Table = MigrationTables.Extract
            };

        protected override ExtractContext CreateContext(DbContextOptions<ExtractContext> migrationContextOptions)
            => new ExtractContext(migrationContextOptions);
    }
}
