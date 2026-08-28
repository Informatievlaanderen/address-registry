namespace AddressRegistry.Migrator.Lambert2008.Infrastructure
{
    using System;

    /// <summary>
    /// What one street name stream cost. Load and dispatch are kept apart because they scale with
    /// different things — loading with the number of events in the stream, dispatching with the number of
    /// addresses converted — and a staging run is only extrapolatable to production if you can tell which
    /// of the two dominates.
    /// </summary>
    internal sealed record StreamConversionResult(
        int AddressCount,
        int ConvertedAddresses,
        TimeSpan LoadDuration,
        TimeSpan DispatchDuration)
    {
        public TimeSpan TotalDuration => LoadDuration + DispatchDuration;

        public static StreamConversionResult Skipped { get; } = new(0, 0, TimeSpan.Zero, TimeSpan.Zero);
    }
}
