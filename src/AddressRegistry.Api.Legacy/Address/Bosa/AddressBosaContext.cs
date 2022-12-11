namespace AddressRegistry.Api.Legacy.Address.Bosa
{
    using System.Reflection;
    using AddressRegistry.Consumer.Read.Municipality;
    using AddressRegistry.Consumer.Read.StreetName;
    using AddressRegistry.Projections.Legacy;
    using AddressRegistry.Projections.Legacy.AddressDetail;
    using AddressRegistry.Projections.Legacy.AddressDetailV2;
    using AddressRegistry.Projections.Syndication;
    using AddressRegistry.Projections.Syndication.Municipality;
    using AddressRegistry.Projections.Syndication.PostalInfo;
    using AddressRegistry.Projections.Syndication.StreetName;
    using Be.Vlaanderen.Basisregisters.EntityFrameworkCore.EntityTypeConfiguration;
    using Microsoft.EntityFrameworkCore;

    public class AddressBosaContext : DbContext
    {
        public DbSet<AddressDetailItem> AddressDetail { get; set; }
        public DbSet<AddressDetailItemV2> AddressDetailV2 { get; set; }
        public DbSet<MunicipalityBosaItem> MunicipalityBosaItems { get; set; }
        public DbSet<MunicipalityLatestItem> MunicipalityLatestItems { get; set; }
        public DbSet<Consumer.Read.Municipality.Projections.MunicipalityBosaItem> MunicipalityConsumerBosaItems { get; set; }
        public DbSet<StreetNameBosaItem> StreetNameBosaItems { get; set; }
        public DbSet<Consumer.Read.StreetName.Projections.StreetNameBosaItem> StreetNameConsumerBosaItems { get; set; }
        public DbSet<PostalInfoLatestItem> PostalInfoLatestItems { get; set; }

        public AddressBosaContext()
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public AddressBosaContext(DbContextOptions<AddressBosaContext> options)
            : base(options)
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.AddEntityConfigurationsFromAssembly(typeof(LegacyContext).GetTypeInfo().Assembly);
            modelBuilder.AddEntityConfigurationsFromAssembly(typeof(SyndicationContext).GetTypeInfo().Assembly);
            modelBuilder.AddEntityConfigurationsFromAssembly(typeof(MunicipalityConsumerContext).GetTypeInfo().Assembly);
            modelBuilder.AddEntityConfigurationsFromAssembly(typeof(StreetNameConsumerContext).GetTypeInfo().Assembly);
        }
    }
}
