namespace AddressRegistry.Api.Oslo.Address
{
    using System.Reflection;
    using Consumer.Read.Municipality;
    using Consumer.Read.Postal;
    using Consumer.Read.StreetName;
    using Microsoft.EntityFrameworkCore;
    using Projections.Legacy;
    using Projections.Legacy.AddressList;
    using Projections.Legacy.AddressListV2;

    public class AddressQueryContext : DbContext
    {
        public DbSet<AddressListItem> AddressList { get; set; }
        public DbSet<AddressListItemV2> AddressListV2 { get; set; }
        public DbSet<Consumer.Read.Municipality.Projections.MunicipalityLatestItem> MunicipalityConsumerLatestItems { get; set; }
        public DbSet<Consumer.Read.StreetName.Projections.StreetNameLatestItem> StreetNameConsumerLatestItems { get; set; }
        public DbSet<Consumer.Read.Postal.Projections.PostalLatestItem> PostalConsumerLatestItems { get; set; }

        public DbSet<AddressListViewItem> AddressListView { get; set; }
        public DbSet<AddressListViewItemV2> AddressListViewV2 { get; set; }

        public AddressQueryContext()
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public AddressQueryContext(DbContextOptions<AddressQueryContext> options)
            : base(options)
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(LegacyContext).GetTypeInfo().Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(PostalConsumerContext).GetTypeInfo().Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(MunicipalityConsumerContext).GetTypeInfo().Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(StreetNameConsumerContext).GetTypeInfo().Assembly);
        }
    }
}
