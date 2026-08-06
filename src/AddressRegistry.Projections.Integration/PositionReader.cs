namespace AddressRegistry.Projections.Integration
{
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using NetTopologySuite.Geometries;

    public static class PositionReader
    {
        /// <summary>
        /// Reads a persisted position in the reference system its EWKB carries, rather than assuming one.
        /// The SRID travels into the PostGIS <c>geometry</c> column, so a row says which Lambert system it
        /// is in and <c>ST_SRID</c> can be branched on. See ADR 0004.
        /// </summary>
        public static Geometry ReadPosition(string extendedWkbGeometryHex)
        {
            var extendedWkb = extendedWkbGeometryHex.ToByteArray();

            return WKBReaderFactory.CreateForEwkb(extendedWkb).Read(extendedWkb);
        }
    }
}
