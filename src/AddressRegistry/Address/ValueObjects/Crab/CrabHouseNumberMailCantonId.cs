namespace AddressRegistry.Address.Crab
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;

    [Obsolete("This is a legacy valueobject and should not be used anymore.")]
    public class CrabHouseNumberMailCantonId : IntegerValueObject<CrabHouseNumberMailCantonId>
    {
        public CrabHouseNumberMailCantonId([JsonProperty("value")] int houseNumberMailCantonId) : base(houseNumberMailCantonId) { }
    }
}
