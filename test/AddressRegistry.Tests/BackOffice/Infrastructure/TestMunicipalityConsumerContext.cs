namespace AddressRegistry.Tests.BackOffice.Infrastructure
{
    using System;
    using AddressRegistry.Api.BackOffice.Abstractions;
    using Consumer.Read.Municipality;
    using Consumer.Read.Municipality.Projections;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using StreetName;

    public class TestMunicipalityConsumerContext : MunicipalityConsumerContext
    {
        // This needs to be here to please EF
        public TestMunicipalityConsumerContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public TestMunicipalityConsumerContext(DbContextOptions<MunicipalityConsumerContext> options)
            : base(options) { }

        public MunicipalityLatestItem AddMunicipality(MunicipalityId municipalityId, string gml)
        {
            var item = new MunicipalityLatestItem
            {
                MunicipalityId = (Guid)municipalityId,
                ExtendedWkbGeometry = gml.ToExtendedWkbGeometry()
            };

            MunicipalityLatestItems.Add(item);
            SaveChanges();

            return item;
        }
    }

    public class FakeMunicipalityConsumerContextFactory : IDesignTimeDbContextFactory<TestMunicipalityConsumerContext>
    {
        public TestMunicipalityConsumerContext CreateDbContext(params string[] args)
        {
            var builder = new DbContextOptionsBuilder<MunicipalityConsumerContext>().UseInMemoryDatabase(Guid.NewGuid().ToString());
            return new TestMunicipalityConsumerContext(builder.Options);
        }
    }
}
