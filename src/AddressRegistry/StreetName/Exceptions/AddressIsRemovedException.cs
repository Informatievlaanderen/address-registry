namespace AddressRegistry.StreetName.Exceptions
{
    public class AddressIsRemovedException : AddressRegistryException
    {
        public AddressIsRemovedException(int addressPersistentLocalId)
            : base($"Address with Id '{addressPersistentLocalId}' is removed.")
        { }
    }
}
