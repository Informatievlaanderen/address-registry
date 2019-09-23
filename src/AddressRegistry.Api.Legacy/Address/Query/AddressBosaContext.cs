namespace AddressRegistry.Api.Legacy.Address.Query
{
    using System.Reflection;
    using Be.Vlaanderen.Basisregisters.EntityFrameworkCore.EntityTypeConfiguration;
    using Microsoft.EntityFrameworkCore;
    using Projections.Legacy;
    using Projections.Legacy.AddressDetail;
    using Projections.Syndication;
    using Projections.Syndication.Municipality;
    using Projections.Syndication.PostalInfo;
    using Projections.Syndication.StreetName;

    public class AddressBosaContext : DbContext
    {
        public DbSet<AddressDetailItem> AddressDetail { get; set; }
        public DbSet<MunicipalityBosaItem> MunicipalityBosaItems { get; set; }
        public DbSet<StreetNameBosaItem> StreetNameBosaItems { get; set; }
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
        }
    }
}
