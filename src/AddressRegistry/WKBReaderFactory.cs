namespace AddressRegistry
{
    using Address;
    using NetTopologySuite;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.Geometries.Implementation;
    using NetTopologySuite.IO;

    // ReSharper disable once InconsistentNaming
    public static class WKBReaderFactory
    {
        public static WKBReader CreateForLegacy() =>
            Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology.WKBReaderFactory.CreateForLambert72();

        public static WKBReader Create() =>
            Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology.WKBReaderFactory.CreateForLambert72();

        public static WKBReader CreateForLambert2008() =>
            Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology.WKBReaderFactory.CreateForLambert2008();
    }
}
