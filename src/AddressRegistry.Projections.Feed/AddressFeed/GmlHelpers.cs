namespace AddressRegistry.Projections.Feed.AddressFeed
{
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using NetTopologySuite.Geometries;
    using StreetName;

    public static class GmlHelpers
    {
        public static Geometry ParseGeometry(string extendedWkbGeometryHex)
        {
            if (!extendedWkbGeometryHex.TryReadSrid(out var srid))
            {
                var newExtended = ExtendedWkbGeometry.CreateEWkb(extendedWkbGeometryHex.ToByteArray());
                extendedWkbGeometryHex = newExtended.ToString();
            }

            var reader = WKBReaderFactory.CreateForEwkbAsHex(extendedWkbGeometryHex);
            return reader.Read(extendedWkbGeometryHex.ToByteArray());
        }
    }
}
