namespace AddressRegistry.Address.Crab
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;

    [Obsolete("This is a legacy valueobject and should not be used anymore.")]
    public class CrabHouseNumberStatusId : IntegerValueObject<CrabHouseNumberStatusId>
    {
        public CrabHouseNumberStatusId([JsonProperty("value")] int houseNumberStatusId) : base(houseNumberStatusId) { }
    }
}
