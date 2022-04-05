namespace AddressRegistry.Address
{
    using ValueObjects;

    public interface IPersistentLocalIdGenerator
    {
        PersistentLocalId GenerateNextPersistentLocalId();
    }
}
