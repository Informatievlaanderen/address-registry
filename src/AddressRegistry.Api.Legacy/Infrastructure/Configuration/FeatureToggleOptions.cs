namespace AddressRegistry.Api.Legacy.Infrastructure.Configuration
{
    public class FeatureToggleOptions
    {
        public const string ConfigurationKey = "FeatureToggles";
        public bool UseProjectionsV2 { get; set; }
    }
}
