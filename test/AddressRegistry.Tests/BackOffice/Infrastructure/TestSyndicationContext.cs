namespace AddressRegistry.Tests.BackOffice.Infrastructure
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Projections.Syndication;
    using Projections.Syndication.PostalInfo;

    public class TestSyndicationContext : SyndicationContext
    {
        // This needs to be here to please EF
        public TestSyndicationContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public TestSyndicationContext(DbContextOptions<SyndicationContext> options)
            : base(options) { }

        public void AddPostalInfoLatestItem(PostalInfoLatestItem item)
        {
            PostalInfoLatestItems.Add(item);
            SaveChanges();
        }
    }

    public class FakeSyndicationContextFactory : IDesignTimeDbContextFactory<TestSyndicationContext>
    {
        public TestSyndicationContext CreateDbContext(params string[] args)
        {
            var builder = new DbContextOptionsBuilder<SyndicationContext>().UseInMemoryDatabase(Guid.NewGuid().ToString());
            return new TestSyndicationContext(builder.Options);
        }
    }
}
