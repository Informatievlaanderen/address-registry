namespace AddressRegistry.StreetName.Exceptions
{
    public class ParentAddressNotFoundException : AddressRegistryException
    {
        public readonly string StreetNamePersistentLocalId;
        public readonly string HouseNumber;

        public ParentAddressNotFoundException(string streetNamePersistentLocalId, string houseNumber)
        {
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            HouseNumber = houseNumber;
        }
    }
}
