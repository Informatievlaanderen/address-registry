namespace AddressRegistry.Producer
{
    using Microsoft.EntityFrameworkCore;
    using AddressRegistry.Infrastructure;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;

    public class ProducerContext : RunnerDbContext<ProducerContext>
    {
        public override string ProjectionStateSchema => Schema.Producer;

        // This needs to be here to please EF
        public ProducerContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public ProducerContext(DbContextOptions<ProducerContext> options)
            : base(options) { }
    }
}
