namespace AddressRegistry.Tests
{
    using Address;
    using NetTopologySuite.IO;

    public static class GeometryHelpers
    {
        private static readonly WKBWriter WkbWriter = new WKBWriter { Strict = false, HandleSRID = true };
        public static byte[] ExampleWkb { get; }
        public static byte[] ExampleExtendedWkb { get; }

        static GeometryHelpers()
        {
            var point = "POINT (141299 185188)";
            var geometry = new WKTReader { DefaultSRID = SpatialReferenceSystemId.Lambert72 }.Read(point);
            ExampleWkb = geometry.AsBinary();
            geometry.SRID = SpatialReferenceSystemId.Lambert72;
            ExampleExtendedWkb = WkbWriter.Write(geometry);
        }

        public static ExtendedWkbGeometry CreateEwkbFrom(WkbGeometry wkbGeometry)
        {
            var reader = new WKBReader();
            var geometry = reader.Read(wkbGeometry);
            geometry.SRID = SpatialReferenceSystemId.Lambert72;
            return new ExtendedWkbGeometry(WkbWriter.Write(geometry));
        }

        public static WkbGeometry CreateFromWkt(string wkt)
        {
            var geometry = new WKTReader { DefaultSRID = SpatialReferenceSystemId.Lambert72 }.Read(wkt);
            return new WkbGeometry(WkbWriter.Write(geometry));
        }
    }
}
