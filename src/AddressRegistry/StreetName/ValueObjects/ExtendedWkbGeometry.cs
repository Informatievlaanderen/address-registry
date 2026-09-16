namespace AddressRegistry.StreetName
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.GrAr.CrsTransform;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;

    public class ExtendedWkbGeometry : ByteArrayValueObject<ExtendedWkbGeometry>
    {
        private static readonly WKBWriter WkbWriter = new WKBWriter { Strict = false, HandleSRID = true };

        public const int SridLambert72 = SystemReferenceId.SridLambert72;

        /// <summary>
        /// The precision the event store holds positions at: centimetres. See ADR 0005.
        /// </summary>
        public const int CoordinateDecimals = 2;

        public ExtendedWkbGeometry(byte[] ewkbBytes) : base(ewkbBytes) { }

        public ExtendedWkbGeometry(string ewkbBytesHex) : base(ewkbBytesHex.ToByteArray()) { }

        public override string ToString() => Value.ToHexString();

        /// <summary>
        /// The EWKB as persisted. Readers take bytes, so this is what they get: going through
        /// <see cref="ToString"/> would allocate a hex string per address and parse it straight back.
        /// </summary>
        public byte[] ToByteArray() => Value;

        /// <summary>
        /// Wraps a geometry that has already been read and transformed, keeping the SRID it carries, and
        /// rounds it to <see cref="CoordinateDecimals"/> decimals. Every path that writes a new or corrected
        /// position goes through here, so this is what decides how a position is serialized.
        /// <see cref="CreateEWkb"/> is the exception: it re-serializes a position the event store already
        /// holds, and does not round.
        /// </summary>
        /// <remarks>
        /// The rounding is what keeps the event store at a single precision. Anything finer than a
        /// centimetre is below what a position is surveyed to, and it is not reproducible from what the API
        /// serves — every reader rounds to centimetres, so a caller who posts an unchanged position back
        /// would send a coarser one, and the aggregate, which compares the EWKB bytes, would read that as an
        /// edit and apply an event for a correction that corrected nothing.
        ///
        /// The geometry is copied first: <c>RoundCoordinates</c> rounds in place, and callers hand us
        /// geometries they still use.
        /// </remarks>
        public static ExtendedWkbGeometry Create(Geometry geometry)
            => new ExtendedWkbGeometry(WkbWriter.Write(geometry.Copy().RoundCoordinates(CoordinateDecimals)));

        public static ExtendedWkbGeometry? CreateEWkb(byte[]? wkb, int useSrid = SridLambert72)
        {
            if (wkb == null)
                return null;

            try
            {
                if (!wkb.TryReadSrid(out var srid))
                {
                    if (useSrid == SridLambert72)
                    {
                        var geometry = WKBReaderFactory.CreateForLambert72().Read(wkb);
                        return new ExtendedWkbGeometry(WkbWriter.Write(geometry));
                    }

                    if (useSrid == SystemReferenceId.SridLambert2008)
                    {
                        var geometry = WKBReaderFactory.CreateForLambert2008().Read(wkb);
                        return new ExtendedWkbGeometry(WkbWriter.Write(geometry));
                    }

                    return null;
                }

                if (srid != useSrid)
                    throw new InvalidOperationException("SRID in EWKB does not match the expected SRID.");

                var reader = WKBReaderFactory.CreateForEwkb(wkb);
                var ewkbGeometry = reader.Read(wkb);
                ewkbGeometry.SRID = srid;
                return new ExtendedWkbGeometry(WkbWriter.Write(ewkbGeometry));
            }
            catch (ParseException)
            {
                return null;
            }
            catch (ArgumentException)
            {
                return null;
            }
        }
    }
}
