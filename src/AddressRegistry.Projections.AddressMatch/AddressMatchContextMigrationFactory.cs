namespace AddressRegistry.Projections.AddressMatch
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.SqlServer;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;

    public class AddressMatchContextMigrationFactory : SqlServerRunnerDbContextMigrationFactory<AddressMatchContext>
    {
        public AddressMatchContextMigrationFactory()
            : base("AddressMatchProjectionsAdmin", HistoryConfiguration) { }

        private static MigrationHistoryConfiguration HistoryConfiguration =>
            new MigrationHistoryConfiguration
            {
                Schema = Schema.AddressMatch,
                Table = MigrationTables.AddressMatch
            };

        protected override AddressMatchContext CreateContext(DbContextOptions<AddressMatchContext> migrationContextOptions)
            => new AddressMatchContext(migrationContextOptions);
    }
}
