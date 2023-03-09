namespace AddressRegistry.Api.Oslo.AddressMatch.V1
{
    using System.Reflection;
    using AddressRegistry.Infrastructure;
    using AddressRegistry.Projections.Legacy;
    using AddressRegistry.Projections.Legacy.AddressDetail;
    using AddressRegistry.Projections.Legacy.AddressMatch;
    using AddressRegistry.Projections.Legacy.CrabIdToPersistentLocalId;
    using AddressRegistry.Projections.Syndication;
    using AddressRegistry.Projections.Syndication.BuildingUnit;
    using AddressRegistry.Projections.Syndication.Municipality;
    using AddressRegistry.Projections.Syndication.Parcel;
    using AddressRegistry.Projections.Syndication.PostalInfo;
    using AddressRegistry.Projections.Syndication.StreetName;
    using Be.Vlaanderen.Basisregisters.EntityFrameworkCore.EntityTypeConfiguration;
    using Microsoft.EntityFrameworkCore;

    public class AddressMatchContext : DbContext
    {
        public DbSet<AddressDetailItem> AddressDetail { get; set; }
        public DbSet<MunicipalityLatestItem> MunicipalityLatestItems { get; set; }
        public DbSet<StreetNameLatestItem> StreetNameLatestItems { get; set; }
        public DbSet<PostalInfoLatestItem> PostalInfoLatestItems { get; set; }
        public DbSet<BuildingUnitAddressMatchLatestItem> BuildingUnitAddressMatchLatestItems { get; set; }
        public DbSet<ParcelAddressMatchLatestItem> ParcelAddressMatchLatestItems { get; set; }
        public DbSet<CrabIdToPersistentLocalIdItem> CrabIdToPersistentLocalIds { get; set; }

        public DbSet<RRStreetName> RRStreetNames { get; set; }
        public DbSet<KadStreetName> KadStreetNames { get; set; }
        public DbSet<RRAddress> RRAddresses { get; set; }

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
