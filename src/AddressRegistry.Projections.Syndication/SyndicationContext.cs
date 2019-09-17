namespace AddressRegistry.Projections.Syndication
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Municipality;
    using PostalInfo;
    using StreetName;
    using BuildingUnit;
    using Parcel;

    public class SyndicationContext : RunnerDbContext<SyndicationContext>
    {
        public override string ProjectionStateSchema => Schema.Syndication;

        public DbSet<MunicipalitySyndicationItem> MunicipalitySyndicationItems { get; set; }
        public DbSet<MunicipalityLatestItem> MunicipalityLatestItems { get; set; }
        public DbSet<StreetNameLatestItem> StreetNameLatestItems { get; set; }
        public DbSet<StreetNameSyndicationItem> StreetNameSyndicationItems { get; set; }
        public DbSet<PostalInfoLatestItem> PostalInfoLatestItems { get; set; }

        public DbSet<MunicipalityBosaItem> MunicipalityBosaItems { get; set; }
        public DbSet<StreetNameBosaItem> StreetNameBosaItems { get; set; }

        public DbSet<ParcelAddressMatchLatestItem> ParcelAddressMatchLatestItems { get; set; }
        public DbSet<BuildingUnitAddressMatchLatestItem> BuildingUnitAddressMatchLatestItems { get; set; }

        // This needs to be here to please EF
        public SyndicationContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public SyndicationContext(DbContextOptions<SyndicationContext> options)
            : base(options) { }
    }
}
