namespace AddressRegistry.Address
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;

    public class PersistentLocalId : IntegerValueObject<PersistentLocalId>
    {
        public PersistentLocalId([JsonProperty("value")] int persistentLocalId) : base(persistentLocalId) { }
    }
}
