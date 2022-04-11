namespace AddressRegistry.Consumer
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Microsoft.EntityFrameworkCore;
    using StreetName;
    using AddressRegistry.Infrastructure;

    public class ConsumerContext : RunnerDbContext<ConsumerContext>
    {
        public DbSet<StreetNameConsumerItem> StreetNameConsumerItems { get; set; }

        // This needs to be here to please EF
        public ConsumerContext()
        { }

        // This needs to be DbContextOptions<T> for Autofac!
        public ConsumerContext(DbContextOptions<ConsumerContext> options)
            : base(options)
        { }

        public override string ProjectionStateSchema => Schema.Consumer;
    }

    public class ConsumerContextFactory : RunnerDbContextMigrationFactory<ConsumerContext>
    {
        public ConsumerContextFactory()
            : this("ConsumerAdmin")
        { }

        public ConsumerContextFactory(string connectionStringName)
            : base(connectionStringName, new MigrationHistoryConfiguration
            {
                Schema = Schema.Consumer,
                Table = MigrationTables.Consumer
            })
        { }

        protected override ConsumerContext CreateContext(DbContextOptions<ConsumerContext> migrationContextOptions) => new ConsumerContext(migrationContextOptions);

        public ConsumerContext Create(DbContextOptions<ConsumerContext> options) => CreateContext(options);
    }
}
