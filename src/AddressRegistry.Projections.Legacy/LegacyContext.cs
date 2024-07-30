namespace AddressRegistry.Projections.Legacy
{
    using AddressDetailV2WithParent;
    using AddressListV2;
    using AddressSyndication;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;

    public class LegacyContext : RunnerDbContext<LegacyContext>
    {
        public override string ProjectionStateSchema => Schema.Legacy;
        internal const string AddressListViewCountName = "vw_AddressListCount";
        internal const string AddressListViewCountNameV2 = "vw_AddressListCountV2";

        public DbSet<AddressDetailItemV2WithParent> AddressDetailV2WithParent { get; set; }
        public DbSet<AddressListItemV2> AddressListV2 { get; set; }

        public DbSet<AddressSyndicationItem> AddressSyndication { get; set; }
        public DbSet<AddressBoxNumberSyndicationHelper> AddressBoxNumberSyndicationHelper { get; set; }
        public DbSet<AddressListViewCountV2> AddressListViewCountV2 { get; set; }

        public LegacyContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public LegacyContext(DbContextOptions<LegacyContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<AddressListViewCountV2>()
                .HasNoKey()
                .ToView(AddressListViewCountNameV2, Schema.Legacy);
        }
    }
}
