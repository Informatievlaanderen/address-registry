namespace AddressRegistry.Projections.Wms
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;

    public class WmsContext : RunnerDbContext<WmsContext>
    {
        public override string ProjectionStateSchema => Schema.Wms;
        public DbSet<AddressWmsItemV2.AddressWmsItemV2> AddressWmsItemsV2 => Set<AddressWmsItemV2.AddressWmsItemV2>();

        public WmsContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public WmsContext(DbContextOptions<WmsContext> options)
            : base(options) { }
    }
}
