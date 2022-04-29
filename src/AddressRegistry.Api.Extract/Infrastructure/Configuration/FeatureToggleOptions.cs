namespace AddressRegistry.Api.Extract.Infrastructure.Configuration
{
    public class FeatureToggleOptions
    {
        public const string ConfigurationKey = "FeatureToggles";
        public bool UseExtractV2 { get; set; }
    }
}
