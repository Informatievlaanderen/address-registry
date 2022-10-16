namespace AddressRegistry.Projections.Wms
{
    using System;
    using AddressDetail;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;

    public class WmsContext : RunnerDbContext<WmsContext>
    {
        public override string ProjectionStateSchema => Schema.Wms;
        public DbSet<AddressDetailItem> AddressDetail { get; set; }
        public DbSet<AddressWmsItem.AddressWmsItem> AddressWmsItems { get; set; }

        public DbSet<T> Get<T>() where T : class, new()
        {
            if (typeof(T) == typeof(AddressDetailItem))
                return (AddressDetail as DbSet<T>)!;

            throw new NotImplementedException($"DbSet not found of type {typeof(T)}");
        }

        public WmsContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public WmsContext(DbContextOptions<WmsContext> options)
            : base(options) { }
    }
}
