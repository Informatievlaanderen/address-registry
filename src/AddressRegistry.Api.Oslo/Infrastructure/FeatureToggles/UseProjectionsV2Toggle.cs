namespace AddressRegistry.Api.Oslo.Infrastructure.FeatureToggles
{
    using FeatureToggle;

    public class UseProjectionsV2Toggle : IFeatureToggle
    {
        public bool FeatureEnabled { get; }

        public UseProjectionsV2Toggle(bool featureEnabled)
        {
            FeatureEnabled = featureEnabled;
        }
    }
}
