namespace AddressRegistry.Api.Legacy.AddressMatch.V2
{
    using System.Reflection;
    using Consumer.Read.Municipality;
    using Consumer.Read.Municipality.Projections;
    using Consumer.Read.StreetName;
    using Consumer.Read.StreetName.Projections;
    using Microsoft.EntityFrameworkCore;
    using Projections.Legacy;
    using Projections.Legacy.AddressDetailV2;
    using Projections.Syndication;
    using Projections.Syndication.PostalInfo;

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
