namespace AddressRegistry.Api.BackOffice.Infrastructure.Configuration
{
    public sealed class FeatureToggleOptions
    {
        public const string ConfigurationKey = "FeatureToggles";

        /// <summary>
        /// When enabled, the event store persists geometry in Lambert 2008 (EPSG 3812) instead of Lambert 72 (EPSG 31370).
        /// </summary>
        public bool UseLambert2008EventStore { get; set; }
    }
}
