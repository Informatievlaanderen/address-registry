namespace AddressRegistry.Projections.Wfs
{
    using AddressDetail;
    using AddressWfs;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;

    public class WfsContext : RunnerDbContext<WfsContext>
    {
        public override string ProjectionStateSchema => Schema.Wfs;
        public DbSet<AddressDetailItem> AddressDetail { get; set; }
        public DbSet<AddressWfsItem> AddressWfsItems { get; set; }


        public WfsContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public WfsContext(DbContextOptions<WfsContext> options)
            : base(options) { }
    }
}
