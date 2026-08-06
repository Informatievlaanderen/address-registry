namespace AddressRegistry.Api.Oslo.Address.V2.Sync
{
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;

    /// <summary>
    /// The reference system the caller wants the embedded <c>object</c> of a syndication item in.
    /// Only the object honours this; the embedded <c>event</c> is always emitted exactly as the event store
    /// held it at that position. See ADR 0004.
    /// </summary>
    public static class ObjectCrs
    {
        private const string Lambert2008 = "3812";

        /// <summary>
        /// Resolves the requested reference system. Lambert 2008 is returned only for the exact value
        /// <c>3812</c>; anything else — an unrecognised value, an empty one, or no filter at all — resolves to
        /// Lambert 72, which keeps the feed's historical contract for every caller that does not ask.
        /// </summary>
        public static int ToSrid(string? objectCrs) =>
            objectCrs?.Trim() == Lambert2008
                ? SystemReferenceId.SridLambert2008
                : SystemReferenceId.SridLambert72;
    }
}
