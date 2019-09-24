namespace AddressRegistry.Projections.Legacy
{
    using AddressDetail;
    using AddressList;
    using AddressMatch;
    using AddressSyndication;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using CrabIdToPersistentLocalId;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;

    public class LegacyContext : RunnerDbContext<LegacyContext>
    {
        public override string ProjectionStateSchema => Schema.Legacy;

        public DbSet<CrabIdToPersistentLocalIdItem> CrabIdToPersistentLocalIds { get; set; }

        public DbSet<AddressVersion.AddressVersion> AddressVersions { get; set; }
        public DbSet<AddressDetailItem> AddressDetail { get; set; }
        public DbSet<AddressListItem> AddressList { get; set; }
        public DbSet<AddressSyndicationItem> AddressSyndication { get; set; }
        public DbQuery<RRStreetName> RRStreetNames { get; set; }
        public DbQuery<KadStreetName> KadStreetNames { get; set; }
        public DbQuery<RRAddress> RRAddresses { get; set; }

        public LegacyContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public LegacyContext(DbContextOptions<LegacyContext> options)
            : base(options) { }
    }
}
