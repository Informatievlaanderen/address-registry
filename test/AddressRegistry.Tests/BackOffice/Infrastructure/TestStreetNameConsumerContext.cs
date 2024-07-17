namespace AddressRegistry.Tests.BackOffice.Infrastructure
{
    using System;
    using Consumer.Read.StreetName;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;

    public class TestStreetNameConsumerContext : StreetNameConsumerContext
    {
        // This needs to be here to please EF
        public TestStreetNameConsumerContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public TestStreetNameConsumerContext(DbContextOptions<StreetNameConsumerContext> options)
            : base(options) { }
    }

    public class FakeStreetNameConsumerContextFactory : IDesignTimeDbContextFactory<TestStreetNameConsumerContext>
    {
        public TestStreetNameConsumerContext CreateDbContext(params string[] args)
        {
            var builder = new DbContextOptionsBuilder<StreetNameConsumerContext>().UseInMemoryDatabase(Guid.NewGuid().ToString());
            return new TestStreetNameConsumerContext(builder.Options);
        }
    }
}
