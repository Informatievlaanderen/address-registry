namespace AddressRegistry.Api.Legacy.AddressMatch.Matching
{
    public class ManualAddressMatchConfig
    {
        public ManualAddressMatchConfig(double fuzzyMatchThreshold, int maxStreetNamesThreshold)
        {
            FuzzyMatchThreshold = fuzzyMatchThreshold;
            MaxStreetNamesThreshold = maxStreetNamesThreshold;
        }

        public double FuzzyMatchThreshold { get; }

        public int MaxStreetNamesThreshold { get; }
    }
}
