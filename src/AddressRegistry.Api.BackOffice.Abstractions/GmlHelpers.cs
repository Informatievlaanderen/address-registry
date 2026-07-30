namespace AddressRegistry.Api.BackOffice.Abstractions
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.Geometries.Implementation;
    using NetTopologySuite.IO.GML2;
    using StreetName;

    public static class GmlHelpers
    {
        public static GMLReader CreateGmlReader() => CreateGmlReader(ExtendedWkbGeometry.SridLambert72);

        public static GMLReader CreateGmlReader(int srid) =>
            new GMLReader(
                new GeometryFactory(
                    new PrecisionModel(PrecisionModels.Floating),
                    srid,
                    new DotSpatialAffineCoordinateSequenceFactory(Ordinates.XY)));

        /// <summary>
        /// Reads a GML string using the coordinate system of its own srsName attribute.
        /// </summary>
        public static Geometry ReadGeometry(this string gml)
        {
            if (!gml.TryReadSridGml(out var srid))
            {
                throw new InvalidOperationException($"Unsupported or missing srsName in GML.");
            }

            return CreateGmlReader(srid).Read(gml);
        }

        // TODO: the lambda still assumes the event store persists Lambert 72. Make this SRID aware
        // when the event store is migrated to Lambert 2008 (see UseLambert2008EventStoreToggle).
        public static ExtendedWkbGeometry ToExtendedWkbGeometry(this string gml)
        {
            var gmlReader = CreateGmlReader();
            var geometry = gmlReader.Read(gml);

            geometry.SRID = ExtendedWkbGeometry.SridLambert72;

            return new ExtendedWkbGeometry(geometry.AsBinary());
        }
    }
}
