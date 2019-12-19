namespace AddressRegistry.Api.Legacy.AddressMatch
{
    using System.Reflection;
    using Address.Query;
    using Be.Vlaanderen.Basisregisters.EntityFrameworkCore.EntityTypeConfiguration;
    using Microsoft.EntityFrameworkCore;
    using Projections.Legacy;
    using Projections.Legacy.AddressDetail;
    using Projections.Legacy.AddressMatch;
    using Projections.Legacy.CrabIdToPersistentLocalId;
    using Projections.Syndication;
    using Projections.Syndication.BuildingUnit;
    using Projections.Syndication.Municipality;
    using Projections.Syndication.Parcel;
    using Projections.Syndication.PostalInfo;
    using Projections.Syndication.StreetName;

    public class AddressMatchContext : DbContext
    {
        public DbSet<AddressDetailItem> AddressDetail { get; set; }
        public DbSet<MunicipalityLatestItem> MunicipalityLatestItems { get; set; }
        public DbSet<StreetNameLatestItem> StreetNameLatestItems { get; set; }
        public DbSet<PostalInfoLatestItem> PostalInfoLatestItems { get; set; }
        public DbSet<BuildingUnitAddressMatchLatestItem> BuildingUnitAddressMatchLatestItems { get; set; }
        public DbSet<ParcelAddressMatchLatestItem> ParcelAddressMatchLatestItems { get; set; }
        public DbSet<CrabIdToPersistentLocalIdItem> CrabIdToPersistentLocalIds { get; set; }

        public DbQuery<RRStreetName> RRStreetNames { get; set; }
        public DbQuery<KadStreetName> KadStreetNames { get; set; }
        public DbQuery<RRAddress> RRAddresses { get; set; }

        public AddressMatchContext()
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public AddressMatchContext(DbContextOptions<AddressMatchContext> options)
            : base(options)
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.AddEntityConfigurationsFromAssembly(typeof(LegacyContext).GetTypeInfo().Assembly);
            modelBuilder.AddEntityConfigurationsFromAssembly(typeof(SyndicationContext).GetTypeInfo().Assembly);
        }
    }
}
