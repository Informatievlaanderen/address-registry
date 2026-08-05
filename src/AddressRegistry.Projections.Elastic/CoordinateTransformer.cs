namespace AddressRegistry.Projections.Elastic
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.GrAr.CrsTransform;
    using NetTopologySuite.Geometries;
    using ProjNet.CoordinateSystems;
    using ProjNet.CoordinateSystems.Transformations;

    public static class CoordinateTransformer
    {
        private const string Lambert72Wkt = "PROJCS[\"Belge_Lambert_1972\",GEOGCS[\"GCS_Belge_1972\",DATUM[\"D_Belge_1972\",SPHEROID[\"International_1924\",6378388.0,297.0]],PRIMEM[\"Greenwich\",0.0],UNIT[\"Degree\",0.0174532925199433]],PROJECTION[\"Lambert_Conformal_Conic\"],PARAMETER[\"False_Easting\",150000.01256],PARAMETER[\"False_Northing\",5400088.4378],PARAMETER[\"Central_Meridian\",4.367486666666666],PARAMETER[\"Standard_Parallel_1\",49.8333339],PARAMETER[\"Standard_Parallel_2\",51.16666723333333],PARAMETER[\"Latitude_Of_Origin\",90.0],UNIT[\"Meter\",1.0]]";
        private const string Lambert2008Wkt = "PROJCS[\"Belge_Lambert_2008\",GEOGCS[\"GCS_ETRS_1989\",DATUM[\"D_ETRS_1989\",SPHEROID[\"GRS_1980\",6378137.0,298.257222101]],PRIMEM[\"Greenwich\",0.0],UNIT[\"Degree\",0.0174532925199433]],PROJECTION[\"Lambert_Conformal_Conic\"],PARAMETER[\"False_Easting\",649328.0],PARAMETER[\"False_Northing\",665262.0],PARAMETER[\"Central_Meridian\",4.359215833333333],PARAMETER[\"Standard_Parallel_1\",49.83333333333334],PARAMETER[\"Standard_Parallel_2\",51.16666666666666],PARAMETER[\"Latitude_Of_Origin\",50.797815],UNIT[\"Meter\",1.0]]";

        private static readonly CoordinateSystem Lambert72CoordinateSystem = new CoordinateSystemFactory().CreateFromWkt(Lambert72Wkt);
        private static readonly CoordinateSystem Lambert2008CoordinateSystem = new CoordinateSystemFactory().CreateFromWkt(Lambert2008Wkt);

        /// <summary>
        /// Converts a position to WGS84 from the reference system it is persisted in. Lambert 2008 is
        /// projected directly rather than transformed back to Lambert 72 first, because it loses nothing
        /// on the way: it is already on ETRS89. A position with an unknown SRID is read as Lambert 72,
        /// the same assumption made everywhere else. See ADR 0004.
        /// </summary>
        public static string ToWgs84Text(Point point)
            => point.IsLambert08()
                ? ToWgs84Text(point, Lambert2008CoordinateSystem)
                : ToWgs84Text(point, Lambert72CoordinateSystem);

        private static string ToWgs84Text(Point point, CoordinateSystem sourceCoordinateSystem)
        {
            var coordinateTransformationFactory = new CoordinateTransformationFactory();

            var coordinateTransformation = coordinateTransformationFactory.CreateFromCoordinateSystems(
                sourceCoordinateSystem,
                GeographicCoordinateSystem.WGS84);

            var coordinates = coordinateTransformation.MathTransform
                .TransformList(new List<double[]> { new[] { point.X, point.Y } })
                .ToArray();

            var pointAsWgs84 = new Point(coordinates[0][0], coordinates[0][1])
            {
                SRID = 4326
            };

            return string.Format(CultureInfo.InvariantCulture, "{0}, {1}", pointAsWgs84.X, pointAsWgs84.Y);
        }
    }
}
