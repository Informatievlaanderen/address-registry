namespace AddressRegistry.Address
{
    public interface IPersistentLocalIdGenerator
    {
        PersistentLocalId GenerateNextPersistentLocalId();
    }
}
