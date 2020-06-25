namespace AddressRegistry.Api.Legacy.AddressMatch.Matching
{
    public class ManualAddressMatchConfig
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
