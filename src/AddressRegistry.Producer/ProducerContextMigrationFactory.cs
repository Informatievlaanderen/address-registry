namespace AddressRegistry.Producer
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Microsoft.EntityFrameworkCore;
    using AddressRegistry.Infrastructure;

    public class ProducerContextMigrationFactory : RunnerDbContextMigrationFactory<ProducerContext>
    {
        public ProducerContextMigrationFactory()
            : base("ProducerProjectionsAdmin", HistoryConfiguration) { }

        private static MigrationHistoryConfiguration HistoryConfiguration =>
            new MigrationHistoryConfiguration
            {
                Schema = Schema.Producer,
                Table = MigrationTables.Producer
            };

        protected override ProducerContext CreateContext(DbContextOptions<ProducerContext> migrationContextOptions)
            => new ProducerContext(migrationContextOptions);
    }
}
