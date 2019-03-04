namespace AddressRegistry.Crab
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;

    public class CrabAddressPositionId : IntegerValueObject<CrabAddressPositionId>
    {
        public CrabAddressPositionId([JsonProperty("value")] int addressPositionId) : base(addressPositionId) { }
    }
}
