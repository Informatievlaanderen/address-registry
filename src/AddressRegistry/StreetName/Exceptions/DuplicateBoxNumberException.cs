namespace AddressRegistry.StreetName.Exceptions
{
    public class DuplicateBoxNumberException : AddressRegistryException
    {
        public DuplicateBoxNumberException(string boxNumber)
            : base($"Attempt to add child address with duplicate boxnumber '{boxNumber}'.")
        { }
    }
}
