namespace AddressRegistry.Projector.Infrastructure
{
    using FeatureToggle;

    public class FeatureToggleOptions
    {
        public const string ConfigurationKey = "FeatureToggles";
        public bool UseProjectionsV2 { get; set; }
    }

    public class UseProjectionsV2Toggle : IFeatureToggle
    {
        public bool FeatureEnabled { get; }

        public UseProjectionsV2Toggle(bool featureEnabled)
        {
            FeatureEnabled = featureEnabled;
        }
    }
}
