namespace AddressRegistry.Tests.BackOffice.Infrastructure
{
    using System;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice.Abstractions;
    using global::AutoFixture;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using StreetName;

    public class TestBackOfficeContext : BackOfficeContext
    {
        // This needs to be here to please EF
        public TestBackOfficeContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public TestBackOfficeContext(DbContextOptions<BackOfficeContext> options)
            : base(options) { }

        public AddressPersistentIdStreetNamePersistentId AddStreetNamePersistentIdByAddressPersistentLocalIdToFixture(
            int persistentLocalId, int streetNamePersistentLocalId)
        {
            var item = new Fixture().Create<AddressPersistentIdStreetNamePersistentId>();

            item.AddressPersistentLocalId = persistentLocalId;
            item.StreetNamePersistentLocalId = streetNamePersistentLocalId;

            AddressPersistentIdStreetNamePersistentIds.Add(item);
            SaveChanges();
            return item;
        }


        public async Task<AddressPersistentIdStreetNamePersistentId> AddAddressPersistentIdStreetNamePersistentId(
            AddressPersistentLocalId addressPersistentLocalId,
            StreetNamePersistentLocalId streetNamePersistentId)
        {
            var addressPersistentIdStreetNamePersistentId = new AddressPersistentIdStreetNamePersistentId(addressPersistentLocalId, streetNamePersistentId);
            AddressPersistentIdStreetNamePersistentIds.Add(addressPersistentIdStreetNamePersistentId);
            await SaveChangesAsync();

            return addressPersistentIdStreetNamePersistentId;
        }
    }

    public class FakeBackOfficeContextFactory : IDesignTimeDbContextFactory<TestBackOfficeContext>
    {
        public TestBackOfficeContext CreateDbContext(params string[] args)
        {
            var builder = new DbContextOptionsBuilder<BackOfficeContext>().UseInMemoryDatabase(Guid.NewGuid().ToString());
            return new TestBackOfficeContext(builder.Options);
        }
    }
}
