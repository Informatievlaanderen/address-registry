namespace AddressRegistry.Projections.AddressMatch
{
    using AddressDetailV2WithParent;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;

    public class AddressMatchContext : RunnerDbContext<AddressMatchContext>
    {
        public override string ProjectionStateSchema => Schema.AddressMatch;

        public DbSet<AddressDetailItemV2WithParent> AddressDetailV2WithParent { get; set; }

        public AddressMatchContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public AddressMatchContext(DbContextOptions<AddressMatchContext> options)
            : base(options) { }
    }
}
