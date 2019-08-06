namespace AddressRegistry.Projections.Legacy
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;

    public class LegacyContextMigrationFactory : RunnerDbContextMigrationFactory<LegacyContext>
    {
        public LegacyContextMigrationFactory()
            : base("LegacyProjectionsAdmin", HistoryConfiguration)
        { }

        private static MigrationHistoryConfiguration HistoryConfiguration =>
            new MigrationHistoryConfiguration
            {
                Schema = Schema.Legacy,
                Table = MigrationTables.Legacy
            };

        protected override LegacyContext CreateContext(DbContextOptions<LegacyContext> migrationContextOptions)
            => new LegacyContext(migrationContextOptions);

        //protected override void ConfigureSqlServerOptions(SqlServerDbContextOptionsBuilder sqlServerOptions)
        //    => sqlServerOptions.UseNetTopologySuite();
    }
}
