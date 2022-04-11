namespace AddressRegistry.Address.Crab
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;

    public class CrabHouseNumberStatusId : IntegerValueObject<CrabHouseNumberStatusId>
    {
        public CrabHouseNumberStatusId([JsonProperty("value")] int houseNumberStatusId) : base(houseNumberStatusId) { }
    }
}
