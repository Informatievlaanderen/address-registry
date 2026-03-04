namespace AddressRegistry.Projections.Feed.AddressFeed
{
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using NetTopologySuite.Geometries;

    public static class GmlHelpers
    {
        public static Geometry ParseGeometry(string extendedWkbGeometryHex)
        {
            var reader = WKBReaderFactory.CreateForEwkbAsHex(extendedWkbGeometryHex);
            return reader.Read(extendedWkbGeometryHex.ToByteArray());
        }

        public static string ConvertToGml(string extendedWkbGeometryHex)
        {
            return ParseGeometry(extendedWkbGeometryHex).ConvertToGml();
        }
    }
}
