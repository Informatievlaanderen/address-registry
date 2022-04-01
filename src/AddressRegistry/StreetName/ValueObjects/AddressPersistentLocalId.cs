namespace AddressRegistry.StreetName
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;

    public class AddressPersistentLocalId : IntegerValueObject<AddressPersistentLocalId>
    {
        public AddressPersistentLocalId([JsonProperty("value")] int addressPersistentLocalId) : base(addressPersistentLocalId) { }
    }
}
