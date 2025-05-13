namespace AddressRegistry.Projections.Api
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.Npgsql;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;

    public class ApiContextMigrationFactory : NpgsqlRunnerDbContextMigrationFactory<ApiContext>
    {
        public ApiContextMigrationFactory()
            : base("ApiProjectionsAdmin", HistoryConfiguration) { }

        private static MigrationHistoryConfiguration HistoryConfiguration =>
            new MigrationHistoryConfiguration
            {
                Schema = Schema.Api,
                Table = MigrationTables.Api
            };

        protected override ApiContext CreateContext(DbContextOptions<ApiContext> migrationContextOptions)
            => new ApiContext(migrationContextOptions);
    }
}
