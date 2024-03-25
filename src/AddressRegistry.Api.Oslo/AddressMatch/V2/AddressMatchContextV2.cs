namespace AddressRegistry.Api.Oslo.AddressMatch.V2
{
    using System.Reflection;
    using AddressRegistry.Consumer.Read.Municipality;
    using AddressRegistry.Consumer.Read.Municipality.Projections;
    using AddressRegistry.Consumer.Read.StreetName;
    using AddressRegistry.Consumer.Read.StreetName.Projections;
    using AddressRegistry.Projections.Legacy;
    using AddressRegistry.Projections.Legacy.AddressDetailV2;
    using AddressRegistry.Projections.Syndication;
    using AddressRegistry.Projections.Syndication.PostalInfo;
    using Microsoft.EntityFrameworkCore;

    public class AddressMatchContextV2 : DbContext
    {
        public DbSet<AddressDetailItemV2> AddressDetailV2 { get; set; }
        public DbSet<MunicipalityLatestItem> MunicipalityLatestItems { get; set; }
        public DbSet<StreetNameLatestItem> StreetNameLatestItems { get; set; }
        public DbSet<PostalInfoLatestItem> PostalInfoLatestItems { get; set; }

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
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SyndicationContext).GetTypeInfo().Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(MunicipalityConsumerContext).GetTypeInfo().Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(StreetNameConsumerContext).GetTypeInfo().Assembly);
        }
    }
}
