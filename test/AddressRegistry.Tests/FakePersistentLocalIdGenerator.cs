namespace AddressRegistry.Tests
{
    using Address;
    using Address.ValueObjects;

    public class FakePersistentLocalIdGenerator : IPersistentLocalIdGenerator
    {
        public PersistentLocalId GenerateNextPersistentLocalId()
        {
            return new PersistentLocalId(1);
        }
    }
}
