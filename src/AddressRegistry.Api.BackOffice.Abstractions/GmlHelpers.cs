namespace AddressRegistry.Api.BackOffice.Abstractions
{
    using NetTopologySuite.Geometries;
    using NetTopologySuite.Geometries.Implementation;
    using NetTopologySuite.IO.GML2;
    using StreetName;

    public static class GmlHelpers
    {
        public static GMLReader CreateGmlReader() =>
            new GMLReader(
                new GeometryFactory(
                    new PrecisionModel(PrecisionModels.Floating),
                    ExtendedWkbGeometry.SridLambert72,
                    new DotSpatialAffineCoordinateSequenceFactory(Ordinates.XY)));

        public static ExtendedWkbGeometry ToExtendedWkbGeometry(this string gml)
        {
            var gmlReader = CreateGmlReader();
            var geometry = gmlReader.Read(gml);

            geometry.SRID = ExtendedWkbGeometry.SridLambert72;

            return new ExtendedWkbGeometry(geometry.AsBinary());
        }
    }
}
