namespace AddressRegistry.Tests
{
    using Address;

    public class FakePersistentLocalIdGenerator : IPersistentLocalIdGenerator
    {
        private int _persistentLocalId = 0;

        public PersistentLocalId GenerateNextPersistentLocalId()
        {
            return new PersistentLocalId(++_persistentLocalId);
        }
    }
}
