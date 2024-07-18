namespace AddressRegistry.Tests.BackOffice.Infrastructure
{
    using System;
    using Consumer.Read.Postal;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;

    public class FakePostalConsumerContext : PostalConsumerContext
    {
        // This needs to be here to please EF
        public FakePostalConsumerContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public FakePostalConsumerContext(DbContextOptions<PostalConsumerContext> options)
            : base(options) { }
    }

    public class FakePostalConsumerContextFactory : IDesignTimeDbContextFactory<FakePostalConsumerContext>
    {
        public FakePostalConsumerContext CreateDbContext(params string[] args)
        {
            var builder = new DbContextOptionsBuilder<PostalConsumerContext>().UseInMemoryDatabase(Guid.NewGuid().ToString());
            return new FakePostalConsumerContext(builder.Options);
        }
    }
}
