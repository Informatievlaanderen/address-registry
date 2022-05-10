namespace AddressRegistry.Tests.BackOffice.Infrastructure
{
    using System;
    using Consumer;
    using Consumer.StreetName;
    using global::AutoFixture;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;

    public class TestConsumerContext : ConsumerContext
    {
        // This needs to be here to please EF
        public TestConsumerContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public TestConsumerContext(DbContextOptions<ConsumerContext> options)
            : base(options) { }

        public StreetNameConsumerItem AddStreetNameConsumerItemFixture()
        {
            var streetNameConsumerItem = new Fixture().Create<StreetNameConsumerItem>();
            StreetNameConsumerItems.Add(streetNameConsumerItem);
            SaveChanges();
            return streetNameConsumerItem;
        }

        public StreetNameConsumerItem AddStreetNameConsumerItemFixtureWithPersistentLocalId(int persistentLocalId)
        {
            var streetNameConsumerItem = new Fixture().Create<StreetNameConsumerItem>();
            streetNameConsumerItem.PersistentLocalId = persistentLocalId;
            StreetNameConsumerItems.Add(streetNameConsumerItem);
            SaveChanges();
            return streetNameConsumerItem;
        }

        public StreetNameConsumerItem AddStreetNameConsumerItemFixtureWithPersistentLocalIdAndStreetNameId(
            Guid streetNameId, int persistentLocalId)
        {
            var streetNameConsumerItem = new Fixture().Create<StreetNameConsumerItem>();
            streetNameConsumerItem.PersistentLocalId = persistentLocalId;
            streetNameConsumerItem.StreetNameId = streetNameId;
            StreetNameConsumerItems.Add(streetNameConsumerItem);
            SaveChanges();
            return streetNameConsumerItem;
        }
    }

    public class FakeConsumerContextFactory : IDesignTimeDbContextFactory<TestConsumerContext>
    {
        public TestConsumerContext CreateDbContext(params string[] args)
        {
            var builder = new DbContextOptionsBuilder<ConsumerContext>().UseInMemoryDatabase(Guid.NewGuid().ToString());
            return new TestConsumerContext(builder.Options);
        }
    }
}
