namespace AddressRegistry.Projections.Integration
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Microsoft.EntityFrameworkCore;
    using AddressRegistry.Infrastructure;

    public class IntegrationContext : RunnerDbContext<IntegrationContext>
    {
        public override string ProjectionStateSchema => Schema.Integration;

        public DbSet<AddressLatestItem> AddressLatestItems => Set<AddressLatestItem>();
        public DbSet<AddressVersion> AddressVersions => Set<AddressVersion>();

        // This needs to be here to please EF
        public IntegrationContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public IntegrationContext(DbContextOptions<IntegrationContext> options)
            : base(options) { }
    }
}
