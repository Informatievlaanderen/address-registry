namespace AddressRegistry.StreetName.Exceptions
{
    public class StreetNameNotActiveException : AddressRegistryException
    {
        public StreetNameNotActiveException(int streetNameId)
            : base($"StreetName with Id '{streetNameId}' is not active.")
        { }
    }
}
