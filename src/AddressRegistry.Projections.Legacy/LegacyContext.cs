namespace AddressRegistry.Projections.Legacy
{
    using AddressDetailV2WithParent;
    using AddressSyndication;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;

    public class LegacyContext : RunnerDbContext<LegacyContext>
    {
        public override string ProjectionStateSchema => Schema.Legacy;

        public DbSet<AddressDetailItemV2WithParent> AddressDetailV2WithParent { get; set; }

        public DbSet<AddressSyndicationItem> AddressSyndication { get; set; }
        public DbSet<AddressBoxNumberSyndicationHelper> AddressBoxNumberSyndicationHelper { get; set; }

        public LegacyContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public LegacyContext(DbContextOptions<LegacyContext> options)
            : base(options) { }
    }
}
