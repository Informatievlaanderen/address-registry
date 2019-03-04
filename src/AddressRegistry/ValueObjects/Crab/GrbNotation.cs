namespace AddressRegistry.Crab
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;

    public class GrbNotation : StringValueObject<GrbNotation>
    {
        public GrbNotation([JsonProperty("value")] string grbNotation) : base(grbNotation) { }
    }
}
