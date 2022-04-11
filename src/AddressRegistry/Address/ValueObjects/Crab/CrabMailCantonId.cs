namespace AddressRegistry.Address.Crab
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;

    public class CrabMailCantonId : IntegerValueObject<CrabMailCantonId>
    {
        public CrabMailCantonId([JsonProperty("value")] int mailCantonId) : base(mailCantonId) { }
    }
}
