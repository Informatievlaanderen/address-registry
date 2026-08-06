namespace AddressRegistry.Projections.Elastic
{
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using NetTopologySuite.Geometries;

    public static class PositionExtensions
    {
        /// <summary>
        /// The position as EWKT, for example <c>SRID=31370;POINT (140252.76 198794.27)</c>.
        /// Plain WKT does not say which Lambert reference system the coordinates are in, and the index
        /// holds both for as long as the event store conversion is in flight — telling them apart by
        /// looking at the coordinates is not something a consumer should have to do. See ADR 0004.
        /// A position without an SRID is labelled Lambert 72, which is the same assumption
        /// <see cref="CoordinateTransformer.ToWgs84Text"/> makes about it.
        /// </summary>
        public static string ToEwkt(this Point point)
        {
            var srid = point.SRID == 0
                ? SystemReferenceId.SridLambert72
                : point.SRID;

            return $"SRID={srid};{point.AsText()}";
        }
    }
}
