namespace AddressRegistry.Projections.Extract
{
    using AddressExtract;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;

    public class ExtractContext : RunnerDbContext<ExtractContext>
    {
        public override string ProjectionStateSchema => Schema.Extract;

        public DbSet<AddressExtractItem> AddressExtract { get; set; }

        // This needs to be here to please EF
        public ExtractContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public ExtractContext(DbContextOptions<ExtractContext> options)
            : base(options) { }
    }
}
