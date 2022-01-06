namespace AddressRegistry.Projections.Wfs
{
    using System;
    using AddressDetail;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;

    public class WfsContext : RunnerDbContext<WfsContext>
    {
        public override string ProjectionStateSchema => Schema.Wfs;
        public DbSet<AddressDetailItem> AddressDetail { get; set; }

        public DbSet<T> Get<T>() where T : class, new()
        {
            if (typeof(T) == typeof(AddressDetailItem))
                return (AddressDetail as DbSet<T>)!;

            throw new NotImplementedException($"DbSet not found of type {typeof(T)}");
        }

        public WfsContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public WfsContext(DbContextOptions<WfsContext> options)
            : base(options) { }
    }
}
