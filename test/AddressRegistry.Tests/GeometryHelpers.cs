namespace AddressRegistry.Tests
{
    using Address;
    using NetTopologySuite.IO;

    public static class GeometryHelpers
    {
        public const string ValidGmlPolygon =
            "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\">" +
            "<gml:exterior>" +
            "<gml:LinearRing>" +
            "<gml:posList>" +
            "140284.15277253836 186724.74131567031 140291.06016454101 186726.38355567306 140288.22675654292 186738.25798767805 140281.19098053873 186736.57913967967 140284.15277253836 186724.74131567031" +
            "</gml:posList>" +
            "</gml:LinearRing>" +
            "</gml:exterior>" +
            "</gml:Polygon>";

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
