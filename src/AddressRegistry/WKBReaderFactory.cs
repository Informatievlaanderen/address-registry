namespace AddressRegistry
{
    using Address.ValueObjects;
    using NetTopologySuite;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.Geometries.Implementation;
    using NetTopologySuite.IO;

    public static class WKBReaderFactory
    {
        public static WKBReader CreateForLegacy() =>
            new WKBReader(
                new NtsGeometryServices(
                    new DotSpatialAffineCoordinateSequenceFactory(Ordinates.XY),
                    new PrecisionModel(PrecisionModels.Floating),
                    SpatialReferenceSystemId.Lambert72));
    }
}