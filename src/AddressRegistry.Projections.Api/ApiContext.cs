namespace AddressRegistry.Projections.Api
{
    using AddressDetail;
    using AddressSyndication;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;

    public class ApiContext : RunnerDbContext<ApiContext>
    {
        public override string ProjectionStateSchema => Schema.Api;
        internal const string AddressListViewCountName = "vw_AddressListCount";
        internal const string AddressListViewCountNameV2 = "vw_AddressListCountV2";

        public DbSet<AddressDetailItem> AddressDetails { get; set; }

        public DbSet<AddressSyndicationItem> AddressSyndication { get; set; }
        public DbSet<AddressBoxNumberSyndicationHelper> AddressBoxNumberSyndicationHelper { get; set; }

        public ApiContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public ApiContext(DbContextOptions<ApiContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
