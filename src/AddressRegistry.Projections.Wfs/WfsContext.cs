namespace AddressRegistry.Projections.Wfs
{
    using AddressWfsV2;
    using AddressWfsV3;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;

    public class WfsContext : RunnerDbContext<WfsContext>
    {
        public override string ProjectionStateSchema => Schema.Wfs;
        public DbSet<AddressWfsV2Item> AddressWfsV2Items { get; set; }

        /// <summary>
        /// The Lambert 2008 (EPSG 3812) counterpart of <see cref="AddressWfsV2Items"/>. Both are kept in
        /// parallel until the geoserver consumers have moved over, after which V2 goes. See ADR 0004.
        /// </summary>
        public DbSet<AddressWfsV3Item> AddressWfsV3Items { get; set; }

        public WfsContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public WfsContext(DbContextOptions<WfsContext> options)
            : base(options) { }
    }
}
