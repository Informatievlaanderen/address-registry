namespace AddressRegistry.Projections.Wms
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;

    public class WmsContext : RunnerDbContext<WmsContext>
    {
        public override string ProjectionStateSchema => Schema.Wms;
        public DbSet<AddressWmsItemV3.AddressWmsItemV3> AddressWmsItemsV3 => Set<AddressWmsItemV3.AddressWmsItemV3>();

        /// <summary>
        /// The Lambert 2008 (EPSG 3812) counterpart of <see cref="AddressWmsItemsV3"/>. Both are kept in
        /// parallel until the geoserver consumers have moved over, after which V3 goes. See ADR 0004.
        /// </summary>
        public DbSet<AddressWmsItemV4.AddressWmsItemV4> AddressWmsItemsV4 => Set<AddressWmsItemV4.AddressWmsItemV4>();

        public WmsContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public WmsContext(DbContextOptions<WmsContext> options)
            : base(options) { }
    }
}
