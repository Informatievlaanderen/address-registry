namespace AddressRegistry.Projections.LastChangedList
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.SqlServer;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;

    public class DataMigrationsContext : RunnerDbContext<DataMigrationsContext>
    {
        public override string ProjectionStateSchema => LastChangedListContext.Schema;

        // This needs to be here to please EF
        public DataMigrationsContext()
        { }

        // This needs to be DbContextOptions<T> for Autofac!
        public DataMigrationsContext(DbContextOptions<DataMigrationsContext> options)
            : base(options)
        { }

        public override Task UpdateProjectionState(string projectionName, long position,
            CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("Cannot update projection state from this context");
        }

        public override Task UpdateProjectionDesiredState(string projectionName, string desiredState,
            CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("Cannot update projection state from this context");
        }

        public override Task SetErrorMessage(string projectionName, string? errorMessage,
            CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("Cannot set error message from this context");
        }
    }

    public class DataMigrationContextMigrationFactory : SqlServerRunnerDbContextMigrationFactory<DataMigrationsContext>
    {
        private static MigrationHistoryConfiguration HistoryConfiguration =>
            new MigrationHistoryConfiguration
            {
                Schema = LastChangedListContext.Schema,
                Table = MigrationTables.RedisDataMigration
            };

        public DataMigrationContextMigrationFactory()
            : base("LastChangedListAdmin", HistoryConfiguration)
        {
        }

        protected override DataMigrationsContext CreateContext(
            DbContextOptions<DataMigrationsContext> migrationContextOptions)
            => new DataMigrationsContext(migrationContextOptions);
    }
}
