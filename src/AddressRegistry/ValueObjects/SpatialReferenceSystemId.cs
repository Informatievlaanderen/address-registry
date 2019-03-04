namespace AddressRegistry
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;

    public class SpatialReferenceSystemId : IntegerValueObject<SpatialReferenceSystemId>
    {
        public static SpatialReferenceSystemId Lambert72 => new SpatialReferenceSystemId(31370);

        public SpatialReferenceSystemId([JsonProperty("value")] int spatialReferenceSystemId) : base(spatialReferenceSystemId) { }
    }
}
