namespace AddressRegistry.StreetName.Exceptions
{
    public class AddressNotFoundException : AddressRegistryException
    {
        public AddressNotFoundException(int addressPersistentLocalId)
            : base($"Address with Id '{addressPersistentLocalId}' has not been found.")
        { }
    }
}
