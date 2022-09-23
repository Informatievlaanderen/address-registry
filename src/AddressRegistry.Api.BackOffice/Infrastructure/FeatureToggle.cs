namespace AddressRegistry.Api.BackOffice.Infrastructure
{
    using FeatureToggle;

    public class UseSqsToggle : IFeatureToggle
    {
        public bool FeatureEnabled { get; }

        public UseSqsToggle(bool featureEnabled)
        {
            FeatureEnabled = featureEnabled;
        }
    }

    public class FeatureToggleOptions
    {
        public const string ConfigurationKey = "FeatureToggles";
        public bool UseSqs { get; set; }
    }
}
