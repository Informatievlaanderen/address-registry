namespace AddressRegistry.Tests
{
    using Address;

    public class FakeOsloIdGenerator : IOsloIdGenerator
    {
        public OsloId GenerateNextOsloId()
        {
            return new OsloId(1);
        }
    }
}
