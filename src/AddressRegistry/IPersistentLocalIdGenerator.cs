namespace AddressRegistry
{
    public interface IPersistentLocalIdGenerator
    {
        PersistentLocalId GenerateNextPersistentLocalId();
    }
}
