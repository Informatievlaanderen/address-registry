namespace AddressRegistry.StreetName.Exceptions
{
    public class StreetNameIsRemovedException : AddressRegistryException
    {
        public StreetNameIsRemovedException(int streetNameId)
            : base($"StreetName with Id '{streetNameId}' is removed.")
        { }
    }
}
