namespace AddressRegistry.Projections.Integration
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.Npgsql;
    using Microsoft.EntityFrameworkCore;
    using AddressRegistry.Infrastructure;
    using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

    public class IntegrationContextMigrationFactory : NpgsqlRunnerDbContextMigrationFactory<IntegrationContext>
    {
        public IntegrationContextMigrationFactory()
            : base("IntegrationProjectionsAdmin", HistoryConfiguration) { }

        private static MigrationHistoryConfiguration HistoryConfiguration =>
            new MigrationHistoryConfiguration
            {
                Schema = Schema.Integration,
                Table = MigrationTables.Integration
            };

        protected override void ConfigureSqlServerOptions(NpgsqlDbContextOptionsBuilder serverOptions)
        {
            serverOptions.UseNetTopologySuite();
            base.ConfigureSqlServerOptions(serverOptions);
        }

        protected override IntegrationContext CreateContext(DbContextOptions<IntegrationContext> migrationContextOptions)
            => new IntegrationContext(migrationContextOptions);
    }
}
