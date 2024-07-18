namespace AddressRegistry.Api.Oslo.AddressMatch.V2
{
    using System.Reflection;
    using Consumer.Read.Municipality;
    using Consumer.Read.Municipality.Projections;
    using Consumer.Read.Postal;
    using Consumer.Read.Postal.Projections;
    using Consumer.Read.StreetName;
    using Consumer.Read.StreetName.Projections;
    using Microsoft.EntityFrameworkCore;
    using Projections.Legacy;
    using Projections.Legacy.AddressDetailV2WithParent;

    public class AddressMatchContextV2 : DbContext
    {
        public DbSet<AddressDetailItemV2WithParent> AddressDetailV2WithParent { get; set; }
        public DbSet<MunicipalityLatestItem> MunicipalityLatestItems { get; set; }
        public DbSet<StreetNameLatestItem> StreetNameLatestItems { get; set; }
        public DbSet<PostalLatestItem> PostalInfoLatestItems { get; set; }

        public AddressMatchContextV2()
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public AddressMatchContextV2(DbContextOptions<AddressMatchContextV2> options)
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
