namespace AddressRegistry.Api.Legacy.AddressMatch
{
    using System.Reflection;
    using AddressRegistry.Infrastructure;
    using Be.Vlaanderen.Basisregisters.EntityFrameworkCore.EntityTypeConfiguration;
    using Consumer.Read.Municipality;
    using Consumer.Read.StreetName;
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
        public DbSet<Consumer.Read.Municipality.Projections.MunicipalityLatestItem> MunicipalityConsumerLatestItems { get; set; }
        public DbSet<StreetNameLatestItem> StreetNameLatestItems { get; set; }
        public DbSet<Consumer.Read.StreetName.Projections.StreetNameLatestItem> StreetNameConsumerLatestItems { get; set; }
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
            modelBuilder.AddEntityConfigurationsFromAssembly(typeof(MunicipalityConsumerContext).GetTypeInfo().Assembly);
            modelBuilder.AddEntityConfigurationsFromAssembly(typeof(StreetNameConsumerContext).GetTypeInfo().Assembly);

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
