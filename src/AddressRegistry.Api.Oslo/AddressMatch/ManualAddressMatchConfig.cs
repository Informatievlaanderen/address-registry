namespace AddressRegistry.Api.Oslo.AddressMatch
{
    public sealed class ManualAddressMatchConfig
    {
        public double FuzzyMatchThreshold { get; }

        public int MaxStreetNamesThreshold { get; }

        public ManualAddressMatchConfig(double fuzzyMatchThreshold, int maxStreetNamesThreshold)
        {
            FuzzyMatchThreshold = fuzzyMatchThreshold;
            MaxStreetNamesThreshold = maxStreetNamesThreshold;
        }
    }
}
