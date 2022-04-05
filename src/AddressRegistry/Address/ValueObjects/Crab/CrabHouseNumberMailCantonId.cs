namespace AddressRegistry.Address.ValueObjects.Crab
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;

    public class CrabHouseNumberMailCantonId : IntegerValueObject<CrabHouseNumberMailCantonId>
    {
        public CrabHouseNumberMailCantonId([JsonProperty("value")] int houseNumberMailCantonId) : base(houseNumberMailCantonId) { }
    }
}
