namespace AddressRegistry.Address.ValueObjects.Crab
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;

    public class CrabSubaddressStatusId : IntegerValueObject<CrabSubaddressStatusId>
    {
        public CrabSubaddressStatusId([JsonProperty("value")] int subaddressStatusId) : base(subaddressStatusId) { }
    }
}
