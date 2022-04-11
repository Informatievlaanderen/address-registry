namespace AddressRegistry.Tests
{
    using Address;

    public class FakePersistentLocalIdGenerator : IPersistentLocalIdGenerator
    {
        public PersistentLocalId GenerateNextPersistentLocalId()
        {
            return new PersistentLocalId(1);
        }
    }
}
