namespace AddressRegistry.StreetName
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;

    public class StreetNamePersistentLocalId : IntegerValueObject<StreetNamePersistentLocalId>
    {
        public StreetNamePersistentLocalId([JsonProperty("value")] int streetNamePersistentLocalId)
            : base(streetNamePersistentLocalId)
        { }
    }
}
