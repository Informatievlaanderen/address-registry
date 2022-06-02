namespace AddressRegistry.StreetName
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    public class AddressPersistentLocalId : IntegerValueObject<AddressPersistentLocalId>
    {
        public AddressPersistentLocalId(int addressPersistentLocalId) : base(addressPersistentLocalId) { }
    }
}
