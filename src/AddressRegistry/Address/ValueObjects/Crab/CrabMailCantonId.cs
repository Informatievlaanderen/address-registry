namespace AddressRegistry.Address.Crab
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;

    [Obsolete("This is a legacy valueobject and should not be used anymore.")]
    public class CrabMailCantonId : IntegerValueObject<CrabMailCantonId>
    {
        public CrabMailCantonId([JsonProperty("value")] int mailCantonId) : base(mailCantonId) { }
    }
}
