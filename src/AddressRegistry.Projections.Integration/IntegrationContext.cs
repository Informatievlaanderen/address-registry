namespace AddressRegistry.Projections.Integration
{
    using AddressRegistry.Infrastructure;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using LatestItem;
    using LatestItemV2;
    using Merger;
    using Microsoft.EntityFrameworkCore;
    using Version;

    public class IntegrationContext : RunnerDbContext<IntegrationContext>
    {
        public override string ProjectionStateSchema => Schema.Integration;

        public DbSet<AddressLatestItem> AddressLatestItems => Set<AddressLatestItem>();
        public DbSet<AddressLatestItemV2> AddressLatestItemsV2 => Set<AddressLatestItemV2>();
        public DbSet<AddressVersion> AddressVersions => Set<AddressVersion>();
        public DbSet<AddressIdAddressPersistentLocalIdRelation> AddressIdAddressPersistentLocalIds => Set<AddressIdAddressPersistentLocalIdRelation>();
        public DbSet<AddressMergerItem> AddressMergerItems => Set<AddressMergerItem>();

        // This needs to be here to please EF
        public IntegrationContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public IntegrationContext(DbContextOptions<IntegrationContext> options)
            : base(options) { }
    }
}
