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

        /// <summary>
        /// Reads a GML position in the reference system its own srsName declares, and persists it as EWKB
        /// carrying that SRID — so the event store records which reference system a position is in rather
        /// than leaving every reader to infer it.
        /// </summary>
        /// <remarks>
        /// It deliberately does not consult <c>UseLambert2008EventStoreToggle</c>: the BackOffice API
        /// normalizes every incoming position to the event store's reference system before the SQS message
        /// is created (ADR 0003), so the srsName arriving here is already the right one.
        ///
        /// What it must not do is what it did before — force-set the SRID to Lambert 72 — which silently
        /// relabelled a Lambert 2008 position rather than rejecting it, persisting coordinates ~500 km from
        /// where the address is. An unsupported or missing srsName throws in <see cref="ReadGeometry"/>.
        /// See ADR 0005.
        /// </remarks>
        public static ExtendedWkbGeometry ToExtendedWkbGeometry(this string gml)
            => ExtendedWkbGeometry.Create(gml.ReadGeometry());
    }
}
