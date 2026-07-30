namespace AddressRegistry.Api.BackOffice.Abstractions
{
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;

    /// <summary>
    /// Indicates whether the event store persists geometry in Lambert 2008 (EPSG 3812) instead of Lambert 72 (EPSG 31370).
    /// Incoming positions are always converted to <see cref="EventStoreSrid"/>, regardless of the srsName they were sent with.
    /// </summary>
    public sealed class UseLambert2008EventStoreToggle
    {
        public bool FeatureEnabled { get; }

        public int EventStoreSrid => FeatureEnabled
            ? SystemReferenceId.SridLambert2008
            : SystemReferenceId.SridLambert72;

        public UseLambert2008EventStoreToggle(bool featureEnabled) => FeatureEnabled = featureEnabled;
    }
}
