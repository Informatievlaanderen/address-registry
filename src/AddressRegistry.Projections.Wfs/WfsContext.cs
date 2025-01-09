namespace AddressRegistry.Projections.Wfs
{
    using AddressWfs;
    using AddressWfsV2;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;

    public class WfsContext : RunnerDbContext<WfsContext>
    {
        public override string ProjectionStateSchema => Schema.Wfs;
        public DbSet<AddressWfsItem> AddressWfsItems { get; set; }
        public DbSet<AddressWfsV2Item> AddressWfsV2Items { get; set; }


        public WfsContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public WfsContext(DbContextOptions<WfsContext> options)
            : base(options) { }
    }
}
