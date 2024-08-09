namespace AddressRegistry.Projections.Elastic
{
    using System.Collections.Generic;
    using System.Linq;
    using NetTopologySuite.Geometries;
    using ProjNet.CoordinateSystems;
    using ProjNet.CoordinateSystems.Transformations;

    public static class CoordinateTransformer
    {
        // private const string Lambert72Wkt = "PROJCS[\"Belge_Lambert_1972\",GEOGCS[\"GCS_Belge_1972\",DATUM[\"D_Belge_1972\",SPHEROID[\"International_1924\",6378388.0,297.0]],PRIMEM[\"Greenwich\",0.0],UNIT[\"Degree\",0.0174532925199433]],PROJECTION[\"Lambert_Conformal_Conic\"],PARAMETER[\"False_Easting\",150000.013],PARAMETER[\"False_Northing\",5400088.438],PARAMETER[\"Central_Meridian\",4.36748666666667],PARAMETER[\"Standard_Parallel_1\",51.1666672333333],PARAMETER[\"Standard_Parallel_2\",49.8333339],PARAMETER[\"Latitude_Of_Origin\",90.0],UNIT[\"Meter\",1.0]]";
        private const string Lambert72Wkt = "PROJCS[\"Belge_Lambert_1972\",GEOGCS[\"GCS_Belge_1972\",DATUM[\"D_Belge_1972\",SPHEROID[\"International_1924\",6378388.0,297.0]],PRIMEM[\"Greenwich\",0.0],UNIT[\"Degree\",0.0174532925199433]],PROJECTION[\"Lambert_Conformal_Conic\"],PARAMETER[\"False_Easting\",150000.01256],PARAMETER[\"False_Northing\",5400088.4378],PARAMETER[\"Central_Meridian\",4.367486666666666],PARAMETER[\"Standard_Parallel_1\",49.8333339],PARAMETER[\"Standard_Parallel_2\",51.16666723333333],PARAMETER[\"Latitude_Of_Origin\",90.0],UNIT[\"Meter\",1.0]]";

        private static readonly CoordinateSystem Lambert72CoordinateSystem = new CoordinateSystemFactory().CreateFromWkt(Lambert72Wkt);

        public static Point FromLambert72ToWgs84(Point point)
        {
            var coordinateTransformationFactory = new CoordinateTransformationFactory();

            var coordinateTransformation = coordinateTransformationFactory.CreateFromCoordinateSystems(
                Lambert72CoordinateSystem,
                GeographicCoordinateSystem.WGS84);

            var coordinates = coordinateTransformation.MathTransform
                .TransformList(new List<double[]> { new[] { point.X, point.Y } })
                .ToArray();

            return new Point(coordinates[0][0], coordinates[0][1])
            {
                SRID = 4326
            };
        }
    }
}
