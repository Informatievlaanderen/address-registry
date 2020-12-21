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
        internal const string AddressListViewCountName = "vw_AddressListCount";

        public DbSet<CrabIdToPersistentLocalIdItem> CrabIdToPersistentLocalIds { get; set; }

        public DbSet<AddressVersion.AddressVersion> AddressVersions { get; set; }
        public DbSet<AddressDetailItem> AddressDetail { get; set; }
        public DbSet<AddressListItem> AddressList { get; set; }
        public DbSet<AddressSyndicationItem> AddressSyndication { get; set; }
        public DbSet<RRStreetName> RRStreetNames { get; set; }
        public DbSet<KadStreetName> KadStreetNames { get; set; }
        public DbSet<RRAddress> RRAddresses { get; set; }

        public DbSet<AddressListViewCount> AddressListViewCount { get; set; }

        public LegacyContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public LegacyContext(DbContextOptions<LegacyContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder
                .Entity<AddressListViewCount>()
                .HasNoKey()
                .ToView(AddressListViewCountName, Schema.Legacy);

            modelBuilder
                .Entity<RRStreetName>()
                .HasNoKey()
                .ToView(RRStreetName.TableName, Schema.Legacy);

            modelBuilder
                .Entity<KadStreetName>()
                .HasNoKey()
                .ToView(KadStreetName.TableName, Schema.Legacy);

            modelBuilder
                .Entity<RRAddress>()
                .HasNoKey()
                .ToView(RRAddress.TableName, Schema.Legacy);
        }
    }
}
