namespace AddressRegistry.Api.Extract.Infrastructure.FeatureToggles
{
    using FeatureToggle;

    public class UseExtractV2Toggle : IFeatureToggle
    {
        public bool FeatureEnabled { get; }

        public UseExtractV2Toggle(bool featureEnabled)
        {
            FeatureEnabled = featureEnabled;
        }
    }
}
