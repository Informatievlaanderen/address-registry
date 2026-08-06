namespace AddressRegistry
{
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using NetTopologySuite.IO;
    using GrArWKBReaderFactory = Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology.WKBReaderFactory;

    // ReSharper disable once InconsistentNaming
    public static class WKBReaderFactory
    {
        public static WKBReader CreateForLegacy() =>
            GrArWKBReaderFactory.CreateForLambert72();

        public static WKBReader Create() =>
            GrArWKBReaderFactory.CreateForLambert72();

        public static WKBReader CreateForLambert2008() =>
            GrArWKBReaderFactory.CreateForLambert2008();

        /// <summary>
        /// Creates a reader for a persisted position, in the reference system the bytes themselves carry,
        /// so callers do not have to assume which one the event store writes.
        /// Positions persisted before the event store recorded an SRID are read as Lambert 72.
        /// </summary>
        public static WKBReader CreateForEwkb(byte[] ewkb) =>
            ewkb.TryReadSrid(out _)
                ? GrArWKBReaderFactory.CreateForEwkb(ewkb)
                : GrArWKBReaderFactory.CreateForLambert72();
    }
}
