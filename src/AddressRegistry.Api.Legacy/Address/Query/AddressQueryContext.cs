namespace AddressRegistry.Api.Legacy.Address.Query
{
    using System.Reflection;
    using Be.Vlaanderen.Basisregisters.EntityFrameworkCore.EntityTypeConfiguration;
    using Microsoft.EntityFrameworkCore;
    using Projections.Legacy;
    using Projections.Legacy.AddressList;
    using Projections.Syndication;
    using Projections.Syndication.Municipality;
    using Projections.Syndication.StreetName;

    public class AddressQueryContext : DbContext
    {
        public DbSet<AddressListItem> AddressList { get; set; }
        public DbSet<MunicipalityLatestItem> MunicipalityLatestItems { get; set; }
        public DbSet<StreetNameLatestItem> StreetNameLatestItems { get; set; }

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

            modelBuilder.AddEntityConfigurationsFromAssembly(typeof(LegacyContext).GetTypeInfo().Assembly);
            modelBuilder.AddEntityConfigurationsFromAssembly(typeof(SyndicationContext).GetTypeInfo().Assembly);
        }
    }
}
