namespace AddressRegistry.Address.Crab
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;

    public class CrabMailCantonCode : StringValueObject<CrabMailCantonCode>
    {
        public CrabMailCantonCode([JsonProperty("value")] string mailCantonCode) : base(mailCantonCode) { }
    }
}
