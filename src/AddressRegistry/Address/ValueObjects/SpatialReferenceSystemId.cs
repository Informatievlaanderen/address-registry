namespace AddressRegistry.Address
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;

    [Obsolete("This is a legacy valueobject and should not be used anymore.")]
    public class SpatialReferenceSystemId : IntegerValueObject<SpatialReferenceSystemId>
    {
        public static SpatialReferenceSystemId Lambert72 => new SpatialReferenceSystemId(31370);

        public SpatialReferenceSystemId([JsonProperty("value")] int spatialReferenceSystemId) : base(spatialReferenceSystemId) { }
    }
}
