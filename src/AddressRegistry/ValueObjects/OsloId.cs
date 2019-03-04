namespace AddressRegistry
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;

    public class OsloId : IntegerValueObject<OsloId>
    {
        public OsloId([JsonProperty("value")] int osloId) : base(osloId) { }
    }
}
